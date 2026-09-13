using SharedKernel.DTOs;

namespace Domain.OrdensServico.Gateways;

public interface IOrdemServicoGateway
{
    Task<OrdemServico?> BuscarPorId(Guid id, CancellationToken ct = default);
    Task<OrdemServico?> BuscarComItens(Guid id, CancellationToken ct = default);
    Task<(IReadOnlyList<OrdemServico> Items, int Total)> BuscarPaginado(PagedRequest paginacao, CancellationToken ct = default);
    Task<(IReadOnlyList<OrdemServico> Items, int Total)> BuscarPaginadoPorDocumentoCliente(string documento, PagedRequest paginacao, CancellationToken ct = default);
    Task Salvar(OrdemServico ordemServico, CancellationToken ct = default);
    Task Atualizar(OrdemServico ordemServico, CancellationToken ct = default);
    Task Remover(OrdemServico ordemServico, CancellationToken ct = default);
}
