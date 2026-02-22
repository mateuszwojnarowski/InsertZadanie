using MediatR;
using Microsoft.EntityFrameworkCore;
using WalletService.DTOs;
using WalletService.Data;

namespace WalletService.Queries;

public class GetAllWalletsHandler : IRequestHandler<GetAllWalletsQuery, List<WalletResponse>>
{
    private readonly WalletDbContext _dbContext;

    public GetAllWalletsHandler(WalletDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<WalletResponse>> Handle(GetAllWalletsQuery request, CancellationToken cancellationToken)
    {
        var wallets = await _dbContext.Wallets
            .Include(w => w.Balances)
            .OrderBy(w => w.CreatedAt)
            .ToListAsync(cancellationToken);

        return wallets.Select(w => new WalletResponse(
            w.Id, w.Name, w.CreatedAt,
            w.Balances.Select(b => new BalanceResponse(b.CurrencyCode, b.Amount)).ToList()
        )).ToList();
    }
}
