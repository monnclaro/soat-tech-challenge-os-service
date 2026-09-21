using Domain.Clientes.Veiculos;
using Domain.Clientes.Veiculos.Gateways;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using SharedKernel.DTOs;

namespace Infrastructure.Gateways.Clientes;

public class VeiculoGateway : IVeiculoGateway
{
    private readonly OsServiceDbContext _db;

    public VeiculoGateway(OsServiceDbContext db) => _db = db;

    public Task<Veiculo?> BuscarPorId(Guid id, CancellationToken ct = default) =>
        _db.Veiculo.FirstOrDefaultAsync(v => v.Id == id, ct);

    public Task<Veiculo?> BuscarPorPlaca(string placa, CancellationToken ct = default) =>
        _db.Veiculo.FirstOrDefaultAsync(v => v.Placa == placa, ct);

    public Task<bool> ExisteComPlaca(string placa, CancellationToken ct = default) =>
        _db.Veiculo.AsNoTracking().AnyAsync(v => v.Placa == placa, ct);

    public Task<bool> ExisteComPlacaExcetoId(string placa, Guid idVeiculo, CancellationToken ct = default) =>
        _db.Veiculo.AsNoTracking().AnyAsync(v => v.Placa == placa && v.Id != idVeiculo, ct);

    public async Task<(IReadOnlyList<Veiculo> Items, int Total)> BuscarPaginadoPorCliente(Guid idCliente, PagedRequest p, CancellationToken ct = default)
    {
        var query = _db.Veiculo.AsNoTracking()
            .Where(v => v.IdCliente == idCliente)
            .OrderBy(v => v.DataCriacao);

        var total = await query.CountAsync(ct);
        var items = await query.Skip((p.Pagina - 1) * p.Tamanho).Take(p.Tamanho).ToListAsync(ct);
        return (items, total);
    }

    public async Task Inserir(Veiculo veiculo, CancellationToken ct = default)
    {
        _db.Veiculo.Add(veiculo);
        await _db.SaveChangesAsync(ct);
    }

    public async Task Atualizar(Veiculo veiculo, CancellationToken ct = default) =>
        await _db.SaveChangesAsync(ct);

    public async Task Remover(Veiculo veiculo, CancellationToken ct = default)
    {
        _db.Veiculo.Remove(veiculo);
        await _db.SaveChangesAsync(ct);
    }
}
