using Api.Extensions.Markers;
using Application.OrdensServico.UseCases.Entregar;
using Microsoft.AspNetCore.Mvc;

namespace Api.Presenters.OrdensServico;

public class EntregarPresenter : IEntregarOutputPort, IPresenter
{
    public IActionResult? Result { get; private set; }
    public void NaoEncontrado() => Result = new NotFoundResult();
    public void Ok() => Result = new OkResult();
}
