using System.Diagnostics.CodeAnalysis;
using Infrastructure.Database.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Infrastructure.Database;

/// <summary>
/// Usado só em design-time (ex.: "dotnet ef migrations add") — a connection string real
/// vem de appsettings/ambiente via <see cref="DependencyInjection.AddDatabase"/> em runtime.
/// </summary>
[ExcludeFromCodeCoverage]
public class OsServiceDbContextFactory : IDesignTimeDbContextFactory<OsServiceDbContext>
{
    public OsServiceDbContext CreateDbContext(string[] args)
    {
        // NOSONAR: não é uma credencial real — só o Postgres local de desenvolvimento
        // (mesmo valor do compose.yaml/.env.example), usado apenas por ferramentas de
        // design-time ("dotnet ef migrations add"), nunca em runtime do serviço.
        var options = new DbContextOptionsBuilder<OsServiceDbContext>()
            .UseNpgsql("Host=localhost;Database=soat_os;Username=postgres;Password=postgres") // NOSONAR
            .Options;

        return new OsServiceDbContext(options, new NoopDomainEventsDispatcher());
    }
}
