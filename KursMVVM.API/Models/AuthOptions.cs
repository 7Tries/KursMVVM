using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace KursMVVM.API.Models;

/// <summary>
/// Параметры JWT-аутентификации.
/// </summary>
public class AuthOptions
{
    public const string ISSUER = "KursApiServer";     // издатель токена
    public const string AUDIENCE = "KursApiClient";   // потребитель токена
    private const string KEY = "kurs_api_supersecret_secretkey!456789";  // ключ шифрования

    public static SymmetricSecurityKey GetSymmetricSecurityKey() =>
        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(KEY));
}
