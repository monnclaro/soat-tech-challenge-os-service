using Api.Extensions.Markers;
using Application.OrdensServico.UseCases.Finalizar;
using Microsoft.AspNetCore.Mvc;

namespace Api.Presenters.OrdensServico;

public class FinalizarPresenter : IFinalizarOutputPort, IPresenter
{
    public IActionResult? Result { get; private set; }
    public void NaoEncontrado() => Result = new NotFoundResult();
    public void Ok() => Result = new OkResult();
}
