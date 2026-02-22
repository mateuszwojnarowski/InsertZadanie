using Azure.Messaging.ServiceBus;
using Shared.Models;
using System.Text.Json;

namespace RatesService.Services;

public class RatePublisher : IRatePublisher
{
    private readonly ServiceBusSender _sender;
    private readonly ILogger<RatePublisher> _logger;

    public RatePublisher(ServiceBusClient serviceBusClient, ILogger<RatePublisher> logger)
    {
        _sender = serviceBusClient.CreateSender("rates-updated");
        _logger = logger;
    }

    public async Task PublishRatesUpdatedAsync(RatesUpdatedEvent ratesEvent, CancellationToken ct = default)
    {
        try
        {
            var json = JsonSerializer.Serialize(ratesEvent);
            var message = new ServiceBusMessage(json)
            {
                ContentType = "application/json",
                Subject = "RatesUpdated"
            };

            await _sender.SendMessageAsync(message, ct);
            _logger.LogInformation(
                "Published RatesUpdatedEvent for table {TableNo} with {Count} rates",
                ratesEvent.TableNo, ratesEvent.Rates.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish RatesUpdatedEvent");
        }
    }
}