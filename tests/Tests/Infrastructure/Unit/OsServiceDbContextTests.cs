using Domain.OrdensServico;
using FluentAssertions;
using Infrastructure.Database;
using Infrastructure.DomainEvents;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Tests.Infrastructure.Unit;

// SaveChangesAsync é sobrescrito pra despachar os domain events acumulados nas
// entidades rastreadas depois de persistir — cobre esse comportamento com o
// provider EF Core InMemory (sem precisar de um Postgres real).
public class OsServiceDbContextTests
{
    private static OsServiceDbContext CriarContexto(IDomainEventsDispatcher dispatcher)
    {
        var options = new DbContextOptionsBuilder<OsServiceDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new OsServiceDbContext(options, dispatcher);
    }

    [Fact]
    public async Task SaveChangesAsync_DespachaEventosDeEntidadesRastreadasELimpaOsEventos()
    {
        var dispatcher = new Mock<IDomainEventsDispatcher>();
        await using var context = CriarContexto(dispatcher.Object);

        var os = new OrdemServico();
        os.Inserir(Guid.NewGuid(), Guid.NewGuid());
        os.DomainEvents.Should().NotBeEmpty();

        context.OrdemServico.Add(os);
        await context.SaveChangesAsync();

        dispatcher.Verify(d => d.DispatchAsync(
            It.Is<IEnumerable<Domain.Common.Events.IDomainEvent>>(events => events.Any()),
            It.IsAny<CancellationToken>()), Times.Once);

        os.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public async Task SaveChangesAsync_SemEntidadesRastreadas_DespachaListaVazia()
    {
        var dispatcher = new Mock<IDomainEventsDispatcher>();
        await using var context = CriarContexto(dispatcher.Object);

        await context.SaveChangesAsync();

        dispatcher.Verify(d => d.DispatchAsync(
            It.Is<IEnumerable<Domain.Common.Events.IDomainEvent>>(events => !events.Any()),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
