using System.Text.Json;
using Shared.Models;
using StackExchange.Redis;

namespace WalletService.Services;

public class RateCacheService : IRateCacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<RateCacheService> _logger;
    private const string RatesHashKey = "rates:latest";
    private static readonly TimeSpan CacheExpiry = TimeSpan.FromHours(2);

    public RateCacheService(
        IConnectionMultiplexer redis,
        IHttpClientFactory httpClientFactory,
        ILogger<RateCacheService> logger)
    {
        _redis = redis;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task SetRatesAsync(List<CurrencyRateDto> rates)
    {
        var db = _redis.GetDatabase();
        var entries = rates.Select(r =>
            new HashEntry(r.Code, r.Mid.ToString("G"))).ToArray();

        await db.HashSetAsync(RatesHashKey, entries);

        await db.HashSetAsync(RatesHashKey, new[]
        {
            new HashEntry("__updated_at", DateTime.UtcNow.ToString("O")),
            new HashEntry("__count", rates.Count.ToString())
        });

        // Set TTL
        await db.KeyExpireAsync(RatesHashKey, CacheExpiry);

        _logger.LogInformation("Cached {Count} rates in Redis with TTL {Ttl}", rates.Count, CacheExpiry);
    }

    public async Task<decimal?> GetRateAsync(string currencyCode)
    {
        var db = _redis.GetDatabase();
        var value = await db.HashGetAsync(RatesHashKey, currencyCode);

        if (value.IsNullOrEmpty)
        {
            if (await FallbackFetchRatesAsync())
            {
                value = await db.HashGetAsync(RatesHashKey, currencyCode);
            }
        }

        if (value.IsNullOrEmpty)
            return null;

        return decimal.Parse(value!);
    }

    public async Task<Dictionary<string, decimal>> GetAllRatesAsync()
    {
        var db = _redis.GetDatabase();
        var entries = await db.HashGetAllAsync(RatesHashKey);

        if (entries.Length == 0)
        {
            if (await FallbackFetchRatesAsync())
            {
                entries = await db.HashGetAllAsync(RatesHashKey);
            }
        }

        var result = new Dictionary<string, decimal>();
        foreach (var entry in entries)
        {
            var key = entry.Name.ToString();
            if (key.StartsWith("__")) continue;

            if (decimal.TryParse(entry.Value.ToString(), out var rate))
            {
                result[key] = rate;
            }
        }

        return result;
    }

    public async Task<bool> IsCurrencyAvailableAsync(string currencyCode)
    {
        if (currencyCode == "PLN")
            return true;

        var rate = await GetRateAsync(currencyCode);
        return rate.HasValue;
    }

    private async Task<bool> FallbackFetchRatesAsync()
    {
        try
        {
            _logger.LogWarning("Redis cache empty or expired. Attempting fallback HTTP fetch from RatesService...");
            var client = _httpClientFactory.CreateClient("RatesService");
            var response = await client.GetAsync("/api/rates/latest");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Fallback fetch failed. RatesService returned {Status}", response.StatusCode);
                return false;
            }

            var content = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;
            
            if (!root.TryGetProperty("rates", out var ratesElement))
            {
                 _logger.LogWarning("Fallback fetch returned invalid JSON format (no 'rates' property)");
                 return false;
            }

            var dtos = new List<CurrencyRateDto>();
            foreach (var r in ratesElement.EnumerateArray())
            {
                var code = r.GetProperty("code").GetString();
                var currency = r.GetProperty("currency").GetString();
                var mid = r.GetProperty("mid").GetDecimal();
                if (code != null && currency != null)
                {
                    dtos.Add(new CurrencyRateDto(currency, code, mid));
                }
            }

            if (dtos.Any())
            {
                await SetRatesAsync(dtos);
                _logger.LogInformation("Fallback fetch successful. Restored {Count} rates to Redis.", dtos.Count);
                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during fallback fetch from RatesService");
        }

        return false;
    }
}
