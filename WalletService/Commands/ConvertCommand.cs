using MediatR;
using WalletService.Services;

namespace WalletService.Commands;

public record ConvertCommand(Guid WalletId, string FromCurrency, string ToCurrency, decimal Amount) : IRequest<ConvertResult>;

public record ConvertResult(
    bool Success,
    string? Error = null,
    string? FromCurrency = null,
    decimal? FromAmount = null,
    decimal? FromNewBalance = null,
    string? ToCurrency = null,
    decimal? ToAmount = null,
    decimal? ToNewBalance = null,
    decimal? ExchangeRate = null
);
