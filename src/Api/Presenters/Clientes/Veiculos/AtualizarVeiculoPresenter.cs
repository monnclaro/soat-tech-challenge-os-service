using Api.Extensions.Markers;
using Application.Clientes.Veiculos.UseCases;
using Application.Clientes.Veiculos.UseCases.AtualizarVeiculo;
using Microsoft.AspNetCore.Mvc;

namespace Api.Presenters.Clientes.Veiculos;

public class AtualizarVeiculoPresenter : IAtualizarVeiculoOutputPort, IPresenter
{
    public IActionResult? Result { get; private set; }
    public void NaoEncontrado() => Result = new NotFoundResult();
    public void PlacaDuplicada(string mensagem) => Result = new ConflictObjectResult(new { mensagem });
    public void Ok(VeiculoOutput output) => Result = new OkObjectResult(output);
}
