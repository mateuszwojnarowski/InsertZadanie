using MediatR;
using WalletService.DTOs;

namespace WalletService.Queries;

public record GetWalletByIdQuery(Guid Id) : IRequest<WalletResponse?>;
