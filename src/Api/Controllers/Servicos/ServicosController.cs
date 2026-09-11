using Api.Controllers.Servicos.Requests;
using Api.Presenters.Servicos;
using Application.Servicos.Controllers;
using Application.Servicos.DTOs;
using Application.Servicos.UseCases.AtualizarServico;
using Application.Servicos.UseCases.BuscarListaPaginada;
using Application.Servicos.UseCases.BuscarServico;
using Application.Servicos.UseCases.InserirServico;
using Application.Servicos.UseCases.RemoverServico;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.DTOs;

namespace Api.Controllers.Servicos;

[ApiController]
[Route("api/v1/servicos")]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public class ServicosController : ControllerBase
{
    private readonly ServicoController _controller;
    private readonly BuscarServicoPresenter _buscarPresenter;
    private readonly BuscarListaPaginadaPresenter _listarPresenter;
    private readonly InserirServicoPresenter _inserirPresenter;
    private readonly AtualizarServicoPresenter _atualizarPresenter;
    private readonly RemoverServicoPresenter _removerPresenter;

    public ServicosController(
        ServicoController controller,
        BuscarServicoPresenter buscarPresenter,
        BuscarListaPaginadaPresenter listarPresenter,
        InserirServicoPresenter inserirPresenter,
        AtualizarServicoPresenter atualizarPresenter,
        RemoverServicoPresenter removerPresenter)
    {
        _controller = controller;
        _buscarPresenter = buscarPresenter;
        _listarPresenter = listarPresenter;
        _inserirPresenter = inserirPresenter;
        _atualizarPresenter = atualizarPresenter;
        _removerPresenter = removerPresenter;
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ServicoOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Buscar([FromRoute] Guid id, CancellationToken ct)
    {
        await _controller.Buscar(new BuscarServicoInput(id), ct);
        return _buscarPresenter.Result!;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ServicoOutput>), StatusCodes.Status200OK)]
    public async Task<IActionResult> BuscarListaPaginada([FromQuery] PagedRequest request, CancellationToken ct)
    {
        await _controller.BuscarListaPaginada(new BuscarListaPaginadaInput(request), ct);
        return _listarPresenter.Result!;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ServicoOutput), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Inserir([FromBody] InserirServicoRequest request, CancellationToken ct)
    {
        await _controller.Inserir(new InserirServicoInput(request.Nome, request.Descricao, request.Valor), ct);
        return _inserirPresenter.Result!;
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ServicoOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar([FromRoute] Guid id, [FromBody] AtualizarServicoRequest request, CancellationToken ct)
    {
        await _controller.Atualizar(new AtualizarServicoInput(id, request.Nome, request.Descricao, request.Valor), ct);
        return _atualizarPresenter.Result!;
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Remover([FromRoute] Guid id, CancellationToken ct)
    {
        await _controller.Remover(new RemoverServicoInput(id), ct);
        return _removerPresenter.Result!;
    }
}
