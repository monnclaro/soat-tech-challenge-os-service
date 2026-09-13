using Api.Extensions.Markers;
using Application.Clientes.UseCases;
using Application.Clientes.UseCases.InserirCliente;
using Microsoft.AspNetCore.Mvc;

namespace Api.Presenters.Clientes;

public class InserirClientePresenter : IInserirClienteOutputPort, IPresenter
{
    public IActionResult? Result { get; private set; }

    public void DocumentoDuplicado(string mensagem) =>
        Result = new ConflictObjectResult(new { mensagem });

    public void Ok(ClienteOutput output) =>
        Result = new CreatedAtActionResult("Buscar", "Clientes", new { id = output.Id }, output);
}
