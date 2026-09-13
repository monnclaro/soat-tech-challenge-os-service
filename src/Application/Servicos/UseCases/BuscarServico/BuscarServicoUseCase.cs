using Application.Common.Interfaces;
using Application.Servicos.DTOs;
using Domain.Servicos.Gateways;

namespace Application.Servicos.UseCases.BuscarServico;

public class BuscarServicoUseCase : IUseCase
{
    private readonly IServicoGateway _gateway;
    private readonly IBuscarServicoOutputPort _outputPort;

    public BuscarServicoUseCase(IServicoGateway gateway, IBuscarServicoOutputPort outputPort)
    {
        _gateway = gateway;
        _outputPort = outputPort;
    }

    public async Task Execute(BuscarServicoInput input, CancellationToken ct = default)
    {
        var servico = await _gateway.BuscarPorId(input.Id, ct);

        if (servico is null)
        {
            _outputPort.NaoEncontrado();
            return;
        }

        _outputPort.Ok(new ServicoOutput(servico.Id, servico.Nome, servico.Descricao, servico.Valor));
    }
}
