using Api.Extensions.Markers;
using Application.Clientes.Veiculos.UseCases;
using Application.Clientes.Veiculos.UseCases.BuscarVeiculo;
using Microsoft.AspNetCore.Mvc;

namespace Api.Presenters.Clientes.Veiculos;

public class BuscarVeiculoPresenter : IBuscarVeiculoOutputPort, IPresenter
{
    public IActionResult? Result { get; private set; }
    public void NaoEncontrado() => Result = new NotFoundResult();
    public void Ok(VeiculoOutput output) => Result = new OkObjectResult(output);
}
