using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using Api.Controllers.OrdensServico.Requests;
using Api.Presenters.OrdensServico;
using Application.OrdensServico.Controllers;
using Application.OrdensServico.UseCases;
using Application.OrdensServico.UseCases.BuscarHistorico;
using Application.OrdensServico.UseCases.BuscarListaPaginada;
using Application.OrdensServico.UseCases.BuscarListaPaginadaPorDocumento;
using Application.OrdensServico.UseCases.BuscarOrdemServico;
using Application.OrdensServico.UseCases.Cancelar;
using Application.OrdensServico.UseCases.Entregar;
using Application.OrdensServico.UseCases.Inserir;
using Application.OrdensServico.UseCases.Remover;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.DTOs;

namespace Api.Controllers.OrdensServico;

[ApiController]
[Route("api/v1/ordens-servico")]
[Produces("application/json")]
// Controller "fino": só mapeia request -> UseCase.Execute() -> presenter.Result, sem
// nenhuma lógica de branching própria (a lógica real está nos UseCases/Presenters,
// cobertos por testes dedicados) — ver instruções de cobertura do projeto.
[ExcludeFromCodeCoverage]
public class OrdemServicosController : ControllerBase
{
    private readonly OrdemServicoController _controller;
    private readonly BuscarOrdemServicoPresenter _buscarPresenter;
    private readonly BuscarListaPaginadaOrdemServicoPresenter _listarPresenter;
    private readonly BuscarListaPaginadaPorDocumentoPresenter _listarPorDocumentoPresenter;
    private readonly BuscarHistoricoPresenter _historicoPresenter;
    private readonly InserirOrdemServicoPresenter _inserirPresenter;
    private readonly CancelarPresenter _cancelarPresenter;
    private readonly EntregarPresenter _entregarPresenter;
    private readonly RemoverOrdemServicoPresenter _removerPresenter;

    public OrdemServicosController(
        OrdemServicoController controller,
        BuscarOrdemServicoPresenter buscarPresenter,
        BuscarListaPaginadaOrdemServicoPresenter listarPresenter,
        BuscarListaPaginadaPorDocumentoPresenter listarPorDocumentoPresenter,
        BuscarHistoricoPresenter historicoPresenter,
        InserirOrdemServicoPresenter inserirPresenter,
        CancelarPresenter cancelarPresenter,
        EntregarPresenter entregarPresenter,
        RemoverOrdemServicoPresenter removerPresenter)
    {
        _controller = controller;
        _buscarPresenter = buscarPresenter;
        _listarPresenter = listarPresenter;
        _listarPorDocumentoPresenter = listarPorDocumentoPresenter;
        _historicoPresenter = historicoPresenter;
        _inserirPresenter = inserirPresenter;
        _cancelarPresenter = cancelarPresenter;
        _entregarPresenter = entregarPresenter;
        _removerPresenter = removerPresenter;
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(OrdemServicoOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Buscar([FromRoute] Guid id, CancellationToken ct)
    {
        await _controller.Buscar(new BuscarOrdemServicoInput(id), ct);
        return _buscarPresenter.Result!;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(PagedResult<OrdemServicoOutput>), StatusCodes.Status200OK)]
    public async Task<IActionResult> BuscarListaPaginada([FromQuery] PagedRequest request, CancellationToken ct)
    {
        await _controller.BuscarListaPaginada(new BuscarListaPaginadaOrdemServicoInput(request), ct);
        return _listarPresenter.Result!;
    }

    [HttpGet("cliente")]
    [Authorize(Roles = "Cliente,Admin")]
    [ProducesResponseType(typeof(PagedResult<OrdemServicoPorDocumentoOutput>), StatusCodes.Status200OK)]
    public async Task<IActionResult> BuscarListaPaginadaPorDocumento([FromQuery] string documento, [FromQuery] PagedRequest request, CancellationToken ct)
    {
        await _controller.BuscarListaPaginadaPorDocumento(new BuscarListaPaginadaPorDocumentoInput(documento, request, CallerDocumento()), ct);
        return _listarPorDocumentoPresenter.Result!;
    }

    [HttpGet("{id:guid}/historico")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(IReadOnlyList<HistoricoStatusOutput>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BuscarHistorico([FromRoute] Guid id, CancellationToken ct)
    {
        await _controller.BuscarHistorico(new BuscarHistoricoInput(id), ct);
        return _historicoPresenter.Result!;
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Inserir([FromBody] InserirOrdemServicoRequest request, CancellationToken ct)
    {
        await _controller.Inserir(new InserirOrdemServicoInput(request.IdCliente, request.IdVeiculo), ct);
        return _inserirPresenter.Result!;
    }

    // Os demais passos da saga (iniciar diagnóstico, registrar diagnóstico, aprovar
    // pagamento, finalizar) não são mais expostos aqui — cada um já é disparado
    // automaticamente pelos consumers RabbitMQ/MassTransit (ver
    // Infrastructure/Messaging/Consumers) reagindo aos eventos publicados pelo
    // Execução Service/Billing Service. Expor isso como endpoint HTTP permitia
    // avançar a OS sem o outro microsserviço saber, dessincronizando os dois.
    // Cancelamento e entrega continuam manuais: não existe gatilho automático pra
    // nenhum dos dois (cancelamento voluntário do atendente / entrega ao cliente).

    [HttpPatch("{id:guid}/cancelamento")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Cancelar([FromRoute] Guid id, CancellationToken ct)
    {
        await _controller.Cancelar(new CancelarInput(id), ct);
        return _cancelarPresenter.Result!;
    }

    [HttpPatch("{id:guid}/entrega")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Entregar([FromRoute] Guid id, CancellationToken ct)
    {
        await _controller.Entregar(new EntregarInput(id), ct);
        return _entregarPresenter.Result!;
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remover([FromRoute] Guid id, CancellationToken ct)
    {
        await _controller.Remover(new RemoverOrdemServicoInput(id), ct);
        return _removerPresenter.Result!;
    }

    // Documento (CPF) do Cliente dono do token, só quando a role é Cliente —
    // usado pra escopar a consulta de OS por documento ao próprio cliente.
    // Embutido pelo Lambda de login por CPF como claim "documento".
    private string? CallerDocumento() =>
        User.IsInRole("Cliente") ? User.FindFirstValue("documento") : null;
}
