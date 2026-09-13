using Api.Extensions.Markers;
using Application.Produtos.UseCases.RemoverProduto;
using Microsoft.AspNetCore.Mvc;

namespace Api.Presenters.Produtos;

public class RemoverProdutoPresenter : IRemoverProdutoOutputPort, IPresenter
{
    public IActionResult? Result { get; private set; }
    public void Ok() => Result = new NoContentResult();
}
