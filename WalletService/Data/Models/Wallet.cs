namespace WalletService.Models;

public class Wallet
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<WalletBalance> Balances { get; set; } = new();
}
