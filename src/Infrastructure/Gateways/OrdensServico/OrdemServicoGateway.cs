using Domain.OrdensServico;
using Domain.OrdensServico.Enums;
using Domain.OrdensServico.Gateways;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using SharedKernel.DTOs;

namespace Infrastructure.Gateways.OrdensServico;

public class OrdemServicoGateway : IOrdemServicoGateway
{
    private readonly OsServiceDbContext _db;

    public OrdemServicoGateway(OsServiceDbContext db) => _db = db;

    public Task<OrdemServico?> BuscarPorId(Guid id, CancellationToken ct = default) =>
        _db.OrdemServico.FirstOrDefaultAsync(o => o.Id == id, ct);

    public Task<OrdemServico?> BuscarComItens(Guid id, CancellationToken ct = default) =>
        _db.OrdemServico
           .AsSplitQuery()
           .Include(o => o.Servicos)
           .Include(o => o.Produtos)
           .FirstOrDefaultAsync(o => o.Id == id, ct);

    public async Task<(IReadOnlyList<OrdemServico> Items, int Total)> BuscarPaginado(PagedRequest p, CancellationToken ct = default)
    {
        var query = _db.OrdemServico.AsNoTracking()
            .Where(o => o.Status != StatusOrdemServico.Finalizada
                        && o.Status != StatusOrdemServico.Entregue
                        && o.Status != StatusOrdemServico.Cancelada)
            .OrderBy(o =>
                o.Status == StatusOrdemServico.EmExecucao ? 1 :
                o.Status == StatusOrdemServico.AguardandoAprovacao ? 2 :
                o.Status == StatusOrdemServico.EmDiagnostico ? 3 :
                o.Status == StatusOrdemServico.Recebida ? 4 : 99)
            .ThenBy(o => o.DataCriacao);

        var total = await query.CountAsync(ct);
        var items = await query.Skip((p.Pagina - 1) * p.Tamanho).Take(p.Tamanho).ToListAsync(ct);
        return (items, total);
    }

    public async Task<(IReadOnlyList<OrdemServico> Items, int Total)> BuscarPaginadoPorDocumentoCliente(string documento, PagedRequest p, CancellationToken ct = default)
    {
        var idsClientes = await _db.Cliente
            .AsNoTracking()
            .Where(c => c.Documento == documento)
            .Select(c => c.Id)
            .ToListAsync(ct);

        if (idsClientes.Count == 0)
        {
            return ([], 0);
        }

        var query = _db.OrdemServico.AsNoTracking()
            .Where(o => idsClientes.Contains(o.IdCliente))
            .OrderBy(o => o.DataCriacao);

        var total = await query.CountAsync(ct);
        var items = await query.Skip((p.Pagina - 1) * p.Tamanho).Take(p.Tamanho).ToListAsync(ct);
        return (items, total);
    }

    public async Task Salvar(OrdemServico ordemServico, CancellationToken ct = default)
    {
        _db.OrdemServico.Add(ordemServico);
        await _db.SaveChangesAsync(ct);
    }

    public async Task Atualizar(OrdemServico ordemServico, CancellationToken ct = default) =>
        await _db.SaveChangesAsync(ct);
}
