using Api.Extensions.Markers;
using Application.OrdensServico.UseCases.Remover;
using Microsoft.AspNetCore.Mvc;

namespace Api.Presenters.OrdensServico;

public class RemoverOrdemServicoPresenter : IRemoverOrdemServicoOutputPort, IPresenter
{
    public IActionResult? Result { get; private set; }
    public void NaoEncontrado() => Result = new NotFoundResult();
    public void Ok() => Result = new NoContentResult();
}
