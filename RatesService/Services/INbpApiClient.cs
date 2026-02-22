using RatesService.Data.Models;

namespace RatesService.Services;

public interface INbpApiClient
{
    Task<ExchangeRateTable?> FetchLatestTableBAsync(CancellationToken cancellationToken);
}
