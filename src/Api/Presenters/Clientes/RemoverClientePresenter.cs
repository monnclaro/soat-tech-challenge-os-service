using Api.Extensions.Markers;
using Application.Clientes.UseCases.RemoverCliente;
using Microsoft.AspNetCore.Mvc;

namespace Api.Presenters.Clientes;

public class RemoverClientePresenter : IRemoverClienteOutputPort, IPresenter
{
    public IActionResult? Result { get; private set; }
    public void NaoEncontrado() => Result = new NotFoundResult();
    public void Ok() => Result = new NoContentResult();
}
