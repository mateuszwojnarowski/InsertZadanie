using MediatR;
using WalletService.DTOs;
using WalletService.Data;
using WalletService.Models;

namespace WalletService.Commands;

public class CreateWalletHandler : IRequestHandler<CreateWalletCommand, WalletResponse>
{
    private readonly WalletDbContext _dbContext;

    public CreateWalletHandler(WalletDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<WalletResponse> Handle(CreateWalletCommand request, CancellationToken cancellationToken)
    {
        var wallet = new Wallet { Name = request.Name };
        _dbContext.Wallets.Add(wallet);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new WalletResponse(wallet.Id, wallet.Name, wallet.CreatedAt, new List<BalanceResponse>());
    }
}
