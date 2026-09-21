using Api.Extensions.Markers;
using Application.Clientes.UseCases;
using Application.Clientes.UseCases.BuscarListaPaginada;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.DTOs;

namespace Api.Presenters.Clientes;

public class BuscarListaPaginadaClientePresenter : IBuscarListaPaginadaClienteOutputPort, IPresenter
{
    public IActionResult? Result { get; private set; }
    public void Ok(PagedResult<ClienteOutput> resultado) => Result = new OkObjectResult(resultado);
}
