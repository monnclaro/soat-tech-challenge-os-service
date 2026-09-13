using Domain.Usuarios;
using Domain.Usuarios.Roles;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Seeders.Usuarios;

public static class UsuarioSeeder
{
    public static async Task SeedAsync(OsServiceDbContext context)
    {
        if (await context.Usuario.AnyAsync()) return;

        var adminUser = new Usuario("Admin", "admin@gmail.com", BCrypt.Net.BCrypt.HashPassword("123"), "52998224725");
        adminUser.AdicionarRoles(new List<UsuarioRole>() { new("Admin") });

        context.Usuario.Add(adminUser);
        await context.SaveChangesAsync();
    }
}
