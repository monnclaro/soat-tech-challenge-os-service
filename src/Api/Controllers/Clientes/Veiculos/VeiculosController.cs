using System.Diagnostics.CodeAnalysis;
using Api.Controllers.Clientes.Veiculos.Requests;
using Api.Presenters.Clientes.Veiculos;
using Application.Clientes.Veiculos.Controllers;
using Application.Clientes.Veiculos.UseCases;
using Application.Clientes.Veiculos.UseCases.AtualizarVeiculo;
using Application.Clientes.Veiculos.UseCases.BuscarListaPaginada;
using Application.Clientes.Veiculos.UseCases.BuscarVeiculo;
using Application.Clientes.Veiculos.UseCases.InserirVeiculo;
using Application.Clientes.Veiculos.UseCases.RemoverVeiculo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.DTOs;

namespace Api.Controllers.Clientes.Veiculos;

[ApiController]
[Route("api/v1/clientes/{idCliente:guid}/veiculos")]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
// Controller "fino" (ver comentário em OrdemServicosController).
[ExcludeFromCodeCoverage]
public class VeiculosController : ControllerBase
{
    private readonly VeiculoController _controller;
    private readonly BuscarVeiculoPresenter _buscarPresenter;
    private readonly BuscarListaPaginadaVeiculoPresenter _listarPresenter;
    private readonly InserirVeiculoPresenter _inserirPresenter;
    private readonly AtualizarVeiculoPresenter _atualizarPresenter;
    private readonly RemoverVeiculoPresenter _removerPresenter;

    public VeiculosController(
        VeiculoController controller,
        BuscarVeiculoPresenter buscarPresenter,
        BuscarListaPaginadaVeiculoPresenter listarPresenter,
        InserirVeiculoPresenter inserirPresenter,
        AtualizarVeiculoPresenter atualizarPresenter,
        RemoverVeiculoPresenter removerPresenter)
    {
        _controller = controller;
        _buscarPresenter = buscarPresenter;
        _listarPresenter = listarPresenter;
        _inserirPresenter = inserirPresenter;
        _atualizarPresenter = atualizarPresenter;
        _removerPresenter = removerPresenter;
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(VeiculoOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Buscar([FromRoute] Guid id, CancellationToken ct)
    {
        await _controller.Buscar(new BuscarVeiculoInput(id), ct);
        return _buscarPresenter.Result!;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<VeiculoOutput>), StatusCodes.Status200OK)]
    public async Task<IActionResult> BuscarListaPaginada([FromRoute] Guid idCliente, [FromQuery] PagedRequest request, CancellationToken ct)
    {
        await _controller.BuscarListaPaginada(new BuscarListaPaginadaVeiculoInput(idCliente, request), ct);
        return _listarPresenter.Result!;
    }

    [HttpPost]
    [ProducesResponseType(typeof(VeiculoOutput), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Inserir([FromRoute] Guid idCliente, [FromBody] InserirVeiculoRequest request, CancellationToken ct)
    {
        await _controller.Inserir(new InserirVeiculoInput(idCliente, request.Placa, request.Marca, request.Modelo, request.Ano), ct);
        return _inserirPresenter.Result!;
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(VeiculoOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Atualizar([FromRoute] Guid id, [FromBody] AtualizarVeiculoRequest request, CancellationToken ct)
    {
        await _controller.Atualizar(new AtualizarVeiculoInput(id, request.Placa, request.Marca, request.Modelo, request.Ano), ct);
        return _atualizarPresenter.Result!;
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remover([FromRoute] Guid id, CancellationToken ct)
    {
        await _controller.Remover(new RemoverVeiculoInput(id), ct);
        return _removerPresenter.Result!;
    }
}
