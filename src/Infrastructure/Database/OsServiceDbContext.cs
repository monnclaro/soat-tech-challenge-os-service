using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database;

public class OsServiceDbContext(DbContextOptions<OsServiceDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OsServiceDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
