using MediatR;
using WalletService.DTOs;

namespace WalletService.Queries;

public record GetAllWalletsQuery : IRequest<List<WalletResponse>>;
