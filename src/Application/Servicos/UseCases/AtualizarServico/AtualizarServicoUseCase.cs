using Application.Common.Interfaces;
using Application.Servicos.DTOs;
using Domain.Servicos.Gateways;

namespace Application.Servicos.UseCases.AtualizarServico;

public class AtualizarServicoUseCase : IUseCase
{
    private readonly IServicoGateway _gateway;
    private readonly IAtualizarServicoOutputPort _outputPort;

    public AtualizarServicoUseCase(IServicoGateway gateway, IAtualizarServicoOutputPort outputPort)
    {
        _gateway = gateway;
        _outputPort = outputPort;
    }

    public async Task Execute(AtualizarServicoInput input, CancellationToken ct = default)
    {
        var servico = await _gateway.BuscarPorId(input.Id, ct);

        if (servico is null)
        {
            _outputPort.NaoEncontrado();
            return;
        }

        servico.Atualizar(input.Nome, input.Descricao, input.Valor);
        await _gateway.Atualizar(servico, ct);

        _outputPort.Ok(new ServicoOutput(servico.Id, servico.Nome, servico.Descricao, servico.Valor));
    }
}
