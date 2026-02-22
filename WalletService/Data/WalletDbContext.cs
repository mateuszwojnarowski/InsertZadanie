using Microsoft.EntityFrameworkCore;
using WalletService.Models;

namespace WalletService.Data;

public class WalletDbContext : DbContext
{
    public DbSet<Wallet> Wallets { get; set; } = null!;
    public DbSet<WalletBalance> WalletBalances { get; set; } = null!;

    public WalletDbContext(DbContextOptions<WalletDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Wallet>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasMany(e => e.Balances)
                .WithOne(b => b.Wallet)
                .HasForeignKey(b => b.WalletId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<WalletBalance>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CurrencyCode).IsRequired().HasMaxLength(10);
            entity.Property(e => e.Amount).HasPrecision(18, 6);

            entity.HasIndex(e => new { e.WalletId, e.CurrencyCode }).IsUnique();
        });
    }
}
