namespace ExchangeRate.Settings;

public class AuthSettings
{
    public const string SectionName = "Auth";

    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; } = 60;

    // Usuario de demostración para el login del reto.
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
