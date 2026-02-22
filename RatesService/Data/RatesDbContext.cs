using Microsoft.EntityFrameworkCore;
using RatesService.Data.Models;

namespace RatesService.Data;

public class RatesDbContext : DbContext
{
    public DbSet<ExchangeRateTable> ExchangeRateTables { get; set; } = null!;

    public RatesDbContext(DbContextOptions<RatesDbContext> options) : base(options)
    {
    }
}
