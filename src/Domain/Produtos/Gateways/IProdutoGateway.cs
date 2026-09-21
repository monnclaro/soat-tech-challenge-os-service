using SharedKernel.DTOs;

namespace Domain.Produtos.Gateways;

public interface IProdutoGateway
{
    Task<Produto?> BuscarPorId(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Produto>> BuscarPorIds(IReadOnlyList<Guid> ids, CancellationToken ct = default);
    Task<Dictionary<Guid, Produto>> BuscarDicionarioPorIds(IReadOnlyList<Guid> ids, CancellationToken ct = default);
    Task<(IReadOnlyList<Produto> Items, int Total)> BuscarPaginado(string? filtro, PagedRequest paginacao, CancellationToken ct = default);
    Task Salvar(Produto produto, CancellationToken ct = default);
    Task Atualizar(Produto produto, CancellationToken ct = default);
    Task AtualizarLote(IReadOnlyList<Produto> produtos, CancellationToken ct = default);
    Task Remover(Produto produto, CancellationToken ct = default);
}
