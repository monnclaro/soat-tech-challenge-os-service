using Api.Extensions.Markers;
using Application.OrdensServico.UseCases;
using Application.OrdensServico.UseCases.BuscarListaPaginada;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.DTOs;

namespace Api.Presenters.OrdensServico;

public class BuscarListaPaginadaOrdemServicoPresenter : IBuscarListaPaginadaOrdemServicoOutputPort, IPresenter
{
    public IActionResult? Result { get; private set; }
    public void Ok(PagedResult<OrdemServicoOutput> resultado) => Result = new OkObjectResult(resultado);
}
