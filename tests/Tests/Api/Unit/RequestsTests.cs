using Api.Controllers.Authentication.Requests;
using Api.Controllers.Clientes.Requests;
using Api.Controllers.Clientes.Veiculos.Requests;
using Api.Controllers.OrdensServico.Requests;
using Api.Controllers.Produtos.Requests;
using Api.Controllers.Servicos.Requests;
using FluentAssertions;

namespace Tests.Api.Unit;

// Records de request do Api layer (DTOs de entrada dos Controllers, que ficam
// [ExcludeFromCodeCoverage] por serem só roteamento HTTP) — construídos e
// comparados aqui pra exercitar construtor/getters/Equals gerados pelo compilador.
public class RequestsTests
{
    [Fact]
    public void ClienteRequests_ConstroemComPropriedadesCorretas()
    {
        var inserir = new InserirClienteRequest("João Silva", "12345678909");
        inserir.Nome.Should().Be("João Silva");
        inserir.Documento.Should().Be("12345678909");
        inserir.Should().Be(new InserirClienteRequest("João Silva", "12345678909"));

        var atualizar = new AtualizarClienteRequest("Novo Nome");
        atualizar.Nome.Should().Be("Novo Nome");
        atualizar.Ativo.Should().BeTrue();
        atualizar.Should().Be(new AtualizarClienteRequest("Novo Nome") with { Ativo = true });
    }

    [Fact]
    public void VeiculoRequests_ConstroemComPropriedadesCorretas()
    {
        var inserir = new InserirVeiculoRequest("ABC1234", "Toyota", "Corolla", 2020);
        inserir.Placa.Should().Be("ABC1234");
        inserir.Ano.Should().Be(2020);

        var atualizar = new AtualizarVeiculoRequest("DEF5678", "Honda", "Civic", 2021);
        atualizar.Marca.Should().Be("Honda");
        atualizar.Should().Be(new AtualizarVeiculoRequest("DEF5678", "Honda", "Civic", 2021));
    }

    [Fact]
    public void ProdutoRequests_ConstroemComPropriedadesCorretas()
    {
        var inserir = new InserirProdutoRequest("Óleo", "Óleo sintético", 79.90m, 10);
        inserir.Nome.Should().Be("Óleo");
        inserir.QuantidadeEmEstoque.Should().Be(10);

        var atualizar = new AtualizarProdutoRequest("Óleo", "Óleo sintético", 79.90m);
        atualizar.Valor.Should().Be(79.90m);

        var estoque = new AtualizarQuantidadeEstoqueProdutoRequest(5m);
        estoque.Quantidade.Should().Be(5m);
        estoque.Should().Be(new AtualizarQuantidadeEstoqueProdutoRequest(5m));
    }

    [Fact]
    public void ServicoRequests_ConstroemComPropriedadesCorretas()
    {
        var inserir = new InserirServicoRequest("Troca de óleo", "Substituição do óleo", 120m);
        inserir.Nome.Should().Be("Troca de óleo");

        var atualizar = new AtualizarServicoRequest("Troca de óleo", "Substituição do óleo", 120m);
        atualizar.Should().Be(new AtualizarServicoRequest("Troca de óleo", "Substituição do óleo", 120m));
    }

    [Fact]
    public void OrdemServicoRequests_ConstroemComPropriedadesCorretas()
    {
        var idCliente = Guid.NewGuid();
        var idVeiculo = Guid.NewGuid();
        var inserir = new InserirOrdemServicoRequest(idCliente, idVeiculo);
        inserir.IdCliente.Should().Be(idCliente);
        inserir.IdVeiculo.Should().Be(idVeiculo);

        var servicoItem = new RegistrarDiagnosticoServicoRequest(Guid.NewGuid(), "Troca de óleo", 120m);
        var produtoItem = new RegistrarDiagnosticoProdutoRequest(Guid.NewGuid(), "Óleo 5W30", 79.90m, 2m);
        var registrar = new RegistrarDiagnosticoRequest([servicoItem], [produtoItem]);

        registrar.Servicos.Should().ContainSingle();
        registrar.Produtos.Should().ContainSingle();
        servicoItem.NomeServico.Should().Be("Troca de óleo");
        produtoItem.Quantidade.Should().Be(2m);
    }

    [Fact]
    public void LoginRequest_ConstroiComPropriedadesCorretas()
    {
        var login = new LoginRequest("admin@teste.com", "123456");

        login.Email.Should().Be("admin@teste.com");
        login.Senha.Should().Be("123456");
        login.Should().Be(new LoginRequest("admin@teste.com", "123456"));
    }
}
