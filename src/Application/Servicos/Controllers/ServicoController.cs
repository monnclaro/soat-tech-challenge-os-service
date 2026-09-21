using Application.Servicos.UseCases.AtualizarServico;
using Application.Servicos.UseCases.BuscarListaPaginada;
using Application.Servicos.UseCases.BuscarServico;
using Application.Servicos.UseCases.InserirServico;
using Application.Servicos.UseCases.RemoverServico;
using SharedKernel.Interfaces;

namespace Application.Servicos.Controllers;

public class ServicoController : IScoped
{
    private readonly BuscarServicoUseCase _buscar;
    private readonly BuscarListaPaginadaUseCase _listar;
    private readonly InserirServicoUseCase _inserir;
    private readonly AtualizarServicoUseCase _atualizar;
    private readonly RemoverServicoUseCase _remover;

    public ServicoController(
        BuscarServicoUseCase buscar,
        BuscarListaPaginadaUseCase listar,
        InserirServicoUseCase inserir,
        AtualizarServicoUseCase atualizar,
        RemoverServicoUseCase remover)
    {
        _buscar = buscar;
        _listar = listar;
        _inserir = inserir;
        _atualizar = atualizar;
        _remover = remover;
    }

    public async Task Buscar(BuscarServicoInput input, CancellationToken ct = default)
        => await _buscar.Execute(input, ct);

    public async Task BuscarListaPaginada(BuscarListaPaginadaInput input, CancellationToken ct = default)
        => await _listar.Execute(input, ct);

    public async Task Inserir(InserirServicoInput input, CancellationToken ct = default)
        => await _inserir.Execute(input, ct);

    public async Task Atualizar(AtualizarServicoInput input, CancellationToken ct = default)
        => await _atualizar.Execute(input, ct);

    public async Task Remover(RemoverServicoInput input, CancellationToken ct = default)
        => await _remover.Execute(input, ct);
}
