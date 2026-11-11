namespace ExchangeRate.Web.Services;

/// <summary>Sesión del usuario. Es Scoped: en Blazor Server vive mientras dure la conexión (circuito).</summary>
public class AuthState
{
    private DateTime _expiresAt;

    public string? Token { get; private set; }
    public string? Username { get; private set; }
    public bool IsAuthenticated
    {
        get { return Token is not null && _expiresAt > DateTime.UtcNow; }
    }

    public event Action? Changed;

    public void SignIn(string username, string token, DateTime expiresAt)
    {
        Username = username;
        Token = token;
        _expiresAt = expiresAt;
        Changed?.Invoke();
    }

    public void SignOut()
    {
        Username = null;
        Token = null;
        Changed?.Invoke();
    }
}
