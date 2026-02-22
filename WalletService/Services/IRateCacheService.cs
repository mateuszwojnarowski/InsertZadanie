using Shared.Models;

namespace WalletService.Services;

public interface IRateCacheService
{
    Task SetRatesAsync(List<CurrencyRateDto> rates);
    Task<decimal?> GetRateAsync(string currencyCode);
    Task<Dictionary<string, decimal>> GetAllRatesAsync();
    Task<bool> IsCurrencyAvailableAsync(string currencyCode);
}
