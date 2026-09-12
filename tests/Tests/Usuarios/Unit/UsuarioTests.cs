using Domain.Common.Exceptions;
using Domain.Usuarios;
using Domain.Usuarios.Roles;
using FluentAssertions;

namespace Tests.Usuarios.Unit;

public class UsuarioTests
{
    private static Usuario CriarUsuario(string nome = "Admin", string email = "admin@teste.com", string senhaHash = "hash", string cpf = "12345678909") =>
        new(nome, email, senhaHash, cpf);

    [Fact]
    public void Construtor_ComDadosValidos_CriaUsuarioAtivo()
    {
        var usuario = CriarUsuario();

        usuario.Nome.Should().Be("Admin");
        usuario.Email.Should().Be("admin@teste.com");
        usuario.SenhaHash.Should().Be("hash");
        usuario.Cpf.Should().Be("12345678909");
        usuario.Ativo.Should().BeTrue();
        usuario.Roles.Should().BeEmpty();
    }

    [Fact]
    public void Construtor_ComCpfFormatado_RemoveMascara()
    {
        var usuario = CriarUsuario(cpf: "123.456.789-09");

        usuario.Cpf.Should().Be("12345678909");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Construtor_ComNomeInvalido_LancaExcecao(string? nome)
    {
        var act = () => new Usuario(nome!, "admin@teste.com", "hash", "12345678909");

        act.Should().Throw<DomainException>().WithMessage("*nome*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Construtor_ComEmailInvalido_LancaExcecao(string email)
    {
        var act = () => new Usuario("Admin", email, "hash", "12345678909");

        act.Should().Throw<DomainException>().WithMessage("*email*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Construtor_ComSenhaInvalida_LancaExcecao(string senhaHash)
    {
        var act = () => new Usuario("Admin", "admin@teste.com", senhaHash, "12345678909");

        act.Should().Throw<DomainException>().WithMessage("*senha*");
    }

    [Fact]
    public void Construtor_ComCpfInvalido_LancaExcecao()
    {
        var act = () => new Usuario("Admin", "admin@teste.com", "hash", "11111111111");

        act.Should().Throw<DomainException>().WithMessage("*CPF*");
    }

    [Fact]
    public void AdicionarRoles_ComRolesNovas_Adiciona()
    {
        var usuario = CriarUsuario();

        usuario.AdicionarRoles([new UsuarioRole("Admin"), new UsuarioRole("Cliente")]);

        usuario.Roles.Should().HaveCount(2);
        usuario.Roles.Select(r => r.Role).Should().BeEquivalentTo(["Admin", "Cliente"]);
    }

    [Fact]
    public void AdicionarRoles_ComRoleJaExistente_NaoDuplica()
    {
        var usuario = CriarUsuario();
        usuario.AdicionarRoles([new UsuarioRole("Admin")]);

        usuario.AdicionarRoles([new UsuarioRole("Admin"), new UsuarioRole("Cliente")]);

        usuario.Roles.Should().HaveCount(2);
    }

    [Fact]
    public void AdicionarRoles_ComRolesDuplicadasNaMesmaChamada_AdicionaUmaSoVez()
    {
        var usuario = CriarUsuario();

        usuario.AdicionarRoles([new UsuarioRole("Admin"), new UsuarioRole("Admin")]);

        usuario.Roles.Should().HaveCount(1);
    }

    [Fact]
    public void Inativar_MudaAtivoParaFalse()
    {
        var usuario = CriarUsuario();

        usuario.Inativar();

        usuario.Ativo.Should().BeFalse();
    }

    [Fact]
    public void Ativar_MudaAtivoParaTrue()
    {
        var usuario = CriarUsuario();
        usuario.Inativar();

        usuario.Ativar();

        usuario.Ativo.Should().BeTrue();
    }

    [Fact]
    public void UsuarioRole_Construtor_DefineRoleEId()
    {
        var role = new UsuarioRole("Admin");

        role.Role.Should().Be("Admin");
        role.Id.Should().NotBe(Guid.Empty);
    }
}
