using Application.Common.Interfaces;
using Domain.OrdensServico.Gateways;

namespace Application.OrdensServico.UseCases.RegistrarLinkPagamento;

// Consumido a partir do evento OrcamentoGerado (Billing Service) — ver
// Domain.OrdensServico.OrdemServico.RegistrarLinkPagamento.
public class RegistrarLinkPagamentoUseCase : IUseCase
{
    private readonly IOrdemServicoGateway _gateway;
    private readonly IRegistrarLinkPagamentoOutputPort _outputPort;

    public RegistrarLinkPagamentoUseCase(IOrdemServicoGateway gateway, IRegistrarLinkPagamentoOutputPort outputPort)
    {
        _gateway = gateway;
        _outputPort = outputPort;
    }

    public async Task Execute(RegistrarLinkPagamentoInput input, CancellationToken ct = default)
    {
        var ordemServico = await _gateway.BuscarPorId(input.IdOrdemServico, ct);
        if (ordemServico is null) { _outputPort.NaoEncontrado(); return; }

        ordemServico.RegistrarLinkPagamento(input.LinkPagamento);
        await _gateway.Atualizar(ordemServico, ct);

        _outputPort.Ok();
    }
}
