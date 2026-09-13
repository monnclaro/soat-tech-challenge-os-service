using Application.Clientes.Veiculos.UseCases.AtualizarVeiculo;
using Application.Clientes.Veiculos.UseCases.BuscarListaPaginada;
using Application.Clientes.Veiculos.UseCases.BuscarVeiculo;
using Application.Clientes.Veiculos.UseCases.InserirVeiculo;
using Application.Clientes.Veiculos.UseCases.RemoverVeiculo;
using SharedKernel.Interfaces;

namespace Application.Clientes.Veiculos.Controllers;

public class VeiculoController : IScoped
{
    private readonly BuscarVeiculoUseCase _buscar;
    private readonly BuscarListaPaginadaVeiculoUseCase _listar;
    private readonly InserirVeiculoUseCase _inserir;
    private readonly AtualizarVeiculoUseCase _atualizar;
    private readonly RemoverVeiculoUseCase _remover;

    public VeiculoController(
        BuscarVeiculoUseCase buscar,
        BuscarListaPaginadaVeiculoUseCase listar,
        InserirVeiculoUseCase inserir,
        AtualizarVeiculoUseCase atualizar,
        RemoverVeiculoUseCase remover)
    {
        _buscar = buscar;
        _listar = listar;
        _inserir = inserir;
        _atualizar = atualizar;
        _remover = remover;
    }

    public async Task Buscar(BuscarVeiculoInput input, CancellationToken ct = default)
        => await _buscar.Execute(input, ct);

    public async Task BuscarListaPaginada(BuscarListaPaginadaVeiculoInput input, CancellationToken ct = default)
        => await _listar.Execute(input, ct);

    public async Task Inserir(InserirVeiculoInput input, CancellationToken ct = default)
        => await _inserir.Execute(input, ct);

    public async Task Atualizar(AtualizarVeiculoInput input, CancellationToken ct = default)
        => await _atualizar.Execute(input, ct);

    public async Task Remover(RemoverVeiculoInput input, CancellationToken ct = default)
        => await _remover.Execute(input, ct);
}
