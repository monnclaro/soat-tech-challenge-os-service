using Api.Extensions.Markers;
using Application.Produtos.DTOs;
using Application.Produtos.UseCases.AtualizarProduto;
using Microsoft.AspNetCore.Mvc;

namespace Api.Presenters.Produtos;

public class AtualizarProdutoPresenter : IAtualizarProdutoOutputPort, IPresenter
{
    public IActionResult? Result { get; private set; }
    public void NaoEncontrado() => Result = new NotFoundResult();
    public void Ok(ProdutoOutput output) => Result = new OkObjectResult(output);
}
