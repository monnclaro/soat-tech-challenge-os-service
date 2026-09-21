using Api.Extensions.Markers;
using Application.OrdensServico.UseCases.Inserir;
using Microsoft.AspNetCore.Mvc;

namespace Api.Presenters.OrdensServico;

public class InserirOrdemServicoPresenter : IInserirOrdemServicoOutputPort, IPresenter
{
    public IActionResult? Result { get; private set; }
    public void ClienteNaoEncontrado() => Result = new NotFoundObjectResult(new { mensagem = "Cliente não encontrado." });
    public void VeiculoNaoPertenceAoCliente(string nomeCliente) => Result = new BadRequestObjectResult(new { mensagem = $"Veículo não encontrado para o cliente '{nomeCliente}'." });
    public void Ok(Guid id) => Result = new CreatedAtActionResult("Buscar", "OrdemServicos", new { id }, new { id });
}
