using Api.Extensions.Markers;
using Application.Clientes.UseCases;
using Application.Clientes.UseCases.AtualizarCliente;
using Microsoft.AspNetCore.Mvc;

namespace Api.Presenters.Clientes;

public class AtualizarClientePresenter : IAtualizarClienteOutputPort, IPresenter
{
    public IActionResult? Result { get; private set; }
    public void NaoEncontrado() => Result = new NotFoundResult();
    public void Ok(ClienteOutput output) => Result = new OkObjectResult(output);
}
