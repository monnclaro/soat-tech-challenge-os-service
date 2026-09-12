using Application.Common.Interfaces;
using Domain.Common.Events;
using Domain.OrdensServico.Events;
using FluentAssertions;
using Infrastructure.DomainEvents;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.Infrastructure.Unit;

public class DomainEventsDispatcherTests
{
    private sealed class ContadorHandler : IDomainEventHandler<OrdemServicoAbertaDomainEvent>
    {
        public int Chamadas { get; private set; }

        public Task Handle(OrdemServicoAbertaDomainEvent domainEvent, CancellationToken cancellationToken)
        {
            Chamadas++;
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task DispatchAsync_ComHandlerRegistrado_InvocaOHandlerParaCadaEvento()
    {
        var handler = new ContadorHandler();
        var services = new ServiceCollection();
        services.AddSingleton<IDomainEventHandler<OrdemServicoAbertaDomainEvent>>(handler);
        var provider = services.BuildServiceProvider();

        var dispatcher = new DomainEventsDispatcher(provider);

        var evento1 = new OrdemServicoAbertaDomainEvent(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        var evento2 = new OrdemServicoAbertaDomainEvent(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        await dispatcher.DispatchAsync([evento1, evento2]);

        handler.Chamadas.Should().Be(2);
    }

    [Fact]
    public async Task DispatchAsync_SemHandlerRegistrado_NaoLancaExcecao()
    {
        var provider = new ServiceCollection().BuildServiceProvider();
        var dispatcher = new DomainEventsDispatcher(provider);

        var act = () => dispatcher.DispatchAsync([new OrdemServicoAbertaDomainEvent(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid())]);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task DispatchAsync_SemEventos_NaoInvocaHandler()
    {
        var handler = new ContadorHandler();
        var services = new ServiceCollection();
        services.AddSingleton<IDomainEventHandler<OrdemServicoAbertaDomainEvent>>(handler);
        var provider = services.BuildServiceProvider();

        var dispatcher = new DomainEventsDispatcher(provider);

        await dispatcher.DispatchAsync([]);

        handler.Chamadas.Should().Be(0);
    }
}
