using Application.OrdensServico.UseCases;
using Application.OrdensServico.UseCases.BuscarHistorico;
using Application.OrdensServico.UseCases.BuscarListaPaginada;
using Application.OrdensServico.UseCases.BuscarListaPaginadaPorDocumento;
using Application.OrdensServico.UseCases.BuscarOrdemServico;
using Application.OrdensServico.UseCases.Entregar;
using Application.OrdensServico.UseCases.Finalizar;
using Application.OrdensServico.UseCases.Remover;
using Domain.Clientes;
using Domain.Clientes.Gateways;
using Domain.Clientes.ValueObjects;
using Domain.Clientes.Veiculos;
using Domain.Clientes.Veiculos.Gateways;
using Domain.Clientes.Veiculos.ValueObjects;
using Domain.OrdensServico;
using Domain.OrdensServico.Enums;
using Domain.OrdensServico.Gateways;
using Domain.OrdensServico.Historico;
using Domain.OrdensServico.Historico.Gateways;
using Domain.OrdensServico.Servicos;
using FluentAssertions;
using Moq;
using SharedKernel.DTOs;

namespace Tests.OrdensServico.Unit;

// Cobre o "lado de leitura" da OS (consultas + os passos finais Entregar/Remover),
// que ainda não tinham teste dedicado (a saga em si já é coberta por SagaUseCasesTests).
public class OrdemServicoQueryUseCasesTests
{
    private static (OrdemServico Os, Cliente Cliente, Veiculo Veiculo) CriarOrdemServicoComItens()
    {
        var cliente = new Cliente();
        cliente.Inserir("João Silva", DocumentoCliente.Criar("12345678909"));

        var veiculo = new Veiculo();
        veiculo.Inserir(cliente.Id, Placa.Criar("ABC1234"), "Toyota", "Corolla", 2020);

        var os = new OrdemServico();
        os.Inserir(cliente.Id, veiculo.Id);
        os.IniciarDiagnostico();
        os.RegistrarDiagnostico(
            [new OrdemServicoServico(os.Id, Guid.NewGuid(), "Troca de óleo", 120m)],
            []);

        return (os, cliente, veiculo);
    }

