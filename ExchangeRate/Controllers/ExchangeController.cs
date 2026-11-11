using ExchangeRate.Core.Dtos;
using ExchangeRate.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExchangeRate.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ExchangeController(IExchangeService exchangeService) : ControllerBase
{
    [HttpPost("convert")]
    public async Task<ActionResult<ConvertResponse>> Convert(ConvertRequest request, CancellationToken ct)
    {
        var result = await exchangeService.ConvertAsync(request, ct);
        if (result is null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Tipo de cambio no encontrado.",
                Detail = $"No existe un tipo de cambio entre {request.FromCurrency.ToUpperInvariant()} y {request.ToCurrency.ToUpperInvariant()}."
            });
        }

        return Ok(result);
    }

    [HttpPost("rate")]
    public async Task<ActionResult<UpdateRateResponse>> UpdateRate(UpdateRateRequest request, CancellationToken ct)
    {
        if (string.Equals(request.FromCurrency, request.ToCurrency, StringComparison.OrdinalIgnoreCase))
            return BadRequest(new ProblemDetails { Title = "La moneda origen y destino deben ser distintas." });

        return Ok(await exchangeService.UpdateRateAsync(request, ct));
    }
}
