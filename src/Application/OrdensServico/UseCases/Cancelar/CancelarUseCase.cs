using Application.Common.Interfaces;
using Domain.OrdensServico.Gateways;

namespace Application.OrdensServico.UseCases.Cancelar;

// Compensação da saga: hoje disparado via endpoint interno; futuramente reage
// a "PagamentoRecusado"/expiração (Billing Service) ou a um diagnóstico que
// concluiu o veículo como não atendível (Execução Service) — ver
// PLANO-FASE-4-MICROSSERVICOS.md.
public class CancelarUseCase : IUseCase
{
    private readonly IOrdemServicoGateway _gateway;
    private readonly ICancelarOutputPort _outputPort;

    public CancelarUseCase(IOrdemServicoGateway gateway, ICancelarOutputPort outputPort)
    {
        _gateway = gateway;
        _outputPort = outputPort;
    }

    public async Task Execute(CancelarInput input, CancellationToken ct = default)
    {
        var ordemServico = await _gateway.BuscarPorId(input.IdOrdemServico, ct);
        if (ordemServico is null) { _outputPort.NaoEncontrado(); return; }

        ordemServico.Cancelar();
        await _gateway.Atualizar(ordemServico, ct);

        _outputPort.Ok();
    }
}
