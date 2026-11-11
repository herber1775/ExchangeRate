using System.ComponentModel.DataAnnotations;

namespace ExchangeRate.Web.Models;

// Modelos mutables para los formularios (los DTOs de Core son records de solo lectura).
public class LoginForm
{
    [Required(ErrorMessage = "Ingresa tu usuario.")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresa tu contraseña.")]
    public string Password { get; set; } = string.Empty;
}

public class ConvertForm
{
    [Range(typeof(decimal), "0.01", "1000000000000", ErrorMessage = "El monto debe ser mayor a 0.")]
    public decimal Amount { get; set; } = 100;

    [Required(ErrorMessage = "Ingresa la moneda de origen."), RegularExpression("^[A-Za-z]{3}$", ErrorMessage = "Usa un código de 3 letras (ej. USD).")]
    public string FromCurrency { get; set; } = "USD";

    [Required(ErrorMessage = "Ingresa la moneda de destino."), RegularExpression("^[A-Za-z]{3}$", ErrorMessage = "Usa un código de 3 letras (ej. PEN).")]
    public string ToCurrency { get; set; } = "PEN";
}

public class UpdateRateForm
{
    [Required(ErrorMessage = "Ingresa la moneda de origen."), RegularExpression("^[A-Za-z]{3}$", ErrorMessage = "Usa un código de 3 letras (ej. USD).")]
    public string FromCurrency { get; set; } = "USD";

    [Required(ErrorMessage = "Ingresa la moneda de destino."), RegularExpression("^[A-Za-z]{3}$", ErrorMessage = "Usa un código de 3 letras (ej. PEN).")]
    public string ToCurrency { get; set; } = "PEN";

    [Range(typeof(decimal), "0.000001", "1000000000", ErrorMessage = "El tipo de cambio debe ser mayor a 0.")]
    public decimal Rate { get; set; } = 3.75m;
}
