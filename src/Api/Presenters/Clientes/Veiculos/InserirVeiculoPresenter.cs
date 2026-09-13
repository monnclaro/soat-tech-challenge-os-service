using Api.Extensions.Markers;
using Application.Clientes.Veiculos.UseCases;
using Application.Clientes.Veiculos.UseCases.InserirVeiculo;
using Microsoft.AspNetCore.Mvc;

namespace Api.Presenters.Clientes.Veiculos;

public class InserirVeiculoPresenter : IInserirVeiculoOutputPort, IPresenter
{
    public IActionResult? Result { get; private set; }
    public void ClienteNaoEncontrado() => Result = new NotFoundObjectResult(new { mensagem = "Cliente não encontrado." });
    public void PlacaDuplicada(string mensagem) => Result = new ConflictObjectResult(new { mensagem });
    public void Ok(VeiculoOutput output) => Result = new CreatedAtActionResult(
        "Buscar", "Veiculos", new { idCliente = output.IdCliente, id = output.Id }, output);
}
