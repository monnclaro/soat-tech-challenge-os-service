using Application.Clientes.Veiculos.UseCases;
using Application.Clientes.Veiculos.UseCases.AtualizarVeiculo;
using Application.Clientes.Veiculos.UseCases.BuscarListaPaginada;
using Application.Clientes.Veiculos.UseCases.BuscarVeiculo;
using Application.Clientes.Veiculos.UseCases.InserirVeiculo;
using Application.Clientes.Veiculos.UseCases.RemoverVeiculo;
using Domain.Clientes;
using Domain.Clientes.Gateways;
using Domain.Clientes.Veiculos;
using Domain.Clientes.Veiculos.Gateways;
using Domain.Clientes.Veiculos.ValueObjects;
using FluentAssertions;
using Moq;
using SharedKernel.DTOs;

namespace Tests.Clientes.Veiculos.Unit;

public class VeiculoUseCasesTests
{
    private static Veiculo CriarVeiculo(Guid? idCliente = null, string placa = "ABC1234")
    {
        var veiculo = new Veiculo();
        veiculo.Inserir(idCliente ?? Guid.NewGuid(), Placa.Criar(placa), "Toyota", "Corolla", 2020);
        return veiculo;
    }

    [Fact]
    public async Task Inserir_QuandoClienteNaoExiste_ChamaClienteNaoEncontrado()
    {
        var clienteGateway = new Mock<IClienteGateway>();
        clienteGateway.Setup(g => g.BuscarPorId(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Cliente?)null);

        var veiculoGateway = new Mock<IVeiculoGateway>();
        var outputPort = new Mock<IInserirVeiculoOutputPort>();
        var useCase = new InserirVeiculoUseCase(veiculoGateway.Object, clienteGateway.Object, outputPort.Object);

        await useCase.Execute(new InserirVeiculoInput(Guid.NewGuid(), "ABC1234", "Toyota", "Corolla", 2020));

        outputPort.Verify(p => p.ClienteNaoEncontrado(), Times.Once);
    }

