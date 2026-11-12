namespace ExchangeRate.Core.Interfaces;

public interface IExternalRateProvider
{
    /// <returns>SOLO: Cuántas unidades de cada moneda equivalen a 1 USD, o null si el servicio externo no está disponible.</returns>
    Task<IReadOnlyDictionary<string, decimal>?> GetUsdRatesAsync(CancellationToken ct = default);
}
