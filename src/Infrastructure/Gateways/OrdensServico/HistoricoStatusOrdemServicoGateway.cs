using Domain.OrdensServico.Historico;
using Domain.OrdensServico.Historico.Gateways;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Gateways.OrdensServico;

public class HistoricoStatusOrdemServicoGateway : IHistoricoStatusOrdemServicoGateway
{
    private readonly OsServiceDbContext _db;

    public HistoricoStatusOrdemServicoGateway(OsServiceDbContext db) => _db = db;

    public async Task<IReadOnlyList<HistoricoStatusOrdemServico>> BuscarPorOrdemServico(Guid idOrdemServico, CancellationToken ct = default) =>
        await _db.HistoricoStatusOrdemServico
            .AsNoTracking()
            .Where(h => h.IdOrdemServico == idOrdemServico)
            .OrderBy(h => h.OcorridoEm)
            .ToListAsync(ct);

    public async Task Salvar(HistoricoStatusOrdemServico historico, CancellationToken ct = default)
    {
        _db.HistoricoStatusOrdemServico.Add(historico);
        await _db.SaveChangesAsync(ct);
    }
}
