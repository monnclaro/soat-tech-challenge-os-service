namespace Infrastructure.Security.Jwt;

public class JwtSettings
{
    public string Secret { get; set; } = null!;
    public int ExpirationHours { get; set; } = 2;
}
