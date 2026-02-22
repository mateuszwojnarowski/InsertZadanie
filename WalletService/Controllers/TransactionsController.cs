using MediatR;
using Microsoft.AspNetCore.Mvc;
using WalletService.Commands;
using WalletService.DTOs;

namespace WalletService.Controllers;

[ApiController]
[Route("api/wallets/{walletId:guid}")]
public class TransactionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TransactionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("deposit")]
    public async Task<ActionResult<BalanceUpdateResponse>> Deposit(Guid walletId, [FromBody] DepositRequest request)
    {
        if (request.Amount <= 0)
            return BadRequest("Amount must be positive");

        var result = await _mediator.Send(new DepositCommand(walletId, request.CurrencyCode, request.Amount));
        return result.Success
            ? Ok(new BalanceUpdateResponse(result.CurrencyCode!, result.NewBalance!.Value))
            : NotFound(result.Error);
    }

    [HttpPost("withdraw")]
    public async Task<ActionResult<BalanceUpdateResponse>> Withdraw(Guid walletId, [FromBody] WithdrawRequest request)
    {
        if (request.Amount <= 0)
            return BadRequest("Amount must be positive");

        var result = await _mediator.Send(new WithdrawCommand(walletId, request.CurrencyCode, request.Amount));
        return result.Success
            ? Ok(new BalanceUpdateResponse(result.CurrencyCode!, result.NewBalance!.Value))
            : BadRequest(result.Error);
    }

    [HttpPost("convert")]
    public async Task<ActionResult<ConvertResponse>> Convert(Guid walletId, [FromBody] ConvertRequest request)
    {
        if (request.Amount <= 0)
            return BadRequest("Amount must be positive");

        if (request.FromCurrency == request.ToCurrency)
            return BadRequest("Source and target currencies must be different");

        var result = await _mediator.Send(new ConvertCommand(walletId, request.FromCurrency, request.ToCurrency, request.Amount));
        return result.Success
            ? Ok(new ConvertResponse(
                new ConvertCurrencyDetail(result.FromCurrency!, result.FromAmount!.Value, result.FromNewBalance!.Value),
                new ConvertCurrencyDetail(result.ToCurrency!, result.ToAmount!.Value, result.ToNewBalance!.Value),
                result.ExchangeRate!.Value))
            : BadRequest(result.Error);
    }
}
