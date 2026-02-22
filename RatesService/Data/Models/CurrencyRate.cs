namespace RatesService.Data.Models;

public class CurrencyRate
{
    public string Currency { get; set; } = null!;
    public string Code { get; set; } = null!;
    public decimal Mid { get; set; }
}
