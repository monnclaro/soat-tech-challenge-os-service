using System.Diagnostics.CodeAnalysis;
using Domain.Common.Events;
using Infrastructure.DomainEvents;

namespace Infrastructure.Database.Helpers;

// Usado só pela OsServiceDbContextFactory (design-time) — sem lógica de negócio a testar.
[ExcludeFromCodeCoverage]
public class NoopDomainEventsDispatcher : IDomainEventsDispatcher
{
    public Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default) => Task.CompletedTask;
}
