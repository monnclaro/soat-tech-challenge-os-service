using Api.Extensions.Markers;
using Application.Servicos.DTOs;
using Application.Servicos.UseCases.BuscarListaPaginada;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.DTOs;

namespace Api.Presenters.Servicos;

public class BuscarListaPaginadaPresenter : IBuscarListaPaginadaOutputPort, IPresenter
{
    public IActionResult? Result { get; private set; }
    public void Ok(PagedResult<ServicoOutput> r) => Result = new OkObjectResult(r);
}
