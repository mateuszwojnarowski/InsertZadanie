using Microsoft.EntityFrameworkCore;
using RatesService.Data;
using RatesService.Services;
using Shared.Models;

namespace RatesService.Workers;

public class RateFetchWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RateFetchWorker> _logger;

    public RateFetchWorker(IServiceProvider serviceProvider, ILogger<RateFetchWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("RateFetchWorker started");

        await FetchAndStoreRatesAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            // NBP aktualizuje Tabelę B tylko w środy około 11:45-12:15
            // jednak dla uproszczenia, delay jest ustawiony na sprawdzanie co godzinę.
            var delay = TimeSpan.FromHours(1);

            _logger.LogInformation("Next rate check in {Delay}", delay);
            await Task.Delay(delay, stoppingToken);

            if (!stoppingToken.IsCancellationRequested)
            {
                await FetchAndStoreRatesAsync(stoppingToken);
            }
        }
    }

    private async Task FetchAndStoreRatesAsync(CancellationToken ct)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var nbpClient = scope.ServiceProvider.GetRequiredService<INbpApiClient>();
            var dbContext = scope.ServiceProvider.GetRequiredService<RatesDbContext>();
            var publisher = scope.ServiceProvider.GetRequiredService<IRatePublisher>();

            var table = await nbpClient.FetchLatestTableBAsync(ct);
            if (table == null)
            {
                _logger.LogWarning("No table returned from NBP API");
                return;
            }

            var existing = await dbContext.ExchangeRateTables
                .FirstOrDefaultAsync(t => t.TableNo == table.TableNo, ct);

            if (existing != null)
            {
                _logger.LogInformation("Table {TableNo} already exists, skipping", table.TableNo);
                return;
            }

            await dbContext.ExchangeRateTables.AddAsync(table, ct);
            await dbContext.SaveChangesAsync(ct);

            _logger.LogInformation(
                "Saved table {TableNo} with {Count} rates (effective: {Date})",
                table.TableNo, table.Rates.Count, table.EffectiveDate);

            var ratesEvent = new RatesUpdatedEvent(
                table.TableNo,
                table.EffectiveDate,
                table.Rates.Select(r => new CurrencyRateDto(r.Currency, r.Code, r.Mid)).ToList()
            );

            await publisher.PublishRatesUpdatedAsync(ratesEvent, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in FetchAndStoreRatesAsync");
        }
    }
}
