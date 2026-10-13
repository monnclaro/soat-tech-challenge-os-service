using Application.Common.Interfaces;
using Application.OrdensServico.EventHandlers;
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
}
