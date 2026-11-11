using System.ComponentModel.DataAnnotations;

namespace ExchangeRate.Core.Dtos;

public record ConvertRequest(
    [Range(typeof(decimal), "0.01", "1000000000000", ErrorMessage = "El monto debe ser mayor a 0.")]
    decimal Amount,
    [Required, RegularExpression("^[A-Za-z]{3}$", ErrorMessage = "La moneda debe ser un código de 3 letras (ej. USD).")]
    string FromCurrency,
    [Required, RegularExpression("^[A-Za-z]{3}$", ErrorMessage = "La moneda debe ser un código de 3 letras (ej. PEN).")]
    string ToCurrency);

public record ConvertResponse(
    decimal Amount,
    decimal ConvertedAmount,
    string FromCurrency,
    string ToCurrency,
    decimal ExchangeRate);

public record UpdateRateRequest(
    [Required, RegularExpression("^[A-Za-z]{3}$", ErrorMessage = "La moneda debe ser un código de 3 letras (ej. USD).")]
    string FromCurrency,
    [Required, RegularExpression("^[A-Za-z]{3}$", ErrorMessage = "La moneda debe ser un código de 3 letras (ej. PEN).")]
    string ToCurrency,
    [Range(typeof(decimal), "0.000001", "1000000000", ErrorMessage = "El tipo de cambio debe ser mayor a 0.")]
    decimal Rate);

public record UpdateRateResponse(
    string FromCurrency,
    string ToCurrency,
    decimal Rate,
    DateTime UpdatedAt);

public record LoginRequest(
    [Required] string Username,
    [Required] string Password);

public record TokenResponse(string Token, DateTime ExpiresAt);
