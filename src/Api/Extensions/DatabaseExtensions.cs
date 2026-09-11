using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Api.Extensions;

public static class DatabaseExtensions
{
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<OsServiceDbContext>();
        await db.Database.MigrateAsync();

        // Seeder por domínio entra aqui conforme as entidades forem portadas
        // (ver PLANO-FASE-4-MICROSSERVICOS.md) — mesmo padrão do monolito
        // (IDatabaseSeeder / DatabaseSeeder em Infrastructure/Seeders).
    }
}
