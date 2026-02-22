namespace WalletService.Models;

public class WalletBalance
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WalletId { get; set; }
    public string CurrencyCode { get; set; } = null!;
    public decimal Amount { get; set; }

    public Wallet Wallet { get; set; } = null!;
}
