using System.Security.Claims;
using System.Text;
using ExchangeRate.Core.Dtos;
using ExchangeRate.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace ExchangeRate.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IOptions<AuthSettings> options) : ControllerBase
{
    private readonly AuthSettings _settings = options.Value;

    [HttpPost("login")]
    public ActionResult<TokenResponse> Login(LoginRequest request)
    {
        if (request.Username != _settings.Username || request.Password != _settings.Password)
            return Unauthorized(new ProblemDetails { Title = "Credenciales inválidas." });

        var expires = DateTime.UtcNow.AddMinutes(_settings.ExpirationMinutes);
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity([new Claim(JwtRegisteredClaimNames.Sub, request.Username)]),
            Expires = expires,
            Issuer = _settings.Issuer,
            Audience = _settings.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key)),
                SecurityAlgorithms.HmacSha256)
        };

        var token = new JsonWebTokenHandler().CreateToken(descriptor);
        return Ok(new TokenResponse(token, expires));
    }
}
