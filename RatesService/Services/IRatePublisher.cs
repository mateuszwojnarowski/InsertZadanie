using Shared.Models;

namespace RatesService.Services;

public interface IRatePublisher
{
    Task PublishRatesUpdatedAsync(RatesUpdatedEvent ratesEvent, CancellationToken ct = default);
}
