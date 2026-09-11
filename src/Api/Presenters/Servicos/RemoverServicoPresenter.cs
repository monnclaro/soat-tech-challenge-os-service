using Api.Extensions.Markers;
using Application.Servicos.UseCases.RemoverServico;
using Microsoft.AspNetCore.Mvc;

namespace Api.Presenters.Servicos;

public class RemoverServicoPresenter : IRemoverServicoOutputPort, IPresenter
{
    public IActionResult? Result { get; private set; }
    public void Ok() => Result = new NoContentResult();
}
