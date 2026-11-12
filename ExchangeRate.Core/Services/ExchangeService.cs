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

    public async Task<IReadOnlyList<CurrencyDto>> GetCurrenciesAsync(CancellationToken ct = default)
    {
        var currencies = await repository.GetCurrenciesAsync(ct);
        return currencies.Select(x => new CurrencyDto(x.Code, x.Name)).ToList();
    }

    public async Task<UpdateRateResponse> UpdateRateAsync(UpdateRateRequest request, CancellationToken ct = default)
    {
        var from = request.FromCurrency.ToUpperInvariant();
        var to = request.ToCurrency.ToUpperInvariant();
        var now = DateTime.UtcNow;

        await repository.EnsureCurrencyAsync(from, ct);
        await repository.EnsureCurrencyAsync(to, ct);

        // Se guarda también el inverso (1 USD = 2 PEN → 1 PEN = 0.5 USD).
        var inverseRate = Math.Round(1m / request.Rate, 6, MidpointRounding.AwayFromZero);
        await SaveRateAsync(from, to, request.Rate, now, ct);
        await SaveRateAsync(to, from, inverseRate, now, ct);
        await DeriveMissingRatesAsync(from, to, request.Rate, now, ct);
        await DeriveMissingRatesAsync(to, from, inverseRate, now, ct);
        await repository.SaveChangesAsync(ct);

        return new UpdateRateResponse(from, to, request.Rate, now);
    }

    // SOLO: Crea los pares que faltan entre "target" y las demás monedas, usando "bridge" como puente
    // (QQQ = 8 PEN y PEN = 1.6 BRL → QQQ = 12.8 BRL). Nunca pisa un par que ya existe.
    private async Task DeriveMissingRatesAsync(string target, string bridge, decimal targetToBridge, DateTime now, CancellationToken ct)
    {
        var known = (await repository.GetRatesFromAsync(target, ct)).Select(x => x.ToCurrency).ToHashSet();

        foreach (var bridgeRate in await repository.GetRatesFromAsync(bridge, ct))
        {
            var other = bridgeRate.ToCurrency;
            if (other == target || known.Contains(other))
                continue;

            var value = Math.Round(targetToBridge * bridgeRate.Rate, 6, MidpointRounding.AwayFromZero);
            if (value <= 0)
                continue;

            await SaveRateAsync(target, other, value, now, ct);
            await SaveRateAsync(other, target, Math.Round(1m / value, 6, MidpointRounding.AwayFromZero), now, ct);
        }
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
