using SharedKernel.DTOs;

namespace Domain.Servicos.Gateways;

public interface IServicoGateway
{
    Task<Servico?> BuscarPorId(Guid id, CancellationToken ct = default);
    Task<Dictionary<Guid, Servico>> BuscarPorIds(IReadOnlyList<Guid> ids, CancellationToken ct = default);
    Task<(IReadOnlyList<Servico> Items, int Total)> BuscarPaginado(PagedRequest paginacao, CancellationToken ct = default);
    Task Salvar(Servico servico, CancellationToken ct = default);
    Task Atualizar(Servico servico, CancellationToken ct = default);
    Task Remover(Servico servico, CancellationToken ct = default);
}

