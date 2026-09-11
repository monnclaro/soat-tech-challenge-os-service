using Application.Produtos.UseCases.AtualizarProduto;
using Application.Produtos.UseCases.BuscarListaPaginada;
using Application.Produtos.UseCases.BuscarProduto;
using Application.Produtos.UseCases.IncrementarEstoque;
using Application.Produtos.UseCases.InserirProduto;
using Application.Produtos.UseCases.RemoverProduto;
using SharedKernel.Interfaces;

namespace Application.Produtos.Controllers;

public class ProdutoController : IScoped
{
    private readonly BuscarProdutoUseCase _buscar;
    private readonly BuscarListaPaginadaProdutoUseCase _listar;
    private readonly InserirProdutoUseCase _inserir;
    private readonly AtualizarProdutoUseCase _atualizar;
    private readonly IncrementarEstoqueUseCase _incrementar;
    private readonly RemoverProdutoUseCase _remover;

    public ProdutoController(
        BuscarProdutoUseCase buscar,
        BuscarListaPaginadaProdutoUseCase listar,
        InserirProdutoUseCase inserir,
        AtualizarProdutoUseCase atualizar,
        IncrementarEstoqueUseCase incrementar,
        RemoverProdutoUseCase remover)
    {
        _buscar = buscar;
        _listar = listar;
        _inserir = inserir;
        _atualizar = atualizar;
        _incrementar = incrementar;
        _remover = remover;
    }

    public async Task Buscar(BuscarProdutoInput input, CancellationToken ct = default)
        => await _buscar.Execute(input, ct);

    public async Task BuscarListaPaginada(BuscarListaPaginadaInput input, CancellationToken ct = default)
        => await _listar.Execute(input, ct);

    public async Task Inserir(InserirProdutoInput input, CancellationToken ct = default)
        => await _inserir.Execute(input, ct);

    public async Task Atualizar(AtualizarProdutoInput input, CancellationToken ct = default)
        => await _atualizar.Execute(input, ct);

    public async Task IncrementarEstoque(IncrementarEstoqueInput input, CancellationToken ct = default)
        => await _incrementar.Execute(input, ct);

    public async Task Remover(RemoverProdutoInput input, CancellationToken ct = default)
        => await _remover.Execute(input, ct);
}
