using Application.OrdensServico.UseCases.AprovarPagamento;
using Application.OrdensServico.UseCases.Cancelar;
using Application.OrdensServico.UseCases.Finalizar;
using Application.OrdensServico.UseCases.IniciarDiagnostico;
using Application.OrdensServico.UseCases.Inserir;
using Application.OrdensServico.UseCases.RegistrarDiagnostico;
using Domain.Clientes;
using Domain.Clientes.Gateways;
using Domain.Clientes.ValueObjects;
using Domain.Clientes.Veiculos;
using Domain.OrdensServico;
using Domain.OrdensServico.Gateways;
using FluentAssertions;
using Moq;

namespace Tests.OrdensServico.Unit;

// Testa os use cases que hoje expõem, como endpoints internos, cada passo da
// saga (a serem substituídos por consumers RabbitMQ/MassTransit).
public class SagaUseCasesTests
{
    private static OrdemServico CriarOrdemServico(Action<OrdemServico>? preparar = null)
    {
        var os = new OrdemServico();
        os.Inserir(Guid.NewGuid(), Guid.NewGuid());
        preparar?.Invoke(os);
        return os;
    }

    [Fact]
    public async Task Inserir_QuandoClienteNaoExiste_ChamaClienteNaoEncontrado()
    {
        var clienteGateway = new Mock<IClienteGateway>();
        clienteGateway.Setup(g => g.BuscarComVeiculos(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cliente?)null);

        var outputPort = new Mock<IInserirOrdemServicoOutputPort>();
        var useCase = new InserirOrdemServicoUseCase(Mock.Of<IOrdemServicoGateway>(), clienteGateway.Object, outputPort.Object);

        await useCase.Execute(new InserirOrdemServicoInput(Guid.NewGuid(), Guid.NewGuid()));

        outputPort.Verify(p => p.ClienteNaoEncontrado(), Times.Once);
    }

