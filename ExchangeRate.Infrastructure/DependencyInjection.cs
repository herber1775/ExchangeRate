using ExchangeRate.Core.Interfaces;
using ExchangeRate.Infrastructure.Persistence;
using ExchangeRate.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

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

        return services;
    }

    public static void InitializeDatabase(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();
    }
}
