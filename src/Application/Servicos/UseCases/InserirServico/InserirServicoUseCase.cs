using Application.Common.Interfaces;
using Application.Servicos.DTOs;
using Domain.Servicos;
using Domain.Servicos.Gateways;

namespace Application.Servicos.UseCases.InserirServico;

public class InserirServicoUseCase : IUseCase
{
    private readonly IServicoGateway _gateway;
    private readonly IInserirServicoOutputPort _outputPort;

    public InserirServicoUseCase(IServicoGateway gateway, IInserirServicoOutputPort outputPort)
    {
        _gateway = gateway;
        _outputPort = outputPort;
    }

    public async Task Execute(InserirServicoInput input, CancellationToken ct = default)
    {
        var servico = new Servico();
        servico.Inserir(input.Nome, input.Descricao, input.Valor);

        await _gateway.Salvar(servico, ct);

        _outputPort.Ok(new ServicoOutput(servico.Id, servico.Nome, servico.Descricao, servico.Valor));
    }
}
