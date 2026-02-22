using Microsoft.AspNetCore.Mvc;
using WalletService.Services;

namespace WalletService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RatesController : ControllerBase
{
    private readonly IRateCacheService _rateCacheService;

    public RatesController(IRateCacheService rateCacheService)
    {
        _rateCacheService = rateCacheService;
    }

    /// <summary>
    /// Get all cached exchange rates (from Redis cache, originally sourced from RatesService via ServiceBus).
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<Dictionary<string, decimal>>> GetAll()
    {
        var rates = await _rateCacheService.GetAllRatesAsync();

        if (rates.Count == 0)
            return NotFound("No exchange rates cached yet. Waiting for RatesService to publish rates.");

        return Ok(rates);
    }
}
