using ExchangeRate.Core.Entities;

namespace ExchangeRate.Core.Interfaces;

public interface ICurrencyRateRepository
{
    // Solo lectura (sin seguimiento de EF Core).
    Task<CurrencyRate?> GetAsync(string from, string to, CancellationToken ct = default);

    // Para modificar la entidad y luego llamar a SaveChangesAsync.
    Task<CurrencyRate?> GetForUpdateAsync(string from, string to, CancellationToken ct = default);

    Task AddAsync(CurrencyRate rate, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
