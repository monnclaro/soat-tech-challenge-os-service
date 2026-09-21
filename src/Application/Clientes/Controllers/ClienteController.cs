using Application.Clientes.UseCases.AtualizarCliente;
using Application.Clientes.UseCases.BuscarCliente;
using Application.Clientes.UseCases.BuscarListaPaginada;
using Application.Clientes.UseCases.InserirCliente;
using Application.Clientes.UseCases.RemoverCliente;
using SharedKernel.Interfaces;

namespace Application.Clientes.Controllers;

public class ClienteController : IScoped
{
    private readonly BuscarClienteUseCase _buscar;
    private readonly BuscarListaPaginadaClienteUseCase _listar;
    private readonly InserirClienteUseCase _inserir;
    private readonly AtualizarClienteUseCase _atualizar;
    private readonly RemoverClienteUseCase _remover;

    public ClienteController(
        BuscarClienteUseCase buscar,
        BuscarListaPaginadaClienteUseCase listar,
        InserirClienteUseCase inserir,
        AtualizarClienteUseCase atualizar,
        RemoverClienteUseCase remover)
    {
        _buscar = buscar;
        _listar = listar;
        _inserir = inserir;
        _atualizar = atualizar;
        _remover = remover;
    }

    public async Task Buscar(BuscarClienteInput input, CancellationToken ct = default)
        => await _buscar.Execute(input, ct);

    public async Task BuscarListaPaginada(BuscarListaPaginadaClienteInput input, CancellationToken ct = default)
        => await _listar.Execute(input, ct);

    public async Task Inserir(InserirClienteInput input, CancellationToken ct = default)
        => await _inserir.Execute(input, ct);

    public async Task Atualizar(AtualizarClienteInput input, CancellationToken ct = default)
        => await _atualizar.Execute(input, ct);

    public async Task Remover(RemoverClienteInput input, CancellationToken ct = default)
        => await _remover.Execute(input, ct);
}
