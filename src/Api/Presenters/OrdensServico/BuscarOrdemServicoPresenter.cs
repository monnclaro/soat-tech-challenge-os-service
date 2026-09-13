using Api.Extensions.Markers;
using Application.OrdensServico.UseCases;
using Application.OrdensServico.UseCases.BuscarOrdemServico;
using Microsoft.AspNetCore.Mvc;

namespace Api.Presenters.OrdensServico;

public class BuscarOrdemServicoPresenter : IBuscarOrdemServicoOutputPort, IPresenter
{
    public IActionResult? Result { get; private set; }
    public void NaoEncontrado() => Result = new NotFoundResult();
    public void Ok(OrdemServicoOutput output) => Result = new OkObjectResult(output);
}
