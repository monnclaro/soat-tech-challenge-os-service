using System.Diagnostics.CodeAnalysis;
using Api.Controllers.Clientes.Requests;
using Api.Presenters.Clientes;
using Application.Clientes.Controllers;
using Application.Clientes.UseCases;
using Application.Clientes.UseCases.AtualizarCliente;
using Application.Clientes.UseCases.BuscarCliente;
using Application.Clientes.UseCases.BuscarListaPaginada;
using Application.Clientes.UseCases.InserirCliente;
using Application.Clientes.UseCases.RemoverCliente;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.DTOs;

namespace Api.Controllers.Clientes;

[ApiController]
[Route("api/v1/clientes")]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
// Controller "fino" (ver comentário em OrdemServicosController).
[ExcludeFromCodeCoverage]
public class ClientesController : ControllerBase
{
    private readonly ClienteController _controller;
    private readonly BuscarClientePresenter _buscarPresenter;
    private readonly BuscarListaPaginadaClientePresenter _listarPresenter;
    private readonly InserirClientePresenter _inserirPresenter;
    private readonly AtualizarClientePresenter _atualizarPresenter;
    private readonly RemoverClientePresenter _removerPresenter;

    public ClientesController(
        ClienteController controller,
        BuscarClientePresenter buscarPresenter,
        BuscarListaPaginadaClientePresenter listarPresenter,
        InserirClientePresenter inserirPresenter,
        AtualizarClientePresenter atualizarPresenter,
        RemoverClientePresenter removerPresenter)
    {
        _controller = controller;
        _buscarPresenter = buscarPresenter;
        _listarPresenter = listarPresenter;
        _inserirPresenter = inserirPresenter;
        _atualizarPresenter = atualizarPresenter;
        _removerPresenter = removerPresenter;
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ClienteOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Buscar([FromRoute] Guid id, CancellationToken ct)
    {
        await _controller.Buscar(new BuscarClienteInput(id), ct);
        return _buscarPresenter.Result!;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ClienteOutput>), StatusCodes.Status200OK)]
    public async Task<IActionResult> BuscarListaPaginada([FromQuery] PagedRequest request, CancellationToken ct)
    {
        await _controller.BuscarListaPaginada(new BuscarListaPaginadaClienteInput(request), ct);
        return _listarPresenter.Result!;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ClienteOutput), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Inserir([FromBody] InserirClienteRequest request, CancellationToken ct)
    {
        await _controller.Inserir(new InserirClienteInput(request.Nome, request.Documento), ct);
        return _inserirPresenter.Result!;
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ClienteOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar([FromRoute] Guid id, [FromBody] AtualizarClienteRequest request, CancellationToken ct)
    {
        await _controller.Atualizar(new AtualizarClienteInput(id, request.Nome, request.Ativo), ct);
        return _atualizarPresenter.Result!;
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remover([FromRoute] Guid id, CancellationToken ct)
    {
        await _controller.Remover(new RemoverClienteInput(id), ct);
        return _removerPresenter.Result!;
    }
}
