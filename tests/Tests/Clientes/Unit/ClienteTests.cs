using Domain.Clientes;
using Domain.Clientes.Enums;
using Domain.Clientes.ValueObjects;
using Domain.Common.Exceptions;
using FluentAssertions;

namespace Tests.Clientes.Unit;

public class ClienteTests
{
    [Fact]
    public void Inserir_ComDadosValidos_CriaClienteAtivo()
    {
        var documento = DocumentoCliente.Criar("12345678909");

        var cliente = new Cliente();
        cliente.Inserir("João Silva", documento);

        cliente.Nome.Should().Be("João Silva");
        cliente.Documento.Should().Be("12345678909");
        cliente.TipoDocumento.Should().Be(TipoDocumentoCliente.Cpf);
        cliente.Ativo.Should().BeTrue();
    }

    [Fact]
    public void Inserir_ComNomeVazio_LancaExcecao()
    {
        var documento = DocumentoCliente.Criar("12345678909");
        var cliente = new Cliente();

        var act = () => cliente.Inserir("", documento);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Inativar_MudaAtivoParaFalse()
    {
        var cliente = new Cliente();
        cliente.Inserir("João Silva", DocumentoCliente.Criar("12345678909"));

        cliente.Inativar();

        cliente.Ativo.Should().BeFalse();
    }

    [Theory]
    [InlineData("12345678909")]
    [InlineData("11222333000181")]
    public void DocumentoCliente_Criar_AceitaCpfECnpjValidos(string documento)
    {
        var act = () => DocumentoCliente.Criar(documento);

        act.Should().NotThrow();
    }

    [Fact]
    public void DocumentoCliente_Criar_ComDigitosInvalidos_LancaExcecao()
    {
        var act = () => DocumentoCliente.Criar("11111111111");

        act.Should().Throw<DomainException>();
    }
}
