using ExchangeRate.Core.Interfaces;
using ExchangeRate.Infrastructure.ExternalRates;
using ExchangeRate.Infrastructure.Persistence;
using ExchangeRate.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ExchangeRate.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // SQLite en memoria vive mientras la conexión esté abierta: se comparte una sola para toda la app.
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        services.AddSingleton(connection);
        services.AddDbContext<AppDbContext>(options => options.UseSqlite(connection));
        services.AddScoped<ICurrencyRateRepository, CurrencyRateRepository>();

        services.AddSingleton<IExternalRateProvider>(sp => new OpenErApiRateProvider(
            new HttpClient { Timeout = TimeSpan.FromSeconds(8) },
            sp.GetRequiredService<ILogger<OpenErApiRateProvider>>()));
        services.AddScoped<ExternalRateLoader>();

        return services;
    }

    public static async Task InitializeDatabaseAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreatedAsync();
        await scope.ServiceProvider.GetRequiredService<ExternalRateLoader>().LoadAsync();
    }
}
