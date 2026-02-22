namespace WalletService.Services;

public class ExchangeService : IExchangeService
{
    private readonly IRateCacheService _rateCacheService;
    private readonly ILogger<ExchangeService> _logger;

    public ExchangeService(IRateCacheService rateCacheService, ILogger<ExchangeService> logger)
    {
        _rateCacheService = rateCacheService;
        _logger = logger;
    }
    
    public async Task<ExchangeResult> ConvertAsync(string fromCurrency, string toCurrency, decimal amount)
    {
        if (fromCurrency == toCurrency)
            throw new InvalidOperationException("Source and target currencies must be different");

        decimal amountInPln;
        decimal targetAmount;

        if (fromCurrency == "PLN")
        {
            var toRate = await _rateCacheService.GetRateAsync(toCurrency)
                ?? throw new InvalidOperationException($"Exchange rate for {toCurrency} not found in cache");

            amountInPln = amount;
            targetAmount = amount / toRate;
        }
        else if (toCurrency == "PLN")
        {
            var fromRate = await _rateCacheService.GetRateAsync(fromCurrency)
                ?? throw new InvalidOperationException($"Exchange rate for {fromCurrency} not found in cache");

            amountInPln = amount * fromRate;
            targetAmount = amountInPln;
        }
        else
        {
            var fromRate = await _rateCacheService.GetRateAsync(fromCurrency)
                ?? throw new InvalidOperationException($"Exchange rate for {fromCurrency} not found in cache");

            var toRate = await _rateCacheService.GetRateAsync(toCurrency)
                ?? throw new InvalidOperationException($"Exchange rate for {toCurrency} not found in cache");

            amountInPln = amount * fromRate;
            targetAmount = amountInPln / toRate;
        }

        var effectiveRate = targetAmount / amount;

        _logger.LogInformation(
            "Converted {Amount} {From} -> {Result} {To} (rate: {Rate})",
            amount, fromCurrency, targetAmount, toCurrency, effectiveRate);

        return new ExchangeResult(amount, fromCurrency, targetAmount, toCurrency, effectiveRate);
    }

    public async Task<bool> IsCurrencyPairValidAsync(string fromCurrency, string toCurrency)
    {
        var fromValid = await _rateCacheService.IsCurrencyAvailableAsync(fromCurrency);
        var toValid = await _rateCacheService.IsCurrencyAvailableAsync(toCurrency);
        return fromValid && toValid;
    }
}