    [Fact]
    public async Task Inserir_QuandoPlacaEmUso_ChamaPlacaDuplicada()
    {
        var cliente = new Cliente();
        cliente.Inserir("João Silva", Domain.Clientes.ValueObjects.DocumentoCliente.Criar("12345678909"));

        var clienteGateway = new Mock<IClienteGateway>();
        clienteGateway.Setup(g => g.BuscarPorId(cliente.Id, It.IsAny<CancellationToken>())).ReturnsAsync(cliente);

        var veiculoGateway = new Mock<IVeiculoGateway>();
        veiculoGateway.Setup(g => g.ExisteComPlaca(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var outputPort = new Mock<IInserirVeiculoOutputPort>();
        var useCase = new InserirVeiculoUseCase(veiculoGateway.Object, clienteGateway.Object, outputPort.Object);

        await useCase.Execute(new InserirVeiculoInput(cliente.Id, "ABC1234", "Toyota", "Corolla", 2020));

        outputPort.Verify(p => p.PlacaDuplicada(It.IsAny<string>()), Times.Once);
        veiculoGateway.Verify(g => g.Inserir(It.IsAny<Veiculo>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Inserir_ComDadosValidos_SalvaEChamaOk()
    {
        var cliente = new Cliente();
        cliente.Inserir("João Silva", Domain.Clientes.ValueObjects.DocumentoCliente.Criar("12345678909"));

        var clienteGateway = new Mock<IClienteGateway>();
        clienteGateway.Setup(g => g.BuscarPorId(cliente.Id, It.IsAny<CancellationToken>())).ReturnsAsync(cliente);

        var veiculoGateway = new Mock<IVeiculoGateway>();
        veiculoGateway.Setup(g => g.ExisteComPlaca(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var outputPort = new Mock<IInserirVeiculoOutputPort>();
        var useCase = new InserirVeiculoUseCase(veiculoGateway.Object, clienteGateway.Object, outputPort.Object);

        await useCase.Execute(new InserirVeiculoInput(cliente.Id, "ABC1234", "Toyota", "Corolla", 2020));

        veiculoGateway.Verify(g => g.Inserir(It.IsAny<Veiculo>(), It.IsAny<CancellationToken>()), Times.Once);
        outputPort.Verify(p => p.Ok(It.Is<VeiculoOutput>(o => o.Placa == "ABC1234")), Times.Once);
    }

    [Fact]
    public async Task AtualizarVeiculo_QuandoNaoEncontrado_ChamaNaoEncontrado()
    {
        var gateway = new Mock<IVeiculoGateway>();
        gateway.Setup(g => g.BuscarPorId(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Veiculo?)null);

        var outputPort = new Mock<IAtualizarVeiculoOutputPort>();
        var useCase = new AtualizarVeiculoUseCase(gateway.Object, outputPort.Object);

        await useCase.Execute(new AtualizarVeiculoInput(Guid.NewGuid(), "ABC1234", "Toyota", "Corolla", 2020));

        outputPort.Verify(p => p.NaoEncontrado(), Times.Once);
    }

    [Fact]
    public async Task AtualizarVeiculo_QuandoPlacaEmUsoPorOutroVeiculo_ChamaPlacaDuplicada()
    {
        var veiculo = CriarVeiculo();
        var gateway = new Mock<IVeiculoGateway>();
        gateway.Setup(g => g.BuscarPorId(veiculo.Id, It.IsAny<CancellationToken>())).ReturnsAsync(veiculo);
        gateway.Setup(g => g.ExisteComPlacaExcetoId(It.IsAny<string>(), veiculo.Id, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var outputPort = new Mock<IAtualizarVeiculoOutputPort>();
        var useCase = new AtualizarVeiculoUseCase(gateway.Object, outputPort.Object);

        await useCase.Execute(new AtualizarVeiculoInput(veiculo.Id, "DEF5678", "Honda", "Civic", 2021));

        outputPort.Verify(p => p.PlacaDuplicada(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task AtualizarVeiculo_ComDadosValidos_AtualizaEChamaOk()
    {
        var veiculo = CriarVeiculo();
        var gateway = new Mock<IVeiculoGateway>();
        gateway.Setup(g => g.BuscarPorId(veiculo.Id, It.IsAny<CancellationToken>())).ReturnsAsync(veiculo);
        gateway.Setup(g => g.ExisteComPlacaExcetoId(It.IsAny<string>(), veiculo.Id, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var outputPort = new Mock<IAtualizarVeiculoOutputPort>();
        var useCase = new AtualizarVeiculoUseCase(gateway.Object, outputPort.Object);

        await useCase.Execute(new AtualizarVeiculoInput(veiculo.Id, "DEF5678", "Honda", "Civic", 2021));

        veiculo.Marca.Should().Be("Honda");
        gateway.Verify(g => g.Atualizar(veiculo, It.IsAny<CancellationToken>()), Times.Once);
        outputPort.Verify(p => p.Ok(It.IsAny<VeiculoOutput>()), Times.Once);
    }

    [Fact]
    public async Task BuscarVeiculo_QuandoNaoEncontrado_ChamaNaoEncontrado()
    {
        var gateway = new Mock<IVeiculoGateway>();
        gateway.Setup(g => g.BuscarPorId(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Veiculo?)null);

        var outputPort = new Mock<IBuscarVeiculoOutputPort>();
        var useCase = new BuscarVeiculoUseCase(gateway.Object, outputPort.Object);

        await useCase.Execute(new BuscarVeiculoInput(Guid.NewGuid()));

        outputPort.Verify(p => p.NaoEncontrado(), Times.Once);
    }

    [Fact]
    public async Task BuscarVeiculo_QuandoEncontrado_ChamaOk()
    {
        var veiculo = CriarVeiculo();
        var gateway = new Mock<IVeiculoGateway>();
        gateway.Setup(g => g.BuscarPorId(veiculo.Id, It.IsAny<CancellationToken>())).ReturnsAsync(veiculo);

        var outputPort = new Mock<IBuscarVeiculoOutputPort>();
        var useCase = new BuscarVeiculoUseCase(gateway.Object, outputPort.Object);

        await useCase.Execute(new BuscarVeiculoInput(veiculo.Id));

        outputPort.Verify(p => p.Ok(It.Is<VeiculoOutput>(o => o.Id == veiculo.Id)), Times.Once);
    }

    [Fact]
    public async Task BuscarListaPaginada_ChamaOkComResultadoPaginado()
    {
        var idCliente = Guid.NewGuid();
        var veiculo = CriarVeiculo(idCliente);
        var gateway = new Mock<IVeiculoGateway>();
        gateway.Setup(g => g.BuscarPaginadoPorCliente(idCliente, It.IsAny<PagedRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Veiculo> { veiculo }, 1));

        var outputPort = new Mock<IBuscarListaPaginadaVeiculoOutputPort>();
        var useCase = new BuscarListaPaginadaVeiculoUseCase(gateway.Object, outputPort.Object);

        await useCase.Execute(new BuscarListaPaginadaVeiculoInput(idCliente, new PagedRequest(1, 10)));

        outputPort.Verify(p => p.Ok(It.Is<PagedResult<VeiculoOutput>>(r => r.TotalCount == 1 && r.Items.Count == 1)), Times.Once);
    }

    [Fact]
    public async Task RemoverVeiculo_QuandoNaoEncontrado_ChamaNaoEncontrado()
    {
        var gateway = new Mock<IVeiculoGateway>();
        gateway.Setup(g => g.BuscarPorId(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Veiculo?)null);

        var outputPort = new Mock<IRemoverVeiculoOutputPort>();
        var useCase = new RemoverVeiculoUseCase(gateway.Object, outputPort.Object);

        await useCase.Execute(new RemoverVeiculoInput(Guid.NewGuid()));

        outputPort.Verify(p => p.NaoEncontrado(), Times.Once);
    }

    [Fact]
    public async Task RemoverVeiculo_QuandoEncontrado_RemoveEChamaOk()
    {
        var veiculo = CriarVeiculo();
        var gateway = new Mock<IVeiculoGateway>();
        gateway.Setup(g => g.BuscarPorId(veiculo.Id, It.IsAny<CancellationToken>())).ReturnsAsync(veiculo);

        var outputPort = new Mock<IRemoverVeiculoOutputPort>();
        var useCase = new RemoverVeiculoUseCase(gateway.Object, outputPort.Object);

        await useCase.Execute(new RemoverVeiculoInput(veiculo.Id));

        gateway.Verify(g => g.Remover(veiculo, It.IsAny<CancellationToken>()), Times.Once);
        outputPort.Verify(p => p.Ok(), Times.Once);
    }
}
