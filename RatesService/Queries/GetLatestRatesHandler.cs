using MediatR;
using Microsoft.EntityFrameworkCore;
using RatesService.Data;
using RatesService.Data.Models;

namespace RatesService.Queries;

public class GetLatestRatesHandler : IRequestHandler<GetLatestRatesQuery, ExchangeRateTable?>
{
    private readonly RatesDbContext _dbContext;

    public GetLatestRatesHandler(RatesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ExchangeRateTable?> Handle(GetLatestRatesQuery request, CancellationToken cancellationToken)
    {
        return await _dbContext.ExchangeRateTables
            .OrderByDescending(t => t.EffectiveDate)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
