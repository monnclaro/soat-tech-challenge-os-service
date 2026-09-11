using Api.Extensions.Markers;
using Application.Servicos.DTOs;
using Application.Servicos.UseCases.InserirServico;
using Microsoft.AspNetCore.Mvc;

namespace Api.Presenters.Servicos;

public class InserirServicoPresenter : IInserirServicoOutputPort, IPresenter
{
    public IActionResult? Result { get; private set; }
    public void Ok(ServicoOutput output) => Result = new CreatedAtActionResult("Buscar", "Servicos", new { id = output.Id }, output);
}
