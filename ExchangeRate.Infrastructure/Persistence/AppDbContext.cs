using ExchangeRate.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExchangeRate.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<CurrencyRate> CurrencyRates => Set<CurrencyRate>();

    public DbSet<Currency> Currencies => Set<Currency>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Currency>(e =>
        {
            e.HasKey(x => x.Code);
            e.Property(x => x.Code).HasMaxLength(3);
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.HasData(
                new Currency { Code = "ARS", Name = "Peso argentino" },
                new Currency { Code = "BRL", Name = "Real brasileño" },
                new Currency { Code = "CLP", Name = "Peso chileno" },
                new Currency { Code = "COP", Name = "Peso colombiano" },
                new Currency { Code = "EUR", Name = "Euro" },
                new Currency { Code = "GBP", Name = "Libra esterlina" },
                new Currency { Code = "JPY", Name = "Yen japonés" },
                new Currency { Code = "MXN", Name = "Peso mexicano" },
                new Currency { Code = "PEN", Name = "Sol peruano" },
                new Currency { Code = "USD", Name = "Dólar estadounidense" });
        });

        modelBuilder.Entity<CurrencyRate>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.FromCurrency).HasMaxLength(3).IsRequired();
            e.Property(x => x.ToCurrency).HasMaxLength(3).IsRequired();
            e.Property(x => x.Rate).HasPrecision(18, 6);
            e.HasIndex(x => new { x.FromCurrency, x.ToCurrency }).IsUnique();

            var seedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            e.HasData(
                new CurrencyRate { Id = 1, FromCurrency = "USD", ToCurrency = "PEN", Rate = 3.75m, UpdatedAt = seedDate },
                new CurrencyRate { Id = 2, FromCurrency = "PEN", ToCurrency = "USD", Rate = 0.2667m, UpdatedAt = seedDate },
                new CurrencyRate { Id = 3, FromCurrency = "EUR", ToCurrency = "PEN", Rate = 4.05m, UpdatedAt = seedDate },
                new CurrencyRate { Id = 4, FromCurrency = "PEN", ToCurrency = "EUR", Rate = 0.2469m, UpdatedAt = seedDate },
                new CurrencyRate { Id = 5, FromCurrency = "USD", ToCurrency = "EUR", Rate = 0.92m, UpdatedAt = seedDate },
                new CurrencyRate { Id = 6, FromCurrency = "EUR", ToCurrency = "USD", Rate = 1.087m, UpdatedAt = seedDate });
        });
    }
}
