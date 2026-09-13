using Domain.Usuarios;
using Domain.Usuarios.Gateways;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Gateways.Usuarios;

public class UsuarioGateway : IUsuarioGateway
{
    private readonly OsServiceDbContext _db;

    public UsuarioGateway(OsServiceDbContext db) => _db = db;

    public Task<Usuario?> BuscarPorEmail(string email, CancellationToken ct = default) => _db.Usuario
            .AsNoTracking()
            .Include(u => u.Roles)
            .AsSplitQuery()
            .Where(u => u.Email == email)
            .FirstOrDefaultAsync(ct);

    public async Task Salvar(Usuario usuario, CancellationToken ct)
    {
        _db.Usuario.Add(usuario);
        await _db.SaveChangesAsync(ct);
    }
}
