using Api.Extensions.Markers;
using Application.Clientes.Veiculos.UseCases;
using Application.Clientes.Veiculos.UseCases.BuscarListaPaginada;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.DTOs;

namespace Api.Presenters.Clientes.Veiculos;

public class BuscarListaPaginadaVeiculoPresenter : IBuscarListaPaginadaVeiculoOutputPort, IPresenter
{
    public IActionResult? Result { get; private set; }
    public void Ok(PagedResult<VeiculoOutput> resultado) => Result = new OkObjectResult(resultado);
}
