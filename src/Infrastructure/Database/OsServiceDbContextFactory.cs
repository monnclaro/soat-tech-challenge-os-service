using Infrastructure.Database.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Infrastructure.Database;

/// <summary>
/// Usado só em design-time (ex.: "dotnet ef migrations add") — a connection string real
/// vem de appsettings/ambiente via <see cref="DependencyInjection.AddDatabase"/> em runtime.
/// </summary>
public class OsServiceDbContextFactory : IDesignTimeDbContextFactory<OsServiceDbContext>
{
    public OsServiceDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<OsServiceDbContext>()
            .UseNpgsql("Host=localhost;Database=soat_os;Username=postgres;Password=postgres")
            .Options;

        return new OsServiceDbContext(options, new NoopDomainEventsDispatcher());
    }
}
