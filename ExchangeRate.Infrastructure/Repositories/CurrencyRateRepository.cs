using ExchangeRate.Core.Entities;
using ExchangeRate.Core.Interfaces;
using ExchangeRate.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ExchangeRate.Infrastructure.Repositories;

public class CurrencyRateRepository(AppDbContext db) : ICurrencyRateRepository
{
    public async Task<CurrencyRate?> GetAsync(string from, string to, CancellationToken ct = default)
    {
        return await db.CurrencyRates
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.FromCurrency == from && x.ToCurrency == to, ct);
    }

    public async Task<CurrencyRate?> GetForUpdateAsync(string from, string to, CancellationToken ct = default)
    {
        return await db.CurrencyRates
            .FirstOrDefaultAsync(x => x.FromCurrency == from && x.ToCurrency == to, ct);
    }

    public async Task AddAsync(CurrencyRate rate, CancellationToken ct = default)
    {
        await db.CurrencyRates.AddAsync(rate, ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await db.SaveChangesAsync(ct);
    }
}
