using Api.Extensions.Markers;
using Application.OrdensServico.UseCases.IniciarDiagnostico;
using Microsoft.AspNetCore.Mvc;

namespace Api.Presenters.OrdensServico;

public class IniciarDiagnosticoPresenter : IIniciarDiagnosticoOutputPort, IPresenter
{
    public IActionResult? Result { get; private set; }
    public void NaoEncontrado() => Result = new NotFoundResult();
    public void Ok() => Result = new OkResult();
}