    [Fact]
    public async Task Inserir_QuandoVeiculoNaoPertenceAoCliente_ChamaVeiculoNaoPertenceAoCliente()
    {
        var cliente = new Cliente();
        cliente.Inserir("João Silva", DocumentoCliente.Criar("12345678909"));

        var clienteGateway = new Mock<IClienteGateway>();
        clienteGateway.Setup(g => g.BuscarComVeiculos(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        var outputPort = new Mock<IInserirOrdemServicoOutputPort>();
        var useCase = new InserirOrdemServicoUseCase(Mock.Of<IOrdemServicoGateway>(), clienteGateway.Object, outputPort.Object);

        await useCase.Execute(new InserirOrdemServicoInput(cliente.Id, Guid.NewGuid()));

        outputPort.Verify(p => p.VeiculoNaoPertenceAoCliente("João Silva"), Times.Once);
    }

    [Fact]
    public async Task Inserir_ComVeiculoDoCliente_SalvaEChamaOk()
    {
        var cliente = new Cliente();
        cliente.Inserir("João Silva", DocumentoCliente.Criar("12345678909"));
        var veiculo = new Veiculo();
        veiculo.Inserir(cliente.Id, Domain.Clientes.Veiculos.ValueObjects.Placa.Criar("ABC1234"), "Toyota", "Corolla", 2020);
        cliente.Veiculos.Add(veiculo);

        var clienteGateway = new Mock<IClienteGateway>();
        clienteGateway.Setup(g => g.BuscarComVeiculos(cliente.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        var ordemServicoGateway = new Mock<IOrdemServicoGateway>();
        var outputPort = new Mock<IInserirOrdemServicoOutputPort>();
        var useCase = new InserirOrdemServicoUseCase(ordemServicoGateway.Object, clienteGateway.Object, outputPort.Object);

        await useCase.Execute(new InserirOrdemServicoInput(cliente.Id, veiculo.Id));

        ordemServicoGateway.Verify(g => g.Salvar(It.IsAny<OrdemServico>(), It.IsAny<CancellationToken>()), Times.Once);
        outputPort.Verify(p => p.Ok(It.IsAny<Guid>()), Times.Once);
    }

    [Fact]
    public async Task IniciarDiagnostico_QuandoNaoEncontrada_ChamaNaoEncontrado()
    {
        var gateway = new Mock<IOrdemServicoGateway>();
        gateway.Setup(g => g.BuscarPorId(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((OrdemServico?)null);

        var outputPort = new Mock<IIniciarDiagnosticoOutputPort>();
        var useCase = new IniciarDiagnosticoUseCase(gateway.Object, outputPort.Object);

        await useCase.Execute(new IniciarDiagnosticoInput(Guid.NewGuid()));

        outputPort.Verify(p => p.NaoEncontrado(), Times.Once);
    }

    [Fact]
    public async Task IniciarDiagnostico_QuandoEncontrada_AtualizaEChamaOk()
    {
        var os = CriarOrdemServico();
        var gateway = new Mock<IOrdemServicoGateway>();
        gateway.Setup(g => g.BuscarPorId(os.Id, It.IsAny<CancellationToken>())).ReturnsAsync(os);

        var outputPort = new Mock<IIniciarDiagnosticoOutputPort>();
        var useCase = new IniciarDiagnosticoUseCase(gateway.Object, outputPort.Object);

        await useCase.Execute(new IniciarDiagnosticoInput(os.Id));

        gateway.Verify(g => g.Atualizar(os, It.IsAny<CancellationToken>()), Times.Once);
        outputPort.Verify(p => p.Ok(), Times.Once);
    }

    [Fact]
    public async Task RegistrarDiagnostico_ComItens_AtualizaEChamaOk()
    {
        var os = CriarOrdemServico(o => o.IniciarDiagnostico());
        var gateway = new Mock<IOrdemServicoGateway>();
        gateway.Setup(g => g.BuscarPorId(os.Id, It.IsAny<CancellationToken>())).ReturnsAsync(os);

        var outputPort = new Mock<IRegistrarDiagnosticoOutputPort>();
        var useCase = new RegistrarDiagnosticoUseCase(gateway.Object, outputPort.Object);

        var input = new RegistrarDiagnosticoInput(
            os.Id,
            [new RegistrarDiagnosticoServicoInput(Guid.NewGuid(), "Troca de óleo", 120m)],
            []);

        await useCase.Execute(input);

        outputPort.Verify(p => p.Ok(), Times.Once);
        os.ValorTotal.Should().Be(120m);
    }

    [Fact]
    public async Task AprovarPagamento_QuandoEncontrada_AtualizaEChamaOk()
    {
        var os = CriarOrdemServico(o =>
        {
            o.IniciarDiagnostico();
            o.RegistrarDiagnostico([new Domain.OrdensServico.Servicos.OrdemServicoServico(o.Id, Guid.NewGuid(), "Troca de óleo", 120m)], []);
        });

        var gateway = new Mock<IOrdemServicoGateway>();
        gateway.Setup(g => g.BuscarPorId(os.Id, It.IsAny<CancellationToken>())).ReturnsAsync(os);

        var outputPort = new Mock<IAprovarPagamentoOutputPort>();
        var useCase = new AprovarPagamentoUseCase(gateway.Object, outputPort.Object);

        await useCase.Execute(new AprovarPagamentoInput(os.Id));

        outputPort.Verify(p => p.Ok(), Times.Once);
        os.Status.Should().Be(Domain.OrdensServico.Enums.StatusOrdemServico.EmExecucao);
    }

    [Fact]
    public async Task Cancelar_QuandoEncontrada_AtualizaEChamaOk()
    {
        var os = CriarOrdemServico();
        var gateway = new Mock<IOrdemServicoGateway>();
        gateway.Setup(g => g.BuscarPorId(os.Id, It.IsAny<CancellationToken>())).ReturnsAsync(os);

        var outputPort = new Mock<ICancelarOutputPort>();
        var useCase = new CancelarUseCase(gateway.Object, outputPort.Object);

        await useCase.Execute(new CancelarInput(os.Id));

        outputPort.Verify(p => p.Ok(), Times.Once);
        os.Status.Should().Be(Domain.OrdensServico.Enums.StatusOrdemServico.Cancelada);
    }

    [Fact]
    public async Task Finalizar_QuandoNaoEncontrada_ChamaNaoEncontrado()
    {
        var gateway = new Mock<IOrdemServicoGateway>();
        gateway.Setup(g => g.BuscarPorId(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((OrdemServico?)null);

        var outputPort = new Mock<IFinalizarOutputPort>();
        var useCase = new FinalizarUseCase(gateway.Object, outputPort.Object);

        await useCase.Execute(new FinalizarInput(Guid.NewGuid()));

        outputPort.Verify(p => p.NaoEncontrado(), Times.Once);
    }
}
