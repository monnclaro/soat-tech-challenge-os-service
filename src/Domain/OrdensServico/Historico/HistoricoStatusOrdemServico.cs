using Domain.Common;
using Domain.OrdensServico.Enums;

namespace Domain.OrdensServico.Historico;

// Novo na Fase 4 — auditoria persistida de cada transição de status (antes só existia
// como log estruturado via OrdemServicoStatusAlteradoLogHandler). Alimenta o endpoint
// de consulta de histórico (GET /ordens-servico/{id}/historico).
public class HistoricoStatusOrdemServico : Entity
{
    public Guid Id { get; private set; }
    public Guid IdOrdemServico { get; private set; }
    public StatusOrdemServico Status { get; private set; }
    public DateTime OcorridoEm { get; private set; }

    public HistoricoStatusOrdemServico(Guid idOrdemServico, StatusOrdemServico status, DateTime ocorridoEm)
    {
        Id = Guid.NewGuid();
        IdOrdemServico = idOrdemServico;
        Status = status;
        OcorridoEm = ocorridoEm;
    }
}
