using Api.Extensions.Markers;
using Application.Produtos.DTOs;
using Application.Produtos.UseCases.InserirProduto;
using Microsoft.AspNetCore.Mvc;

namespace Api.Presenters.Produtos;

public class InserirProdutoPresenter : IInserirProdutoOutputPort, IPresenter
{
    public IActionResult? Result { get; private set; }
    public void Ok(ProdutoOutput output) => Result = new CreatedAtActionResult(
        "Buscar", "Produtos", new { id = output.Id }, output);
}
