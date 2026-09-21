using Application.Servicos.DTOs;
using Application.Servicos.UseCases.AtualizarServico;
using Application.Servicos.UseCases.BuscarListaPaginada;
using Application.Servicos.UseCases.BuscarServico;
using Application.Servicos.UseCases.InserirServico;
using Application.Servicos.UseCases.RemoverServico;
using Domain.Servicos;
using Domain.Servicos.Gateways;
using FluentAssertions;
using Moq;
using SharedKernel.DTOs;

namespace Tests.Servicos.Unit;

public class ServicoUseCasesTests
{
    private static Servico CriarServico(string nome = "Troca de Óleo")
    {
        var servico = new Servico();
        servico.Inserir(nome, "Substituição do óleo do motor", 120m);
        return servico;
    }

    [Fact]
    public async Task Inserir_SalvaEChamaOk()
    {
        var gateway = new Mock<IServicoGateway>();
        var outputPort = new Mock<IInserirServicoOutputPort>();
        var useCase = new InserirServicoUseCase(gateway.Object, outputPort.Object);

        await useCase.Execute(new InserirServicoInput("Troca de Óleo", "Substituição do óleo do motor", 120m));

        gateway.Verify(g => g.Salvar(It.IsAny<Servico>(), It.IsAny<CancellationToken>()), Times.Once);
        outputPort.Verify(p => p.Ok(It.Is<ServicoOutput>(o => o.Nome == "Troca de Óleo")), Times.Once);
    }

    [Fact]
    public async Task AtualizarServico_QuandoNaoEncontrado_ChamaNaoEncontrado()
    {
        var gateway = new Mock<IServicoGateway>();
        gateway.Setup(g => g.BuscarPorId(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Servico?)null);

        var outputPort = new Mock<IAtualizarServicoOutputPort>();
        var useCase = new AtualizarServicoUseCase(gateway.Object, outputPort.Object);

        await useCase.Execute(new AtualizarServicoInput(Guid.NewGuid(), "Novo Nome", "Nova Descrição", 150m));

        outputPort.Verify(p => p.NaoEncontrado(), Times.Once);
    }

    [Fact]
    public async Task AtualizarServico_QuandoEncontrado_AtualizaEChamaOk()
    {
        var servico = CriarServico();
        var gateway = new Mock<IServicoGateway>();
        gateway.Setup(g => g.BuscarPorId(servico.Id, It.IsAny<CancellationToken>())).ReturnsAsync(servico);

        var outputPort = new Mock<IAtualizarServicoOutputPort>();
        var useCase = new AtualizarServicoUseCase(gateway.Object, outputPort.Object);

        await useCase.Execute(new AtualizarServicoInput(servico.Id, "Novo Nome", "Nova Descrição", 150m));

        servico.Nome.Should().Be("Novo Nome");
        gateway.Verify(g => g.Atualizar(servico, It.IsAny<CancellationToken>()), Times.Once);
        outputPort.Verify(p => p.Ok(It.IsAny<ServicoOutput>()), Times.Once);
    }

    [Fact]
    public async Task BuscarServico_QuandoNaoEncontrado_ChamaNaoEncontrado()
    {
        var gateway = new Mock<IServicoGateway>();
        gateway.Setup(g => g.BuscarPorId(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Servico?)null);

        var outputPort = new Mock<IBuscarServicoOutputPort>();
        var useCase = new BuscarServicoUseCase(gateway.Object, outputPort.Object);

        await useCase.Execute(new BuscarServicoInput(Guid.NewGuid()));

        outputPort.Verify(p => p.NaoEncontrado(), Times.Once);
    }

    [Fact]
    public async Task BuscarServico_QuandoEncontrado_ChamaOk()
    {
        var servico = CriarServico();
        var gateway = new Mock<IServicoGateway>();
        gateway.Setup(g => g.BuscarPorId(servico.Id, It.IsAny<CancellationToken>())).ReturnsAsync(servico);

        var outputPort = new Mock<IBuscarServicoOutputPort>();
        var useCase = new BuscarServicoUseCase(gateway.Object, outputPort.Object);

        await useCase.Execute(new BuscarServicoInput(servico.Id));

        outputPort.Verify(p => p.Ok(It.Is<ServicoOutput>(o => o.Id == servico.Id)), Times.Once);
    }

    [Fact]
    public async Task BuscarListaPaginada_ChamaOkComResultadoPaginado()
    {
        var servico = CriarServico();
        var gateway = new Mock<IServicoGateway>();
        gateway.Setup(g => g.BuscarPaginado(It.IsAny<PagedRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Servico> { servico }, 1));

        var outputPort = new Mock<IBuscarListaPaginadaOutputPort>();
        var useCase = new BuscarListaPaginadaUseCase(gateway.Object, outputPort.Object);

        await useCase.Execute(new BuscarListaPaginadaInput(new PagedRequest(1, 10)));

        outputPort.Verify(p => p.Ok(It.Is<PagedResult<ServicoOutput>>(r => r.TotalCount == 1 && r.Items.Count == 1)), Times.Once);
    }

    [Fact]
    public async Task RemoverServico_QuandoNaoEncontrado_ChamaOkIdempotente()
    {
        var gateway = new Mock<IServicoGateway>();
        gateway.Setup(g => g.BuscarPorId(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Servico?)null);

        var outputPort = new Mock<IRemoverServicoOutputPort>();
        var useCase = new RemoverServicoUseCase(gateway.Object, outputPort.Object);

        await useCase.Execute(new RemoverServicoInput(Guid.NewGuid()));

        outputPort.Verify(p => p.Ok(), Times.Once);
        gateway.Verify(g => g.Remover(It.IsAny<Servico>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RemoverServico_QuandoEncontrado_RemoveEChamaOk()
    {
        var servico = CriarServico();
        var gateway = new Mock<IServicoGateway>();
        gateway.Setup(g => g.BuscarPorId(servico.Id, It.IsAny<CancellationToken>())).ReturnsAsync(servico);

        var outputPort = new Mock<IRemoverServicoOutputPort>();
        var useCase = new RemoverServicoUseCase(gateway.Object, outputPort.Object);

        await useCase.Execute(new RemoverServicoInput(servico.Id));

        gateway.Verify(g => g.Remover(servico, It.IsAny<CancellationToken>()), Times.Once);
        outputPort.Verify(p => p.Ok(), Times.Once);
    }
}
