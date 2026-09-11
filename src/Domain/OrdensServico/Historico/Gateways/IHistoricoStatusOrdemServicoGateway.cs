namespace Domain.OrdensServico.Historico.Gateways;

public interface IHistoricoStatusOrdemServicoGateway
{
    Task<IReadOnlyList<HistoricoStatusOrdemServico>> BuscarPorOrdemServico(Guid idOrdemServico, CancellationToken ct = default);
    Task Salvar(HistoricoStatusOrdemServico historico, CancellationToken ct = default);
}
