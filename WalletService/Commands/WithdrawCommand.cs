using MediatR;

namespace WalletService.Commands;

public record WithdrawCommand(Guid WalletId, string CurrencyCode, decimal Amount) : IRequest<WithdrawResult>;

public record WithdrawResult(bool Success, string? Error = null, string? CurrencyCode = null, decimal? NewBalance = null);
