using MediatR;
using Microsoft.EntityFrameworkCore;
using WalletService.Data;
using WalletService.Models;
using WalletService.Services;

namespace WalletService.Commands;

public class ConvertHandler : IRequestHandler<ConvertCommand, ConvertResult>
{
    private readonly WalletDbContext _dbContext;
    private readonly IExchangeService _exchangeService;

    public ConvertHandler(WalletDbContext dbContext, IExchangeService exchangeService)
    {
        _dbContext = dbContext;
        _exchangeService = exchangeService;
    }

    public async Task<ConvertResult> Handle(ConvertCommand request, CancellationToken cancellationToken)
    {
        if (!await _exchangeService.IsCurrencyPairValidAsync(request.FromCurrency, request.ToCurrency))
            return new ConvertResult(false, Error: $"One or both currencies ({request.FromCurrency}, {request.ToCurrency}) are not available in NBP Table B");

        var wallet = await _dbContext.Wallets
            .Include(w => w.Balances)
            .FirstOrDefaultAsync(w => w.Id == request.WalletId, cancellationToken);

        if (wallet == null)
            return new ConvertResult(false, Error: $"Wallet {request.WalletId} not found");

        var fromBalance = wallet.Balances.FirstOrDefault(b => b.CurrencyCode == request.FromCurrency);
        if (fromBalance == null || fromBalance.Amount < request.Amount)
            return new ConvertResult(false, Error: $"Insufficient {request.FromCurrency} balance. Available: {fromBalance?.Amount ?? 0}");

        ExchangeResult exchangeResult;
        try
        {
            exchangeResult = await _exchangeService.ConvertAsync(request.FromCurrency, request.ToCurrency, request.Amount);
        }
        catch (InvalidOperationException ex)
        {
            return new ConvertResult(false, Error: ex.Message);
        }

        // Update balances
        fromBalance.Amount -= request.Amount;

        var toBalance = wallet.Balances.FirstOrDefault(b => b.CurrencyCode == request.ToCurrency);
        if (toBalance == null)
        {
            toBalance = new WalletBalance
            {
                WalletId = request.WalletId,
                CurrencyCode = request.ToCurrency,
                Amount = exchangeResult.TargetAmount
            };
            _dbContext.WalletBalances.Add(toBalance);
        }
        else
        {
            toBalance.Amount += exchangeResult.TargetAmount;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ConvertResult(
            true,
            FromCurrency: request.FromCurrency,
            FromAmount: request.Amount,
            FromNewBalance: fromBalance.Amount,
            ToCurrency: request.ToCurrency,
            ToAmount: exchangeResult.TargetAmount,
            ToNewBalance: toBalance.Amount,
            ExchangeRate: exchangeResult.EffectiveRate
        );
    }
}
