using Domain.Clientes;
using Domain.Clientes.Gateways;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using SharedKernel.DTOs;

namespace Infrastructure.Gateways.Clientes;

public class ClienteGateway : IClienteGateway
{
    private readonly OsServiceDbContext _db;

    public ClienteGateway(OsServiceDbContext db) => _db = db;

    public Task<Cliente?> BuscarPorId(Guid id, CancellationToken ct = default) => _db.Cliente.FirstOrDefaultAsync(c => c.Id == id, ct);
    public Task<Cliente?> BuscarPorDocumento(string documento, CancellationToken ct = default) => _db.Cliente.FirstOrDefaultAsync(c => c.Documento == documento, ct);

    public Task<Cliente?> BuscarComVeiculos(Guid id, CancellationToken ct = default) =>
        _db.Cliente
            .Include(c => c.Veiculos)
            .AsSplitQuery()
            .FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<bool> ExisteComDocumento(string documento, CancellationToken ct = default) =>
        _db.Cliente.AsNoTracking().AnyAsync(c => c.Documento == documento, ct);

    public async Task<(IReadOnlyList<Cliente> Items, int Total)> BuscarPaginado(PagedRequest p, CancellationToken ct = default)
    {
        var query = _db.Cliente.AsNoTracking().OrderBy(c => c.DataCriacao);
        var total = await query.CountAsync(ct);
        var items = await query.Skip((p.Pagina - 1) * p.Tamanho).Take(p.Tamanho).ToListAsync(ct);
        return (items, total);
    }

    public async Task Salvar(Cliente cliente, CancellationToken ct = default)
    {
        _db.Cliente.Add(cliente);
        await _db.SaveChangesAsync(ct);
    }

    public async Task Atualizar(Cliente cliente, CancellationToken ct = default) =>
        await _db.SaveChangesAsync(ct);

    public async Task Remover(Cliente cliente, CancellationToken ct = default)
    {
        _db.Cliente.Remove(cliente);
        await _db.SaveChangesAsync(ct);
    }
}
