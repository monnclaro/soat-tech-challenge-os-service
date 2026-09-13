using System.Diagnostics.CodeAnalysis;

namespace Infrastructure.Messaging;

// POCO de configuração (appsettings) — sem lógica de negócio a testar.
[ExcludeFromCodeCoverage]
public class RabbitMqSettings
{
    public string Host { get; set; } = "localhost";
    public string VirtualHost { get; set; } = "/";
    public string Username { get; set; } = "guest";
    public string Password { get; set; } = "guest";
}
