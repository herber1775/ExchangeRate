using ExchangeRate.Core.Entities;

namespace ExchangeRate.Core.Interfaces;

public interface ICurrencyRateRepository
{
    // Solo lectura (sin seguimiento de EF Core).
    Task<CurrencyRate?> GetAsync(string from, string to, CancellationToken ct = default);

    // Para modificar la entidad y luego llamar a SaveChangesAsync.
    Task<CurrencyRate?> GetForUpdateAsync(string from, string to, CancellationToken ct = default);

    // SOLO: Todos los tipos de cambio que tienen a esta moneda como origen.
    Task<IReadOnlyList<CurrencyRate>> GetRatesFromAsync(string from, CancellationToken ct = default);

    // SOLO: Monedas disponibles, ordenadas por código.
    Task<IReadOnlyList<Currency>> GetCurrenciesAsync(CancellationToken ct = default);

    // SOLO: Registra la moneda (con su código como nombre) si todavía no existe.
    Task EnsureCurrencyAsync(string code, CancellationToken ct = default);

    Task AddAsync(CurrencyRate rate, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
