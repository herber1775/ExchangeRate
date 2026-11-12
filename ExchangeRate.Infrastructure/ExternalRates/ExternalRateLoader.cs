using ExchangeRate.Core.Entities;
using ExchangeRate.Core.Interfaces;
using ExchangeRate.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ExchangeRate.Infrastructure.ExternalRates;

/// <summary>SOLO: Al arrancar, actualiza los tipos de cambio entre las monedas registradas con los valores de la API externa.</summary>
public class ExternalRateLoader(AppDbContext db, IExternalRateProvider provider)
{
    public async Task LoadAsync(CancellationToken ct = default)
    {
        var usdRates = await provider.GetUsdRatesAsync(ct);
        if (usdRates is null)
            return;

        var codes = (await db.Currencies.Select(x => x.Code).ToListAsync(ct))
            .Where(usdRates.ContainsKey)
            .ToList();
        var existing = await db.CurrencyRates.ToDictionaryAsync(x => (x.FromCurrency, x.ToCurrency), ct);
        var now = DateTime.UtcNow;

        foreach (var from in codes)
        {
            foreach (var to in codes.Where(c => c != from))
            {
                // SOLO: Ambas tasas son "unidades por 1 USD": 1 from = usd[to] / usd[from] to.
                var value = Math.Round(usdRates[to] / usdRates[from], 8, MidpointRounding.AwayFromZero);

                if (!existing.TryGetValue((from, to), out var rate))
                {
                    rate = new CurrencyRate { FromCurrency = from, ToCurrency = to };
                    db.CurrencyRates.Add(rate);
                }

                rate.Rate = value;
                rate.UpdatedAt = now;
            }
        }

        await db.SaveChangesAsync(ct);
    }
}
