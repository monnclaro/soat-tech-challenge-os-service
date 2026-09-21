using SharedKernel.DTOs;

namespace Domain.Clientes.Gateways;

public interface IClienteGateway
{
    Task<Cliente?> BuscarPorId(Guid id, CancellationToken ct = default);
    Task<Cliente?> BuscarPorDocumento(string documento, CancellationToken ct = default);
    Task<Cliente?> BuscarComVeiculos(Guid id, CancellationToken ct = default);
    Task<bool> ExisteComDocumento(string documento, CancellationToken ct = default);
    Task<(IReadOnlyList<Cliente> Items, int Total)> BuscarPaginado(PagedRequest paginacao, CancellationToken ct = default);
    Task Salvar(Cliente cliente, CancellationToken ct = default);
    Task Atualizar(Cliente cliente, CancellationToken ct = default);
    Task Remover(Cliente cliente, CancellationToken ct = default);
}
