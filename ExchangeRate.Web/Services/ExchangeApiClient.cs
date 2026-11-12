using System.Net;
using System.Net.Http.Headers;
using ExchangeRate.Core.Dtos;

namespace ExchangeRate.Web.Services;

public record ApiResult<T>(T? Value, string? Error, bool SessionExpired = false)
{
    public bool IsSuccess
    {
        get { return Error is null; }
    }
}

public class ExchangeApiClient(HttpClient http, AuthState auth)
{
    public async Task<ApiResult<TokenResponse>> LoginAsync(LoginRequest request)
    {
        return await PostAsync<LoginRequest, TokenResponse>("api/auth/login", request, authorize: false);
    }

    public async Task<ApiResult<ConvertResponse>> ConvertAsync(ConvertRequest request)
    {
        return await PostAsync<ConvertRequest, ConvertResponse>("api/exchange/convert", request, authorize: true);
    }

    public async Task<ApiResult<UpdateRateResponse>> UpdateRateAsync(UpdateRateRequest request)
    {
        return await PostAsync<UpdateRateRequest, UpdateRateResponse>("api/exchange/rate", request, authorize: true);
    }

    public async Task<ApiResult<List<CurrencyDto>>> GetCurrenciesAsync()
    {
        return await SendAsync<List<CurrencyDto>>(HttpMethod.Get, "api/exchange/currencies", null, authorize: true);
    }

    private async Task<ApiResult<TResponse>> PostAsync<TRequest, TResponse>(string url, TRequest body, bool authorize)
    {
        return await SendAsync<TResponse>(HttpMethod.Post, url, body, authorize);
    }

    private async Task<ApiResult<TResponse>> SendAsync<TResponse>(HttpMethod method, string url, object? body, bool authorize)
    {
        try
        {
            using var message = new HttpRequestMessage(method, url);
            if (body is not null)
                message.Content = JsonContent.Create(body, body.GetType());
            if (authorize && auth.Token is not null)
                message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);

            using var response = await http.SendAsync(message);

            if (response.IsSuccessStatusCode)
                return new ApiResult<TResponse>(await response.Content.ReadFromJsonAsync<TResponse>(), null);

            if (authorize && response.StatusCode == HttpStatusCode.Unauthorized)
            {
                auth.SignOut();
                return new ApiResult<TResponse>(default, "Tu sesión expiró. Inicia sesión nuevamente.", SessionExpired: true);
            }

            return new ApiResult<TResponse>(default, ErrorMessage(response.StatusCode));
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return new ApiResult<TResponse>(default, "No se pudo conectar con la API. Verifica que esté en ejecución.");
        }
    }

    private static string ErrorMessage(HttpStatusCode status)
    {
        switch (status)
        {
            case HttpStatusCode.BadRequest:
                return "Los datos ingresados no son válidos.";
            case HttpStatusCode.Unauthorized:
                return "Usuario o contraseña incorrectos.";
            case HttpStatusCode.NotFound:
                return "No existe un tipo de cambio para esas monedas.";
            default:
                return $"Error inesperado ({(int)status}).";
        }
    }
}
