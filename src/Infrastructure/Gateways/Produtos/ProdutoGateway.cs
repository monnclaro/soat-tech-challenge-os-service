using Domain.Produtos;
using Domain.Produtos.Gateways;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using SharedKernel.DTOs;

namespace Infrastructure.Gateways.Produtos;

public class ProdutoGateway : IProdutoGateway
{
    private readonly OsServiceDbContext _db;

    public ProdutoGateway(OsServiceDbContext db) => _db = db;

    public Task<Produto?> BuscarPorId(Guid id, CancellationToken ct = default) => _db.Produto.FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<Dictionary<Guid, Produto>> BuscarDicionarioPorIds(IReadOnlyList<Guid> ids, CancellationToken ct = default) =>
        await _db.Produto
            .AsNoTracking()
            .Where(p => ids.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, ct);

    public Task<IReadOnlyList<Produto>> BuscarPorIds(IReadOnlyList<Guid> ids, CancellationToken ct = default) =>
        _db.Produto
           .Where(p => ids.Contains(p.Id))
           .ToListAsync(ct)
           .ContinueWith(t => (IReadOnlyList<Produto>)t.Result, ct);

    public async Task<(IReadOnlyList<Produto> Items, int Total)> BuscarPaginado(string? filtro, PagedRequest p, CancellationToken ct = default)
    {
        var query = _db.Produto.AsNoTracking().OrderBy(x => x.Nome);
        var total = await query.CountAsync(ct);
        var items = await query.Skip((p.Pagina - 1) * p.Tamanho).Take(p.Tamanho).ToListAsync(ct);
        return (items, total);
    }

    public async Task Salvar(Produto produto, CancellationToken ct = default)
    {
        _db.Produto.Add(produto);
        await _db.SaveChangesAsync(ct);
    }

    public async Task Atualizar(Produto produto, CancellationToken ct = default) =>
        await _db.SaveChangesAsync(ct);

    public async Task AtualizarLote(IReadOnlyList<Produto> produtos, CancellationToken ct = default) =>
        await _db.SaveChangesAsync(ct);

    public async Task Remover(Produto produto, CancellationToken ct = default)
    {
        _db.Produto.Remove(produto);
        await _db.SaveChangesAsync(ct);
    }
}
