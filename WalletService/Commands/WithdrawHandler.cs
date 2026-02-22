using MediatR;
using Microsoft.EntityFrameworkCore;
using WalletService.Data;
using WalletService.Services;

namespace WalletService.Commands;

public class WithdrawHandler : IRequestHandler<WithdrawCommand, WithdrawResult>
{
    private readonly WalletDbContext _dbContext;
    private readonly IRateCacheService _rateCacheService;

    public WithdrawHandler(WalletDbContext dbContext, IRateCacheService rateCacheService)
    {
        _dbContext = dbContext;
        _rateCacheService = rateCacheService;
    }

    public async Task<WithdrawResult> Handle(WithdrawCommand request, CancellationToken cancellationToken)
    {
        if (!await _rateCacheService.IsCurrencyAvailableAsync(request.CurrencyCode))
            return new WithdrawResult(false, Error: $"Currency {request.CurrencyCode} is not available in NBP Table B");

        var wallet = await _dbContext.Wallets
            .Include(w => w.Balances)
            .FirstOrDefaultAsync(w => w.Id == request.WalletId, cancellationToken);

        if (wallet == null)
            return new WithdrawResult(false, Error: $"Wallet {request.WalletId} not found");

        var balance = wallet.Balances.FirstOrDefault(b => b.CurrencyCode == request.CurrencyCode);
        if (balance == null || balance.Amount < request.Amount)
            return new WithdrawResult(false, Error: $"Insufficient {request.CurrencyCode} balance. Available: {balance?.Amount ?? 0}");

        balance.Amount -= request.Amount;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new WithdrawResult(true, CurrencyCode: balance.CurrencyCode, NewBalance: balance.Amount);
    }
}
