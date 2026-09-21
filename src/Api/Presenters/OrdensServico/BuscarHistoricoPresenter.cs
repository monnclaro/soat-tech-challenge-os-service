using Api.Extensions.Markers;
using Application.OrdensServico.UseCases;
using Application.OrdensServico.UseCases.BuscarHistorico;
using Microsoft.AspNetCore.Mvc;

namespace Api.Presenters.OrdensServico;

public class BuscarHistoricoPresenter : IBuscarHistoricoOutputPort, IPresenter
{
    public IActionResult? Result { get; private set; }
    public void NaoEncontrado() => Result = new NotFoundResult();
    public void Ok(IReadOnlyList<HistoricoStatusOutput> historico) => Result = new OkObjectResult(historico);
}
