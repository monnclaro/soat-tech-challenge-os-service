using Infrastructure.Database;
using Infrastructure.Seeders.Clientes;
using Infrastructure.Seeders.OrdensServico;
using Infrastructure.Seeders.Produtos;
using Infrastructure.Seeders.Servicos;
using Infrastructure.Seeders.Usuarios;

namespace Infrastructure.Seeders;

public sealed class DatabaseSeeder : IDatabaseSeeder
{
    private readonly OsServiceDbContext _db;

    public DatabaseSeeder(OsServiceDbContext db)
    {
        _db = db;
    }

    public async Task SeedAsync()
    {
        await ClienteSeeder.SeedAsync(_db);
        await ProdutoSeeder.SeedAsync(_db);
        await ServicoSeeder.SeedAsync(_db);
        await UsuarioSeeder.SeedAsync(_db);
        await OrdensServicoSeeder.SeedAsync(_db);
    }
}
