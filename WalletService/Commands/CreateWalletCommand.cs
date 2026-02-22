using MediatR;
using WalletService.DTOs;

namespace WalletService.Commands;

public record CreateWalletCommand(string Name) : IRequest<WalletResponse>;
