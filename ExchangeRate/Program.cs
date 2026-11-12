using System.Text;
using ExchangeRate.Core.Interfaces;
using ExchangeRate.Core.Services;
using ExchangeRate.Infrastructure;
using ExchangeRate.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

var authSettings = builder.Configuration.GetSection(AuthSettings.SectionName).Get<AuthSettings>()
    ?? throw new InvalidOperationException($"Falta la sección '{AuthSettings.SectionName}' en la configuración.");

builder.Services.Configure<AuthSettings>(builder.Configuration.GetSection(AuthSettings.SectionName));

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddInfrastructure();
builder.Services.AddScoped<IExchangeService, ExchangeService>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = authSettings.Issuer,
            ValidAudience = authSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authSettings.Key))
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

await app.Services.InitializeDatabaseAsync();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
