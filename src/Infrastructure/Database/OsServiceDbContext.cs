using Domain.Clientes;
using Domain.Clientes.Veiculos;
using Domain.Common;
using Domain.Common.Events;
using Domain.OrdensServico;
using Domain.OrdensServico.Historico;
using Domain.OrdensServico.Produtos;
using Domain.OrdensServico.Servicos;
using Domain.Produtos;
using Domain.Servicos;
using Domain.Usuarios;
using Infrastructure.DomainEvents;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database;

public class OsServiceDbContext : DbContext
{
    private readonly IDomainEventsDispatcher _dispatcher;

    public OsServiceDbContext(
        DbContextOptions<OsServiceDbContext> options,
        IDomainEventsDispatcher dispatcher) : base(options)
    {
        _dispatcher = dispatcher;
    }

    public DbSet<OrdemServico> OrdemServico { get; set; }
    public DbSet<OrdemServicoServico> OrdemServicoServico { get; set; }
    public DbSet<OrdemServicoProduto> OrdemServicoProduto { get; set; }
    public DbSet<HistoricoStatusOrdemServico> HistoricoStatusOrdemServico { get; set; }
    public DbSet<Cliente> Cliente { get; set; }
    public DbSet<Veiculo> Veiculo { get; set; }
    public DbSet<Servico> Servico { get; set; }
    public DbSet<Produto> Produto { get; set; }
    public DbSet<Usuario> Usuario { get; set; }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        int result = await base.SaveChangesAsync(cancellationToken);

        await PublishDomainEventsAsync();

        return result;
    }

    private async Task PublishDomainEventsAsync()
    {
        var domainEvents = ChangeTracker
            .Entries<Entity>()
            .Select(entry => entry.Entity)
            .SelectMany(entity =>
            {
                List<IDomainEvent> domainEvents = entity.DomainEvents;

                entity.ClearDomainEvents();

                return domainEvents;
            })
            .ToList();

        await _dispatcher.DispatchAsync(domainEvents);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OsServiceDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
