using ExchangeRate.Core.Dtos;

namespace ExchangeRate.Core.Interfaces;

public interface IExchangeService
{
    /// <returns>El resultado de la conversión, o null si no existe un tipo de cambio para el par.</returns>
    Task<ConvertResponse?> ConvertAsync(ConvertRequest request, CancellationToken ct = default);

    Task<UpdateRateResponse> UpdateRateAsync(UpdateRateRequest request, CancellationToken ct = default);
}
