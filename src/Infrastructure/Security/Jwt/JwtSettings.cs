using System.Diagnostics.CodeAnalysis;

namespace Infrastructure.Security.Jwt;

// POCO de configuração (appsettings) — sem lógica de negócio a testar.
[ExcludeFromCodeCoverage]
public class JwtSettings
{
    public string Secret { get; set; } = null!;
    public int ExpirationHours { get; set; } = 2;
}
