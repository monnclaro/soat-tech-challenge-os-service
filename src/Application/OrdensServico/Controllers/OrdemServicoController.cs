using Application.OrdensServico.UseCases.AprovarPagamento;
using Application.OrdensServico.UseCases.BuscarHistorico;
using Application.OrdensServico.UseCases.BuscarListaPaginada;
using Application.OrdensServico.UseCases.BuscarListaPaginadaPorDocumento;
using Application.OrdensServico.UseCases.BuscarOrdemServico;
using Application.OrdensServico.UseCases.Cancelar;
using Application.OrdensServico.UseCases.Entregar;
using Application.OrdensServico.UseCases.Finalizar;
using Application.OrdensServico.UseCases.IniciarDiagnostico;
using Application.OrdensServico.UseCases.Inserir;
using Application.OrdensServico.UseCases.RegistrarDiagnostico;
using Application.OrdensServico.UseCases.Remover;
using SharedKernel.Interfaces;

namespace Application.OrdensServico.Controllers;

public class OrdemServicoController : IScoped
{
    private readonly BuscarOrdemServicoUseCase _buscar;
    private readonly BuscarListaPaginadaOrdemServicoUseCase _listar;
    private readonly BuscarListaPaginadaPorDocumentoUseCase _listarPorDocumento;
    private readonly BuscarHistoricoUseCase _buscarHistorico;
    private readonly InserirOrdemServicoUseCase _inserir;
    private readonly IniciarDiagnosticoUseCase _iniciarDiagnostico;
    private readonly RegistrarDiagnosticoUseCase _registrarDiagnostico;
    private readonly AprovarPagamentoUseCase _aprovarPagamento;
    private readonly CancelarUseCase _cancelar;
    private readonly FinalizarUseCase _finalizar;
    private readonly EntregarUseCase _entregar;
    private readonly RemoverOrdemServicoUseCase _remover;

    public OrdemServicoController(
        BuscarOrdemServicoUseCase buscar,
        BuscarListaPaginadaOrdemServicoUseCase listar,
        BuscarListaPaginadaPorDocumentoUseCase listarPorDocumento,
        BuscarHistoricoUseCase buscarHistorico,
        InserirOrdemServicoUseCase inserir,
        IniciarDiagnosticoUseCase iniciarDiagnostico,
        RegistrarDiagnosticoUseCase registrarDiagnostico,
        AprovarPagamentoUseCase aprovarPagamento,
        CancelarUseCase cancelar,
        FinalizarUseCase finalizar,
        EntregarUseCase entregar,
        RemoverOrdemServicoUseCase remover)
    {
        _buscar = buscar;
        _listar = listar;
        _listarPorDocumento = listarPorDocumento;
        _buscarHistorico = buscarHistorico;
        _inserir = inserir;
        _iniciarDiagnostico = iniciarDiagnostico;
        _registrarDiagnostico = registrarDiagnostico;
        _aprovarPagamento = aprovarPagamento;
        _cancelar = cancelar;
        _finalizar = finalizar;
        _entregar = entregar;
        _remover = remover;
    }

    public async Task Buscar(BuscarOrdemServicoInput input, CancellationToken ct = default)
        => await _buscar.Execute(input, ct);

    public async Task BuscarListaPaginada(BuscarListaPaginadaOrdemServicoInput input, CancellationToken ct = default)
        => await _listar.Execute(input, ct);

    public async Task BuscarListaPaginadaPorDocumento(BuscarListaPaginadaPorDocumentoInput input, CancellationToken ct = default)
        => await _listarPorDocumento.Execute(input, ct);

    public async Task BuscarHistorico(BuscarHistoricoInput input, CancellationToken ct = default)
        => await _buscarHistorico.Execute(input, ct);

    public async Task Inserir(InserirOrdemServicoInput input, CancellationToken ct = default)
        => await _inserir.Execute(input, ct);

    public async Task IniciarDiagnostico(IniciarDiagnosticoInput input, CancellationToken ct = default)
        => await _iniciarDiagnostico.Execute(input, ct);

    public async Task RegistrarDiagnostico(RegistrarDiagnosticoInput input, CancellationToken ct = default)
        => await _registrarDiagnostico.Execute(input, ct);

    public async Task AprovarPagamento(AprovarPagamentoInput input, CancellationToken ct = default)
        => await _aprovarPagamento.Execute(input, ct);

    public async Task Cancelar(CancelarInput input, CancellationToken ct = default)
        => await _cancelar.Execute(input, ct);

    public async Task Finalizar(FinalizarInput input, CancellationToken ct = default)
        => await _finalizar.Execute(input, ct);

    public async Task Entregar(EntregarInput input, CancellationToken ct = default)
        => await _entregar.Execute(input, ct);

    public async Task Remover(RemoverOrdemServicoInput input, CancellationToken ct = default)
        => await _remover.Execute(input, ct);
}
