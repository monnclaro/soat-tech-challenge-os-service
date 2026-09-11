using Api.Extensions.Markers;
using Application.OrdensServico.UseCases.Cancelar;
using Microsoft.AspNetCore.Mvc;

namespace Api.Presenters.OrdensServico;

public class CancelarPresenter : ICancelarOutputPort, IPresenter
{
    public IActionResult? Result { get; private set; }
    public void NaoEncontrado() => Result = new NotFoundResult();
    public void Ok() => Result = new OkResult();
}
