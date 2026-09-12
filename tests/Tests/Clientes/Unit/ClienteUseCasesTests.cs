using Application.Clientes.UseCases;
using Application.Clientes.UseCases.AtualizarCliente;
using Application.Clientes.UseCases.BuscarCliente;
using Application.Clientes.UseCases.BuscarListaPaginada;
using Application.Clientes.UseCases.InserirCliente;
using Application.Clientes.UseCases.RemoverCliente;
using Domain.Clientes;
using Domain.Clientes.Gateways;
using Domain.Clientes.ValueObjects;
using FluentAssertions;
using Moq;
using SharedKernel.DTOs;

namespace Tests.Clientes.Unit;

public class ClienteUseCasesTests
{
    private static Cliente CriarCliente(string nome = "João Silva", string documento = "12345678909")
    {
        var cliente = new Cliente();
        cliente.Inserir(nome, DocumentoCliente.Criar(documento));
        return cliente;
    }

    [Fact]
    public async Task Inserir_QuandoDocumentoDuplicado_ChamaDocumentoDuplicado()
    {
        var gateway = new Mock<IClienteGateway>();
        gateway.Setup(g => g.ExisteComDocumento(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var outputPort = new Mock<IInserirClienteOutputPort>();
        var useCase = new InserirClienteUseCase(gateway.Object, outputPort.Object);

        await useCase.Execute(new InserirClienteInput("João Silva", "12345678909"));

        outputPort.Verify(p => p.DocumentoDuplicado(It.IsAny<string>()), Times.Once);
        gateway.Verify(g => g.Salvar(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Inserir_ComDocumentoNovo_SalvaEChamaOk()
    {
        var gateway = new Mock<IClienteGateway>();
        gateway.Setup(g => g.ExisteComDocumento(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var outputPort = new Mock<IInserirClienteOutputPort>();
        var useCase = new InserirClienteUseCase(gateway.Object, outputPort.Object);

        await useCase.Execute(new InserirClienteInput("João Silva", "12345678909"));

        gateway.Verify(g => g.Salvar(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()), Times.Once);
        outputPort.Verify(p => p.Ok(It.Is<ClienteOutput>(o => o.Nome == "João Silva")), Times.Once);
    }

    [Fact]
    public async Task AtualizarCliente_QuandoNaoEncontrado_ChamaNaoEncontrado()
    {
        var gateway = new Mock<IClienteGateway>();
        gateway.Setup(g => g.BuscarPorId(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Cliente?)null);

        var outputPort = new Mock<IAtualizarClienteOutputPort>();
        var useCase = new AtualizarClienteUseCase(gateway.Object, outputPort.Object);

        await useCase.Execute(new AtualizarClienteInput(Guid.NewGuid(), "Novo Nome", true));

        outputPort.Verify(p => p.NaoEncontrado(), Times.Once);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task AtualizarCliente_QuandoEncontrado_AtualizaAtivoEChamaOk(bool ativo)
    {
        var cliente = CriarCliente();
        var gateway = new Mock<IClienteGateway>();
        gateway.Setup(g => g.BuscarPorId(cliente.Id, It.IsAny<CancellationToken>())).ReturnsAsync(cliente);

        var outputPort = new Mock<IAtualizarClienteOutputPort>();
        var useCase = new AtualizarClienteUseCase(gateway.Object, outputPort.Object);

        await useCase.Execute(new AtualizarClienteInput(cliente.Id, "Novo Nome", ativo));

        cliente.Nome.Should().Be("Novo Nome");
        cliente.Ativo.Should().Be(ativo);
        gateway.Verify(g => g.Atualizar(cliente, It.IsAny<CancellationToken>()), Times.Once);
        outputPort.Verify(p => p.Ok(It.IsAny<ClienteOutput>()), Times.Once);
    }

    [Fact]
    public async Task BuscarCliente_QuandoNaoEncontrado_ChamaNaoEncontrado()
    {
        var gateway = new Mock<IClienteGateway>();
        gateway.Setup(g => g.BuscarPorId(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Cliente?)null);

        var outputPort = new Mock<IBuscarClienteOutputPort>();
        var useCase = new BuscarClienteUseCase(gateway.Object, outputPort.Object);

        await useCase.Execute(new BuscarClienteInput(Guid.NewGuid()));

        outputPort.Verify(p => p.NaoEncontrado(), Times.Once);
    }

    [Fact]
    public async Task BuscarCliente_QuandoEncontrado_ChamaOk()
    {
        var cliente = CriarCliente();
        var gateway = new Mock<IClienteGateway>();
        gateway.Setup(g => g.BuscarPorId(cliente.Id, It.IsAny<CancellationToken>())).ReturnsAsync(cliente);

        var outputPort = new Mock<IBuscarClienteOutputPort>();
        var useCase = new BuscarClienteUseCase(gateway.Object, outputPort.Object);

        await useCase.Execute(new BuscarClienteInput(cliente.Id));

        outputPort.Verify(p => p.Ok(It.Is<ClienteOutput>(o => o.Id == cliente.Id)), Times.Once);
    }

    [Fact]
    public async Task BuscarListaPaginada_ChamaOkComResultadoPaginado()
    {
        var cliente = CriarCliente();
        var gateway = new Mock<IClienteGateway>();
        gateway.Setup(g => g.BuscarPaginado(It.IsAny<PagedRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Cliente> { cliente }, 1));

        var outputPort = new Mock<IBuscarListaPaginadaClienteOutputPort>();
        var useCase = new BuscarListaPaginadaClienteUseCase(gateway.Object, outputPort.Object);

        await useCase.Execute(new BuscarListaPaginadaClienteInput(new PagedRequest(1, 10)));

        outputPort.Verify(p => p.Ok(It.Is<PagedResult<ClienteOutput>>(r => r.TotalCount == 1 && r.Items.Count == 1)), Times.Once);
    }

    [Fact]
    public async Task RemoverCliente_QuandoNaoEncontrado_ChamaNaoEncontrado()
    {
        var gateway = new Mock<IClienteGateway>();
        gateway.Setup(g => g.BuscarPorId(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Cliente?)null);

        var outputPort = new Mock<IRemoverClienteOutputPort>();
        var useCase = new RemoverClienteUseCase(gateway.Object, outputPort.Object);

        await useCase.Execute(new RemoverClienteInput(Guid.NewGuid()));

        outputPort.Verify(p => p.NaoEncontrado(), Times.Once);
        gateway.Verify(g => g.Remover(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RemoverCliente_QuandoEncontrado_RemoveEChamaOk()
    {
        var cliente = CriarCliente();
        var gateway = new Mock<IClienteGateway>();
        gateway.Setup(g => g.BuscarPorId(cliente.Id, It.IsAny<CancellationToken>())).ReturnsAsync(cliente);

        var outputPort = new Mock<IRemoverClienteOutputPort>();
        var useCase = new RemoverClienteUseCase(gateway.Object, outputPort.Object);

        await useCase.Execute(new RemoverClienteInput(cliente.Id));

        gateway.Verify(g => g.Remover(cliente, It.IsAny<CancellationToken>()), Times.Once);
        outputPort.Verify(p => p.Ok(), Times.Once);
    }
}
