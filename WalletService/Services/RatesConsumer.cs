using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Shared.Models;

namespace WalletService.Services;

public class RatesConsumer : BackgroundService
{
    private readonly ServiceBusProcessor _processor;
    private readonly IRateCacheService _rateCacheService;
    private readonly ILogger<RatesConsumer> _logger;

    public RatesConsumer(
        ServiceBusClient serviceBusClient,
        IRateCacheService rateCacheService,
        ILogger<RatesConsumer> logger)
    {
        _rateCacheService = rateCacheService;
        _logger = logger;

        _processor = serviceBusClient.CreateProcessor(
            "rates-updated",
            "wallet-service",
            new ServiceBusProcessorOptions
            {
                AutoCompleteMessages = false,
                MaxConcurrentCalls = 1
            });
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _processor.ProcessMessageAsync += ProcessMessageAsync;
        _processor.ProcessErrorAsync += ProcessErrorAsync;

        await _processor.StartProcessingAsync(stoppingToken);
        _logger.LogInformation("RatesConsumer started listening on topic 'rates-updated', subscription 'wallet-service'");

        await Task.Delay(Timeout.Infinite, stoppingToken).ContinueWith(_ => { });

        await _processor.StopProcessingAsync();
    }

    private async Task ProcessMessageAsync(ProcessMessageEventArgs args)
    {
        try
        {
            var body = args.Message.Body.ToString();
            var ratesEvent = JsonSerializer.Deserialize<RatesUpdatedEvent>(body);

            if (ratesEvent == null)
            {
                _logger.LogWarning("Received null RatesUpdatedEvent");
                await args.CompleteMessageAsync(args.Message);
                return;
            }

            _logger.LogInformation(
                "Received RatesUpdatedEvent: table {TableNo}, {Count} rates",
                ratesEvent.TableNo, ratesEvent.Rates.Count);

            await _rateCacheService.SetRatesAsync(ratesEvent.Rates);

            await args.CompleteMessageAsync(args.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing RatesUpdatedEvent");
        }
    }

    private Task ProcessErrorAsync(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception,
            "ServiceBus processor error. Source: {Source}",
            args.ErrorSource);
        return Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _processor.DisposeAsync();
        await base.StopAsync(cancellationToken);
    }
}
