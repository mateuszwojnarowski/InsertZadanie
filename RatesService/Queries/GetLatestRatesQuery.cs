using MediatR;
using RatesService.Data.Models;

namespace RatesService.Queries;

public record GetLatestRatesQuery : IRequest<ExchangeRateTable?>
{
}
