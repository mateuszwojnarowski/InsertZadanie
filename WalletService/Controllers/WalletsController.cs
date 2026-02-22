using MediatR;
using Microsoft.AspNetCore.Mvc;
using WalletService.Commands;
using WalletService.DTOs;
using WalletService.Queries;

namespace WalletService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WalletsController : ControllerBase
{
    private readonly IMediator _mediator;

    public WalletsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<WalletResponse>> Create([FromBody] CreateWalletRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Wallet name is required");

        var result = await _mediator.Send(new CreateWalletCommand(request.Name));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet]
    public async Task<ActionResult<List<WalletResponse>>> GetAll()
    {
        var result = await _mediator.Send(new GetAllWalletsQuery());
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<WalletResponse>> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetWalletByIdQuery(id));
        return result is not null ? Ok(result) : NotFound($"Wallet {id} not found");
    }

}
