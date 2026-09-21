using Api.Extensions.Markers;
using Application.Produtos.DTOs;
using Application.Produtos.UseCases.BuscarListaPaginada;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.DTOs;

namespace Api.Presenters.Produtos;

public class BuscarListaPaginadaProdutoPresenter : IBuscarListaPaginadaProdutoOutputPort, IPresenter
{
    public IActionResult? Result { get; private set; }
    public void Ok(PagedResult<ProdutoOutput> resultado) => Result = new OkObjectResult(resultado);
}
