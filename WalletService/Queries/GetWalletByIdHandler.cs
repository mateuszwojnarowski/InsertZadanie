using MediatR;
using Microsoft.EntityFrameworkCore;
using WalletService.DTOs;
using WalletService.Data;

namespace WalletService.Queries;

public class GetWalletByIdHandler : IRequestHandler<GetWalletByIdQuery, WalletResponse?>
{
    private readonly WalletDbContext _dbContext;

    public GetWalletByIdHandler(WalletDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<WalletResponse?> Handle(GetWalletByIdQuery request, CancellationToken cancellationToken)
    {
        var wallet = await _dbContext.Wallets
            .Include(w => w.Balances)
            .FirstOrDefaultAsync(w => w.Id == request.Id, cancellationToken);

        if (wallet == null)
            return null;

        return new WalletResponse(
            wallet.Id, wallet.Name, wallet.CreatedAt,
            wallet.Balances.Select(b => new BalanceResponse(b.CurrencyCode, b.Amount)).ToList()
        );
    }
}
