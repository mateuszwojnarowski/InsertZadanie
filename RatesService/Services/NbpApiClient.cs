using Polly;
using Polly.Retry;
using RatesService.Data.Models;
using System.Text.Json;

namespace RatesService.Services;

public class NbpApiClient : INbpApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<NbpApiClient> _logger;
    private readonly ResiliencePipeline _retryPipeline;

    public NbpApiClient(HttpClient httpClient, ILogger<NbpApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        _retryPipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(2),
                BackoffType = DelayBackoffType.Exponential,
                OnRetry = args =>
                {
                    _logger.LogWarning("Retrying NBP API call, attempt {Attempt}", args.AttemptNumber + 1);
                    return ValueTask.CompletedTask;
                }
            })
            .Build();
    }

    public async Task<ExchangeRateTable?> FetchLatestTableBAsync(CancellationToken ct = default)
    {
        try
        {
            return await _retryPipeline.ExecuteAsync(async token =>
            {
                _logger.LogInformation("Fetching latest Table B from NBP API...");

                var response = await _httpClient.GetAsync(
                    "/api/exchangerates/tables/B/?format=json", token);

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("NBP API returned 404 - no table available");
                    return null;
                }

                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync(token);
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (root.ValueKind != JsonValueKind.Array || root.GetArrayLength() == 0)
                    return null;

                var table = root[0];
                var no = table.GetProperty("no").GetString();
                var effectiveDateStr = table.GetProperty("effectiveDate").GetString();

                var ratesElement = table.GetProperty("rates");
                var ratesList = new List<CurrencyRate>();

                foreach (var r in ratesElement.EnumerateArray())
                {
                    var code = r.GetProperty("code").GetString();
                    var currency = r.GetProperty("currency").GetString();
                    var mid = r.GetProperty("mid").GetDecimal();

                    if (code != null && currency != null)
                    {
                        ratesList.Add(new CurrencyRate
                        {
                            Code = code,
                            Currency = currency,
                            Mid = mid
                        });
                    }
                }

                return new ExchangeRateTable
                {
                    TableNo = no,
                    EffectiveDate = DateTime.Parse(effectiveDateStr).ToUniversalTime(),
                    Rates = ratesList
                };
            }, ct);
        }
        catch (Exception ex)
        
        {
            _logger.LogError(ex, "Failed to fetch Table B from NBP API");
            return null;
        }
    }
}
