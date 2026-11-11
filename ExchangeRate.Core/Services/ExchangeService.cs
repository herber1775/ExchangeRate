using ExchangeRate.Core.Dtos;
using ExchangeRate.Core.Entities;
using ExchangeRate.Core.Interfaces;

namespace ExchangeRate.Core.Services;

public class ExchangeService(ICurrencyRateRepository repository) : IExchangeService
{
    public async Task<ConvertResponse?> ConvertAsync(ConvertRequest request, CancellationToken ct = default)
    {
        var from = request.FromCurrency.ToUpperInvariant();
        var to = request.ToCurrency.ToUpperInvariant();

        var rate = await repository.GetAsync(from, to, ct);
        if (rate is null)
            return null;

        var converted = Math.Round(request.Amount * rate.Rate, 2, MidpointRounding.AwayFromZero);
        return new ConvertResponse(request.Amount, converted, from, to, rate.Rate);
    }

    public async Task<UpdateRateResponse> UpdateRateAsync(UpdateRateRequest request, CancellationToken ct = default)
    {
        var from = request.FromCurrency.ToUpperInvariant();
        var to = request.ToCurrency.ToUpperInvariant();
        var now = DateTime.UtcNow;

        // Se guarda también el inverso (1 USD = 2 PEN → 1 PEN = 0.5 USD).
        var inverseRate = Math.Round(1m / request.Rate, 6, MidpointRounding.AwayFromZero);
        await SaveRateAsync(from, to, request.Rate, now, ct);
        await SaveRateAsync(to, from, inverseRate, now, ct);
        await repository.SaveChangesAsync(ct);

        return new UpdateRateResponse(from, to, request.Rate, now);
    }

    private async Task SaveRateAsync(string from, string to, decimal value, DateTime now, CancellationToken ct)
    {
        var rate = await repository.GetForUpdateAsync(from, to, ct);
        if (rate is null)
        {
            rate = new CurrencyRate { FromCurrency = from, ToCurrency = to };
            await repository.AddAsync(rate, ct);
        }

        rate.Rate = value;
        rate.UpdatedAt = now;
    }
}
