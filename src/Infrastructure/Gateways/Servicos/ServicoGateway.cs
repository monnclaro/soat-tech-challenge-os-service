using Domain.Servicos;
using Domain.Servicos.Gateways;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using SharedKernel.DTOs;

namespace Infrastructure.Gateways.Servicos;

public class ServicoGateway : IServicoGateway
{
    private readonly OsServiceDbContext _db;
    public ServicoGateway(OsServiceDbContext db) => _db = db;

    public Task<Servico?> BuscarPorId(Guid id, CancellationToken ct = default) => _db.Servico.FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task<Dictionary<Guid, Servico>> BuscarPorIds(IReadOnlyList<Guid> ids, CancellationToken ct = default) =>
        await _db.Servico
            .AsNoTracking()
            .Where(s => ids.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, ct);

    public async Task<(IReadOnlyList<Servico> Items, int Total)> BuscarPaginado(PagedRequest p, CancellationToken ct = default)
    {
        var query = _db.Servico.AsNoTracking().OrderBy(s => s.Nome);
        var total = await query.CountAsync(ct);
        var items = await query.Skip((p.Pagina - 1) * p.Tamanho).Take(p.Tamanho).ToListAsync(ct);
        return (items, total);
    }

    public async Task Salvar(Servico servico, CancellationToken ct = default)
    {
        _db.Servico.Add(servico);
        await _db.SaveChangesAsync(ct);
    }

    public async Task Atualizar(Servico servico, CancellationToken ct = default) => await _db.SaveChangesAsync(ct);

    public async Task Remover(Servico servico, CancellationToken ct = default)
    {
        _db.Servico.Remove(servico);
        await _db.SaveChangesAsync(ct);
    }
}
