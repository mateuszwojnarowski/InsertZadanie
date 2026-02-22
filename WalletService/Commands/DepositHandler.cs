using MediatR;
using Microsoft.EntityFrameworkCore;
using WalletService.Data;
using WalletService.Models;
using WalletService.Services;

namespace WalletService.Commands;

public class DepositHandler : IRequestHandler<DepositCommand, DepositResult>
{
    private readonly WalletDbContext _dbContext;
    private readonly IRateCacheService _rateCacheService;

    public DepositHandler(WalletDbContext dbContext, IRateCacheService rateCacheService)
    {
        _dbContext = dbContext;
        _rateCacheService = rateCacheService;
    }

    public async Task<DepositResult> Handle(DepositCommand request, CancellationToken cancellationToken)
    {
        if (!await _rateCacheService.IsCurrencyAvailableAsync(request.CurrencyCode))
            return new DepositResult(false, Error: $"Currency {request.CurrencyCode} is not available in NBP Table B");

        var wallet = await _dbContext.Wallets
            .Include(w => w.Balances)
            .FirstOrDefaultAsync(w => w.Id == request.WalletId, cancellationToken);

        if (wallet == null)
            return new DepositResult(false, Error: $"Wallet {request.WalletId} not found");

        var balance = wallet.Balances.FirstOrDefault(b => b.CurrencyCode == request.CurrencyCode);
        if (balance == null)
        {
            balance = new WalletBalance
            {
                WalletId = request.WalletId,
                CurrencyCode = request.CurrencyCode,
                Amount = request.Amount
            };
            _dbContext.WalletBalances.Add(balance);
        }
        else
        {
            balance.Amount += request.Amount;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new DepositResult(true, CurrencyCode: balance.CurrencyCode, NewBalance: balance.Amount);
    }
}
