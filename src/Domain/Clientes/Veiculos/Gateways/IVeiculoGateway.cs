using SharedKernel.DTOs;

namespace Domain.Clientes.Veiculos.Gateways;

public interface IVeiculoGateway
{
    Task<Veiculo?> BuscarPorId(Guid id, CancellationToken ct = default);
    Task<Veiculo?> BuscarPorPlaca(string placa, CancellationToken ct = default);
    Task<bool> ExisteComPlaca(string placa, CancellationToken ct = default);
    Task<bool> ExisteComPlacaExcetoId(string placa, Guid idVeiculo, CancellationToken ct = default);
    Task<(IReadOnlyList<Veiculo> Items, int Total)> BuscarPaginadoPorCliente(Guid idCliente, PagedRequest paginacao, CancellationToken ct = default);
    Task Inserir(Veiculo veiculo, CancellationToken ct = default);
    Task Atualizar(Veiculo veiculo, CancellationToken ct = default);
    Task Remover(Veiculo veiculo, CancellationToken ct = default);
}
