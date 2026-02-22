namespace WalletService.Services;

public interface IExchangeService
{
    Task<ExchangeResult> ConvertAsync(string fromCurrency, string toCurrency, decimal amount);
    Task<bool> IsCurrencyPairValidAsync(string fromCurrency, string toCurrency);
}

public record ExchangeResult(
    decimal SourceAmount,
    string SourceCurrency,
    decimal TargetAmount,
    string TargetCurrency,
    decimal EffectiveRate
);
