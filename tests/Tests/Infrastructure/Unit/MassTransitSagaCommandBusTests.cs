using Infrastructure.Messaging;
using MassTransit;
using Moq;
using Soat.Contracts.Saga;

namespace Tests.Infrastructure.Unit;

// Cobre o adaptador do port de saída da saga (ISagaCommandBus) sobre o
// IPublishEndpoint do MassTransit — cada método só precisa publicar o
// comando certo, com os dados certos.
public class MassTransitSagaCommandBusTests
{
    [Fact]
    public async Task EnviarIniciarDiagnostico_PublicaComandoCorreto()
    {
        var publishEndpoint = new Mock<IPublishEndpoint>();
        var bus = new MassTransitSagaCommandBus(publishEndpoint.Object);

        var idOs = Guid.NewGuid();
        var idCliente = Guid.NewGuid();
        var idVeiculo = Guid.NewGuid();

        await bus.EnviarIniciarDiagnostico(idOs, idCliente, idVeiculo);

        publishEndpoint.Verify(p => p.Publish(
            It.Is<IniciarDiagnostico>(c => c.IdOrdemServico == idOs && c.IdCliente == idCliente && c.IdVeiculo == idVeiculo),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EnviarGerarOrcamento_PublicaComandoComItensETotal()
    {
        var publishEndpoint = new Mock<IPublishEndpoint>();
        var bus = new MassTransitSagaCommandBus(publishEndpoint.Object);

        var idOs = Guid.NewGuid();
        var servicos = new List<ItemServicoDiagnosticado> { new(Guid.NewGuid(), "Troca de óleo", 120m) };
        var produtos = new List<ItemProdutoDiagnosticado> { new(Guid.NewGuid(), "Óleo 5W30", 79.90m, 2m) };

        await bus.EnviarGerarOrcamento(idOs, servicos, produtos, 279.80m);

        publishEndpoint.Verify(p => p.Publish(
            It.Is<GerarOrcamento>(c => c.IdOrdemServico == idOs && c.Servicos.Count == 1 && c.Produtos.Count == 1 && c.ValorTotal == 279.80m),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EnviarIniciarExecucao_PublicaComandoCorreto()
    {
        var publishEndpoint = new Mock<IPublishEndpoint>();
        var bus = new MassTransitSagaCommandBus(publishEndpoint.Object);

        var idOs = Guid.NewGuid();

        await bus.EnviarIniciarExecucao(idOs);

        publishEndpoint.Verify(p => p.Publish(
            It.Is<IniciarExecucao>(c => c.IdOrdemServico == idOs),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
