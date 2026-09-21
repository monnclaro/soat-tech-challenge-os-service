using Application.Common.Interfaces;
using Application.OrdensServico.EventHandlers;
using Domain.OrdensServico.Enums;
using Domain.OrdensServico.Events;
using Domain.OrdensServico.Produtos;
using Domain.OrdensServico.Servicos;
using Moq;
using Soat.Contracts.Saga;

namespace Tests.OrdensServico.Unit;

// Cobre os handlers que traduzem domain events em comandos publicados na saga
// (a "metade de saída" da orquestração) — a metade de entrada (consumers) é
// coberta indiretamente pelos testes de use case em SagaUseCasesTests, já que
// os consumers só adaptam o payload da mensagem para o use case existente.
public class SagaEventHandlersTests
{
    [Fact]
    public async Task EnviarIniciarDiagnosticoHandler_PublicaComando()
    {
        var bus = new Mock<ISagaCommandBus>();
        var handler = new EnviarIniciarDiagnosticoHandler(bus.Object);

        var idOrdemServico = Guid.NewGuid();
        var idCliente = Guid.NewGuid();
        var idVeiculo = Guid.NewGuid();

        await handler.Handle(new OrdemServicoAbertaDomainEvent(idOrdemServico, idCliente, idVeiculo), CancellationToken.None);

        bus.Verify(b => b.EnviarIniciarDiagnostico(idOrdemServico, idCliente, idVeiculo, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EnviarGerarOrcamentoHandler_MapeiaItensEPublicaComando()
    {
        var bus = new Mock<ISagaCommandBus>();
        var handler = new EnviarGerarOrcamentoHandler(bus.Object);

        var idOrdemServico = Guid.NewGuid();
        var idServico = Guid.NewGuid();
        var idProduto = Guid.NewGuid();

        var servicos = new List<OrdemServicoServico> { new(idOrdemServico, idServico, "Troca de óleo", 120m) };
        var produtos = new List<OrdemServicoProduto> { new(idOrdemServico, idProduto, "Óleo 5W30", 79.90m, 2) };

        await handler.Handle(new DiagnosticoRegistradoDomainEvent(idOrdemServico, servicos, produtos, 279.80m), CancellationToken.None);

        bus.Verify(b => b.EnviarGerarOrcamento(
            idOrdemServico,
            It.Is<IReadOnlyList<ItemServicoDiagnosticado>>(s => s.Count == 1 && s[0].IdServico == idServico && s[0].Valor == 120m),
            It.Is<IReadOnlyList<ItemProdutoDiagnosticado>>(p => p.Count == 1 && p[0].IdProduto == idProduto),
            279.80m,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EnviarIniciarExecucaoHandler_QuandoStatusEmExecucao_PublicaComando()
    {
        var bus = new Mock<ISagaCommandBus>();
        var handler = new EnviarIniciarExecucaoHandler(bus.Object);

        var idOrdemServico = Guid.NewGuid();
        await handler.Handle(new OrdemServicoStatusAlteradoDomainEvent(idOrdemServico, StatusOrdemServico.EmExecucao), CancellationToken.None);

        bus.Verify(b => b.EnviarIniciarExecucao(idOrdemServico, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(StatusOrdemServico.Recebida)]
    [InlineData(StatusOrdemServico.EmDiagnostico)]
    [InlineData(StatusOrdemServico.AguardandoAprovacao)]
    [InlineData(StatusOrdemServico.Finalizada)]
    [InlineData(StatusOrdemServico.Entregue)]
    [InlineData(StatusOrdemServico.Cancelada)]
    public async Task EnviarIniciarExecucaoHandler_QuandoOutroStatus_NaoPublicaComando(StatusOrdemServico status)
    {
        var bus = new Mock<ISagaCommandBus>();
        var handler = new EnviarIniciarExecucaoHandler(bus.Object);

        await handler.Handle(new OrdemServicoStatusAlteradoDomainEvent(Guid.NewGuid(), status), CancellationToken.None);

        bus.Verify(b => b.EnviarIniciarExecucao(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
