using System.Text.Json;
using ExchangeRate.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace ExchangeRate.Infrastructure.ExternalRates;

/// <summary>SOLO: Tipos de cambio contra USD desde la API pública gratuita open.er-api.com (sin clave).</summary>
public class OpenErApiRateProvider(HttpClient http, ILogger<OpenErApiRateProvider> logger) : IExternalRateProvider
{
    private const string Url = "https://open.er-api.com/v6/latest/USD";

    public async Task<IReadOnlyDictionary<string, decimal>?> GetUsdRatesAsync(CancellationToken ct = default)
    {
        try
        {
            using var response = await http.GetAsync(Url, ct);
            response.EnsureSuccessStatusCode();

            using var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
            var root = json.RootElement;
            if (root.GetProperty("result").GetString() != "success")
                return null;

            var rates = new Dictionary<string, decimal>();
            foreach (var item in root.GetProperty("rates").EnumerateObject())
            {
                // SOLO: Se descartan códigos raros y tasas no positivas para no guardar datos inválidos.
                if (item.Name.Length == 3 && item.Value.TryGetDecimal(out var value) && value > 0)
                    rates[item.Name.ToUpperInvariant()] = value;
            }

            return rates.Count > 0 ? rates : null;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException or KeyNotFoundException or InvalidOperationException)
        {
            logger.LogWarning(ex, "No se pudieron obtener los tipos de cambio externos; se usan los valores iniciales.");
            return null;
        }
    }
}
