using MediatR;

namespace WalletService.Commands;

public record DepositCommand(Guid WalletId, string CurrencyCode, decimal Amount) : IRequest<DepositResult>;

public record DepositResult(bool Success, string? Error = null, string? CurrencyCode = null, decimal? NewBalance = null);
