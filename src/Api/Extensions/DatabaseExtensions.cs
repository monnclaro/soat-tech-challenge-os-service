using System.Diagnostics.CodeAnalysis;
using Infrastructure.Database;
using Infrastructure.Seeders;
using Microsoft.EntityFrameworkCore;

namespace Api.Extensions;

// Bootstrap de startup (migrate + seed) — sem lógica de negócio a testar.
[ExcludeFromCodeCoverage]
public static class DatabaseExtensions
{
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<OsServiceDbContext>();
        await db.Database.MigrateAsync();

        var seeder = scope.ServiceProvider.GetRequiredService<IDatabaseSeeder>();
        await seeder.SeedAsync();
    }
}
