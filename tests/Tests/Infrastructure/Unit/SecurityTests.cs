using Domain.Usuarios;
using Domain.Usuarios.Roles;
using FluentAssertions;
using Infrastructure.Security.BCrypt;
using Infrastructure.Security.Jwt;
using Microsoft.Extensions.Options;

namespace Tests.Infrastructure.Unit;

public class SecurityTests
{
    [Fact]
    public void BCryptPasswordHasher_ComHashCorreto_RetornaTrue()
    {
        var hasher = new BCryptPasswordHasher();
        var hash = global::BCrypt.Net.BCrypt.HashPassword("senha-correta");

        hasher.Verificar("senha-correta", hash).Should().BeTrue();
    }

    [Fact]
    public void BCryptPasswordHasher_ComHashIncorreto_RetornaFalse()
    {
        var hasher = new BCryptPasswordHasher();
        var hash = global::BCrypt.Net.BCrypt.HashPassword("senha-correta");

        hasher.Verificar("senha-errada", hash).Should().BeFalse();
    }

    [Fact]
    public void JwtTokenProvider_GerarToken_IncluiNomeERoles()
    {
        var settings = Options.Create(new JwtSettings { Secret = "chave-secreta-de-teste-bem-grande-1234567890", ExpirationHours = 2 });
        var provider = new JwtTokenProvider(settings);

        var usuario = new Usuario("Admin", "admin@teste.com", "hash", "12345678909");
        usuario.AdicionarRoles([new UsuarioRole("Admin")]);

        var token = provider.GerarToken(usuario);

        token.Should().NotBeNullOrWhiteSpace();
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var parsed = handler.ReadJwtToken(token);
        parsed.Claims.Should().Contain(c => c.Type == System.Security.Claims.ClaimTypes.Name && c.Value == "Admin");
        parsed.Claims.Should().Contain(c => c.Type == System.Security.Claims.ClaimTypes.Role && c.Value == "Admin");
    }

    [Fact]
    public void JwtTokenProvider_SemRoles_GeraTokenSemClaimsDeRole()
    {
        var settings = Options.Create(new JwtSettings { Secret = "chave-secreta-de-teste-bem-grande-1234567890", ExpirationHours = 2 });
        var provider = new JwtTokenProvider(settings);

        var usuario = new Usuario("Cliente", "cliente@teste.com", "hash", "12345678909");

        var token = provider.GerarToken(usuario);

        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var parsed = handler.ReadJwtToken(token);
        parsed.Claims.Should().NotContain(c => c.Type == System.Security.Claims.ClaimTypes.Role);
    }
}
