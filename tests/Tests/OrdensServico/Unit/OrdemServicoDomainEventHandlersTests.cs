using Application.OrdensServico.EventHandlers;
using Application.Produtos.UseCases.DecrementarEstoque;
using Domain.OrdensServico.Enums;
using Domain.OrdensServico.Events;
using Domain.OrdensServico.Historico;
using Domain.OrdensServico.Historico.Gateways;
using Domain.OrdensServico.Produtos;
using Domain.Produtos;
using Domain.Produtos.Gateways;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.OrdensServico.Unit;

// Cobre os handlers internos que reagem aos domain events da OS mas não publicam
// comandos da saga (esses já são cobertos por SagaEventHandlersTests): decremento
// de estoque ao finalizar, log estruturado e persistência do histórico de status.
public class OrdemServicoDomainEventHandlersTests
{
    [Fact]
    public async Task OrdemServicoEventHandler_MapeiaProdutosEChamaDecrementarEstoque()
    {
        var idOrdemServico = Guid.NewGuid();
        var idProduto = Guid.NewGuid();
        var produtos = new List<OrdemServicoProduto> { new(idOrdemServico, idProduto, "Óleo 5W30", 79.90m, 2m) };

        var gateway = new Mock<IProdutoGateway>();
        gateway.Setup(g => g.BuscarPorIds(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyList<Produto>)[]);

        var outputPort = new Mock<IDecrementarEstoqueOutputPort>();
        var useCase = new DecrementarEstoqueUseCase(gateway.Object, outputPort.Object);
        var handler = new OrdemServicoEventHandler(useCase, outputPort.Object);

        await handler.Handle(new OrdemServicoFinalizadaDomainEvent(idOrdemServico, produtos), CancellationToken.None);

        gateway.Verify(g => g.BuscarPorIds(
            It.Is<IReadOnlyList<Guid>>(ids => ids.Count == 1 && ids[0] == idProduto),
            It.IsAny<CancellationToken>()), Times.Once);
        outputPort.Verify(p => p.Ok(), Times.Once);
    }

    [Fact]
    public async Task OrdemServicoStatusAlteradoLogHandler_RegistraLogInformativo()
    {
        // Moq não consegue gerar proxy para ILogger<T> quando T é um tipo internal
        // (Castle DynamicProxy exige acesso ao tipo pela assembly dinâmica, não só
        // via InternalsVisibleTo("Tests")) — por isso um fake simples no lugar do Mock.
        var logger = new FakeLogger<OrdemServicoStatusAlteradoLogHandler>();
        var handler = new OrdemServicoStatusAlteradoLogHandler(logger);

        await handler.Handle(new OrdemServicoStatusAlteradoDomainEvent(Guid.NewGuid(), StatusOrdemServico.EmDiagnostico), CancellationToken.None);

        logger.Registros.Should().ContainSingle(r => r.Level == LogLevel.Information);
    }

    private sealed class FakeLogger<T> : ILogger<T>
    {
        public List<(LogLevel Level, string Message)> Registros { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            Registros.Add((logLevel, formatter(state, exception)));
        }
    }

    [Fact]
    public async Task OrdemServicoStatusAlteradoHistoricoHandler_PersisteHistorico()
    {
        var gateway = new Mock<IHistoricoStatusOrdemServicoGateway>();
        var handler = new OrdemServicoStatusAlteradoHistoricoHandler(gateway.Object);

        var idOrdemServico = Guid.NewGuid();
        await handler.Handle(new OrdemServicoStatusAlteradoDomainEvent(idOrdemServico, StatusOrdemServico.Cancelada), CancellationToken.None);

        gateway.Verify(g => g.Salvar(
            It.Is<HistoricoStatusOrdemServico>(h => h.IdOrdemServico == idOrdemServico && h.Status == StatusOrdemServico.Cancelada),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
