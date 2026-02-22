namespace Shared.Models;
public record CurrencyRateDto(
    string Currency,
    string Code,
    decimal Mid
);