using Application.Common.Interfaces;
using Domain.OrdensServico.Gateways;

namespace Application.OrdensServico.UseCases.Finalizar;

// Último passo automático da saga: hoje disparado via endpoint interno;
// futuramente reage ao evento "ExecucaoFinalizada" publicado pelo Execução
// Service. A entrega ao cliente (Entregar) continua sendo um passo manual,
// fora da saga.
public class FinalizarUseCase : IUseCase
{
    private readonly IOrdemServicoGateway _gateway;
    private readonly IFinalizarOutputPort _outputPort;

    public FinalizarUseCase(IOrdemServicoGateway gateway, IFinalizarOutputPort outputPort)
    {
        _gateway = gateway;
        _outputPort = outputPort;
    }

    public async Task Execute(FinalizarInput input, CancellationToken ct = default)
    {
        var ordemServico = await _gateway.BuscarPorId(input.IdOrdemServico, ct);
        if (ordemServico is null) { _outputPort.NaoEncontrado(); return; }

        ordemServico.Finalizar();
        await _gateway.Atualizar(ordemServico, ct);

        _outputPort.Ok();
    }
}
