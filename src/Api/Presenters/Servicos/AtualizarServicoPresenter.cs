using Api.Extensions.Markers;
using Application.Servicos.DTOs;
using Application.Servicos.UseCases.AtualizarServico;
using Microsoft.AspNetCore.Mvc;

namespace Api.Presenters.Servicos;

public class AtualizarServicoPresenter : IAtualizarServicoOutputPort, IPresenter
{
    public IActionResult? Result { get; private set; }
    public void NaoEncontrado() => Result = new NotFoundResult();
    public void Ok(ServicoOutput output) => Result = new OkObjectResult(output);
}
