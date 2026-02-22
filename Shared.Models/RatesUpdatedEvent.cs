namespace Shared.Models;

public record RatesUpdatedEvent(
    string TableNo,
    DateTime EffectiveDate,
    List<CurrencyRateDto> Rates
);