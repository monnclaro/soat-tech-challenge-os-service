using Application.Common.Interfaces;
using Domain.Servicos.Gateways;

namespace Application.Servicos.UseCases.RemoverServico;

public class RemoverServicoUseCase : IUseCase
{
    private readonly IServicoGateway _gateway;
    private readonly IRemoverServicoOutputPort _outputPort;

    public RemoverServicoUseCase(IServicoGateway gateway, IRemoverServicoOutputPort outputPort)
    {
        _gateway = gateway;
        _outputPort = outputPort;
    }

    public async Task Execute(RemoverServicoInput input, CancellationToken ct = default)
    {
        var servico = await _gateway.BuscarPorId(input.Id, ct);

        if (servico is null)
        {
            _outputPort.Ok();
            return;
        }

        await _gateway.Remover(servico, ct);
        _outputPort.Ok();
    }
}