    [Fact]
    public async Task BuscarOrdemServico_QuandoNaoEncontrada_ChamaNaoEncontrado()
    {
        var gateway = new Mock<IOrdemServicoGateway>();
        gateway.Setup(g => g.BuscarComItens(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((OrdemServico?)null);

        var outputPort = new Mock<IBuscarOrdemServicoOutputPort>();
        var useCase = new BuscarOrdemServicoUseCase(gateway.Object, Mock.Of<IClienteGateway>(), Mock.Of<IVeiculoGateway>(), outputPort.Object);

        await useCase.Execute(new BuscarOrdemServicoInput(Guid.NewGuid()));

        outputPort.Verify(p => p.NaoEncontrado(), Times.Once);
    }

    [Fact]
    public async Task BuscarOrdemServico_QuandoEncontrada_ChamaOkComClienteEVeiculo()
    {
        var (os, cliente, veiculo) = CriarOrdemServicoComItens();

        var gateway = new Mock<IOrdemServicoGateway>();
        gateway.Setup(g => g.BuscarComItens(os.Id, It.IsAny<CancellationToken>())).ReturnsAsync(os);

        var clienteGateway = new Mock<IClienteGateway>();
        clienteGateway.Setup(g => g.BuscarPorId(cliente.Id, It.IsAny<CancellationToken>())).ReturnsAsync(cliente);

        var veiculoGateway = new Mock<IVeiculoGateway>();
        veiculoGateway.Setup(g => g.BuscarPorId(veiculo.Id, It.IsAny<CancellationToken>())).ReturnsAsync(veiculo);

        var outputPort = new Mock<IBuscarOrdemServicoOutputPort>();
        var useCase = new BuscarOrdemServicoUseCase(gateway.Object, clienteGateway.Object, veiculoGateway.Object, outputPort.Object);

        await useCase.Execute(new BuscarOrdemServicoInput(os.Id));

        outputPort.Verify(p => p.Ok(It.Is<OrdemServicoOutput>(o =>
            o.Id == os.Id &&
            o.Cliente.Nome == "João Silva" &&
            o.Veiculo.Placa == "ABC1234" &&
            o.Servicos.Count == 1)), Times.Once);
    }

    [Fact]
    public async Task BuscarListaPaginada_ChamaOkComItensMapeadosECacheDeClienteEVeiculo()
    {
        var (os1, cliente, veiculo) = CriarOrdemServicoComItens();
        var os2 = new OrdemServico();
        os2.Inserir(cliente.Id, veiculo.Id);

        var gateway = new Mock<IOrdemServicoGateway>();
        gateway.Setup(g => g.BuscarPaginado(It.IsAny<PagedRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<OrdemServico> { os1, os2 }, 2));

        var clienteGateway = new Mock<IClienteGateway>();
        clienteGateway.Setup(g => g.BuscarPorId(cliente.Id, It.IsAny<CancellationToken>())).ReturnsAsync(cliente);

        var veiculoGateway = new Mock<IVeiculoGateway>();
        veiculoGateway.Setup(g => g.BuscarPorId(veiculo.Id, It.IsAny<CancellationToken>())).ReturnsAsync(veiculo);

        var outputPort = new Mock<IBuscarListaPaginadaOrdemServicoOutputPort>();
        var useCase = new BuscarListaPaginadaOrdemServicoUseCase(gateway.Object, clienteGateway.Object, veiculoGateway.Object, outputPort.Object);

        await useCase.Execute(new BuscarListaPaginadaOrdemServicoInput(new PagedRequest(1, 10)));

        outputPort.Verify(p => p.Ok(It.Is<PagedResult<OrdemServicoOutput>>(r => r.TotalCount == 2 && r.Items.Count == 2)), Times.Once);
        // Cache: BuscarPorId dos dois gateways deve ser chamado só 1x, mesmo com 2 OS do mesmo cliente/veículo.
        clienteGateway.Verify(g => g.BuscarPorId(cliente.Id, It.IsAny<CancellationToken>()), Times.Once);
        veiculoGateway.Verify(g => g.BuscarPorId(veiculo.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task BuscarListaPaginadaPorDocumento_QuandoCallerDocumentoDiferente_RetornaListaVaziaSemConsultarGateway()
    {
        var gateway = new Mock<IOrdemServicoGateway>();
        var outputPort = new Mock<IBuscarListaPaginadaPorDocumentoOutputPort>();
        var useCase = new BuscarListaPaginadaPorDocumentoUseCase(gateway.Object, Mock.Of<IClienteGateway>(), Mock.Of<IVeiculoGateway>(), outputPort.Object);

        await useCase.Execute(new BuscarListaPaginadaPorDocumentoInput("12345678909", new PagedRequest(1, 10), "99988877766"));

        outputPort.Verify(p => p.Ok(It.Is<PagedResult<OrdemServicoPorDocumentoOutput>>(r => r.TotalCount == 0 && r.Items.Count == 0)), Times.Once);
        gateway.Verify(g => g.BuscarPaginadoPorDocumentoCliente(It.IsAny<string>(), It.IsAny<PagedRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task BuscarListaPaginadaPorDocumento_QuandoCallerDocumentoIgual_ConsultaEMapeiaItens()
    {
        var (os, cliente, veiculo) = CriarOrdemServicoComItens();

        var gateway = new Mock<IOrdemServicoGateway>();
        gateway.Setup(g => g.BuscarPaginadoPorDocumentoCliente("12345678909", It.IsAny<PagedRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<OrdemServico> { os }, 1));

        var clienteGateway = new Mock<IClienteGateway>();
        clienteGateway.Setup(g => g.BuscarPorId(cliente.Id, It.IsAny<CancellationToken>())).ReturnsAsync(cliente);

        var veiculoGateway = new Mock<IVeiculoGateway>();
        veiculoGateway.Setup(g => g.BuscarPorId(veiculo.Id, It.IsAny<CancellationToken>())).ReturnsAsync(veiculo);

        var outputPort = new Mock<IBuscarListaPaginadaPorDocumentoOutputPort>();
        var useCase = new BuscarListaPaginadaPorDocumentoUseCase(gateway.Object, clienteGateway.Object, veiculoGateway.Object, outputPort.Object);

        await useCase.Execute(new BuscarListaPaginadaPorDocumentoInput("123.456.789-09", new PagedRequest(1, 10), "123.456.789-09"));

        outputPort.Verify(p => p.Ok(It.Is<PagedResult<OrdemServicoPorDocumentoOutput>>(r =>
            r.TotalCount == 1 &&
            r.Items.Count == 1 &&
            r.Items[0].Cliente.Nome == "João Silva" &&
            r.Items[0].Servicos.Count == 1)), Times.Once);
    }

    [Fact]
    public async Task BuscarListaPaginadaPorDocumento_SemCallerDocumento_ConsultaNormalmente()
    {
        var gateway = new Mock<IOrdemServicoGateway>();
        gateway.Setup(g => g.BuscarPaginadoPorDocumentoCliente("12345678909", It.IsAny<PagedRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<OrdemServico>(), 0));

        var outputPort = new Mock<IBuscarListaPaginadaPorDocumentoOutputPort>();
        var useCase = new BuscarListaPaginadaPorDocumentoUseCase(gateway.Object, Mock.Of<IClienteGateway>(), Mock.Of<IVeiculoGateway>(), outputPort.Object);

        await useCase.Execute(new BuscarListaPaginadaPorDocumentoInput("123.456.789-09", new PagedRequest(1, 10)));

        outputPort.Verify(p => p.Ok(It.Is<PagedResult<OrdemServicoPorDocumentoOutput>>(r => r.TotalCount == 0)), Times.Once);
        gateway.Verify(g => g.BuscarPaginadoPorDocumentoCliente("12345678909", It.IsAny<PagedRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task BuscarHistorico_QuandoOsNaoEncontrada_ChamaNaoEncontrado()
    {
        var gateway = new Mock<IOrdemServicoGateway>();
        gateway.Setup(g => g.BuscarPorId(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((OrdemServico?)null);

        var outputPort = new Mock<IBuscarHistoricoOutputPort>();
        var useCase = new BuscarHistoricoUseCase(gateway.Object, Mock.Of<IHistoricoStatusOrdemServicoGateway>(), outputPort.Object);

        await useCase.Execute(new BuscarHistoricoInput(Guid.NewGuid()));

        outputPort.Verify(p => p.NaoEncontrado(), Times.Once);
    }

    [Fact]
    public async Task BuscarHistorico_QuandoEncontrada_ChamaOkComItensMapeados()
    {
        var os = new OrdemServico();
        os.Inserir(Guid.NewGuid(), Guid.NewGuid());

        var gateway = new Mock<IOrdemServicoGateway>();
        gateway.Setup(g => g.BuscarPorId(os.Id, It.IsAny<CancellationToken>())).ReturnsAsync(os);

        var historicoGateway = new Mock<IHistoricoStatusOrdemServicoGateway>();
        historicoGateway.Setup(g => g.BuscarPorOrdemServico(os.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<HistoricoStatusOrdemServico>
            {
                new(os.Id, StatusOrdemServico.Recebida, DateTime.UtcNow)
            });

        var outputPort = new Mock<IBuscarHistoricoOutputPort>();
        var useCase = new BuscarHistoricoUseCase(gateway.Object, historicoGateway.Object, outputPort.Object);

        await useCase.Execute(new BuscarHistoricoInput(os.Id));

        outputPort.Verify(p => p.Ok(It.Is<IReadOnlyList<HistoricoStatusOutput>>(l => l.Count == 1 && l[0].Status == "Recebida")), Times.Once);
    }

    [Fact]
    public async Task RemoverOrdemServico_QuandoNaoEncontrada_ChamaNaoEncontrado()
    {
        var gateway = new Mock<IOrdemServicoGateway>();
        gateway.Setup(g => g.BuscarPorId(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((OrdemServico?)null);

        var outputPort = new Mock<IRemoverOrdemServicoOutputPort>();
        var useCase = new RemoverOrdemServicoUseCase(gateway.Object, outputPort.Object);

        await useCase.Execute(new RemoverOrdemServicoInput(Guid.NewGuid()));

        outputPort.Verify(p => p.NaoEncontrado(), Times.Once);
        gateway.Verify(g => g.Remover(It.IsAny<OrdemServico>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RemoverOrdemServico_QuandoEncontrada_RemoveEChamaOk()
    {
        var os = new OrdemServico();
        os.Inserir(Guid.NewGuid(), Guid.NewGuid());

        var gateway = new Mock<IOrdemServicoGateway>();
        gateway.Setup(g => g.BuscarPorId(os.Id, It.IsAny<CancellationToken>())).ReturnsAsync(os);

        var outputPort = new Mock<IRemoverOrdemServicoOutputPort>();
        var useCase = new RemoverOrdemServicoUseCase(gateway.Object, outputPort.Object);

        await useCase.Execute(new RemoverOrdemServicoInput(os.Id));

        gateway.Verify(g => g.Remover(os, It.IsAny<CancellationToken>()), Times.Once);
        outputPort.Verify(p => p.Ok(), Times.Once);
    }

    [Fact]
    public async Task Entregar_QuandoNaoEncontrada_ChamaNaoEncontrado()
    {
        var gateway = new Mock<IOrdemServicoGateway>();
        gateway.Setup(g => g.BuscarPorId(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((OrdemServico?)null);

        var outputPort = new Mock<IEntregarOutputPort>();
        var useCase = new EntregarUseCase(gateway.Object, outputPort.Object);

        await useCase.Execute(new EntregarInput(Guid.NewGuid()));

        outputPort.Verify(p => p.NaoEncontrado(), Times.Once);
    }

    [Fact]
    public async Task Entregar_QuandoFinalizada_AtualizaEChamaOk()
    {
        var os = new OrdemServico();
        os.Inserir(Guid.NewGuid(), Guid.NewGuid());
        os.IniciarDiagnostico();
        os.RegistrarDiagnostico([new OrdemServicoServico(os.Id, Guid.NewGuid(), "Troca de óleo", 120m)], []);
        os.AprovarPagamento();
        os.Finalizar();

        var gateway = new Mock<IOrdemServicoGateway>();
        gateway.Setup(g => g.BuscarPorId(os.Id, It.IsAny<CancellationToken>())).ReturnsAsync(os);

        var outputPort = new Mock<IEntregarOutputPort>();
        var useCase = new EntregarUseCase(gateway.Object, outputPort.Object);

        await useCase.Execute(new EntregarInput(os.Id));

        os.Status.Should().Be(StatusOrdemServico.Entregue);
        gateway.Verify(g => g.Atualizar(os, It.IsAny<CancellationToken>()), Times.Once);
        outputPort.Verify(p => p.Ok(), Times.Once);
    }

    [Fact]
    public async Task Finalizar_QuandoEmExecucao_AtualizaEChamaOk()
    {
        var os = new OrdemServico();
        os.Inserir(Guid.NewGuid(), Guid.NewGuid());
        os.IniciarDiagnostico();
        os.RegistrarDiagnostico([new OrdemServicoServico(os.Id, Guid.NewGuid(), "Troca de óleo", 120m)], []);
        os.AprovarPagamento();

        var gateway = new Mock<IOrdemServicoGateway>();
        gateway.Setup(g => g.BuscarPorId(os.Id, It.IsAny<CancellationToken>())).ReturnsAsync(os);

        var outputPort = new Mock<IFinalizarOutputPort>();
        var useCase = new FinalizarUseCase(gateway.Object, outputPort.Object);

        await useCase.Execute(new FinalizarInput(os.Id));

        os.Status.Should().Be(StatusOrdemServico.Finalizada);
        gateway.Verify(g => g.Atualizar(os, It.IsAny<CancellationToken>()), Times.Once);
        outputPort.Verify(p => p.Ok(), Times.Once);
    }
}
