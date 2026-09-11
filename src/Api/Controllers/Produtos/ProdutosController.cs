using Api.Controllers.Produtos.Requests;
using Api.Presenters.Produtos;
using Application.Produtos.Controllers;
using Application.Produtos.DTOs;
using Application.Produtos.UseCases.AtualizarProduto;
using Application.Produtos.UseCases.BuscarListaPaginada;
using Application.Produtos.UseCases.BuscarProduto;
using Application.Produtos.UseCases.IncrementarEstoque;
using Application.Produtos.UseCases.InserirProduto;
using Application.Produtos.UseCases.RemoverProduto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.DTOs;

namespace Api.Controllers.Produtos;

[ApiController]
[Route("api/v1/produtos")]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public class ProdutosController : ControllerBase
{
    private readonly ProdutoController _controller;
    private readonly BuscarProdutoPresenter _buscarPresenter;
    private readonly BuscarListaPaginadaProdutoPresenter _listarPresenter;
    private readonly InserirProdutoPresenter _inserirPresenter;
    private readonly AtualizarProdutoPresenter _atualizarPresenter;
    private readonly IncrementarEstoquePresenter _incrementarPresenter;
    private readonly RemoverProdutoPresenter _removerPresenter;

    public ProdutosController(
        ProdutoController controller,
        BuscarProdutoPresenter buscarPresenter,
        BuscarListaPaginadaProdutoPresenter listarPresenter,
        InserirProdutoPresenter inserirPresenter,
        AtualizarProdutoPresenter atualizarPresenter,
        IncrementarEstoquePresenter incrementarPresenter,
        RemoverProdutoPresenter removerPresenter)
    {
        _controller = controller;
        _buscarPresenter = buscarPresenter;
        _listarPresenter = listarPresenter;
        _inserirPresenter = inserirPresenter;
        _atualizarPresenter = atualizarPresenter;
        _incrementarPresenter = incrementarPresenter;
        _removerPresenter = removerPresenter;
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProdutoOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Buscar([FromRoute] Guid id, CancellationToken ct)
    {
        await _controller.Buscar(new BuscarProdutoInput(id), ct);
        return _buscarPresenter.Result!;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ProdutoOutput>), StatusCodes.Status200OK)]
    public async Task<IActionResult> BuscarListaPaginada([FromQuery] PagedRequest request, CancellationToken ct)
    {
        await _controller.BuscarListaPaginada(new BuscarListaPaginadaInput(request), ct);
        return _listarPresenter.Result!;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ProdutoOutput), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Inserir([FromBody] InserirProdutoRequest request, CancellationToken ct)
    {
        await _controller.Inserir(new InserirProdutoInput(request.Nome, request.Descricao, request.Valor, request.QuantidadeEmEstoque), ct);
        return _inserirPresenter.Result!;
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ProdutoOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar([FromRoute] Guid id, [FromBody] AtualizarProdutoRequest request, CancellationToken ct)
    {
        await _controller.Atualizar(new AtualizarProdutoInput(id, request.Nome, request.Descricao, request.Valor), ct);
        return _atualizarPresenter.Result!;
    }

    [HttpPatch("{id:guid}/incrementar-estoque")]
    [ProducesResponseType(typeof(ProdutoOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> IncrementarEstoque([FromRoute] Guid id, [FromBody] AtualizarQuantidadeEstoqueProdutoRequest request, CancellationToken ct)
    {
        await _controller.IncrementarEstoque(new IncrementarEstoqueInput(id, request.Quantidade), ct);
        return _incrementarPresenter.Result!;
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Remover([FromRoute] Guid id, CancellationToken ct)
    {
        await _controller.Remover(new RemoverProdutoInput(id), ct);
        return _removerPresenter.Result!;
    }
}
