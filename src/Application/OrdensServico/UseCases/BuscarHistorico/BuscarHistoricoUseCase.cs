using Application.Common.Interfaces;
using Domain.OrdensServico.Gateways;
using Domain.OrdensServico.Historico.Gateways;

namespace Application.OrdensServico.UseCases.BuscarHistorico;

public class BuscarHistoricoUseCase : IUseCase
{
    private readonly IOrdemServicoGateway _ordemServicoGateway;
    private readonly IHistoricoStatusOrdemServicoGateway _historicoGateway;
    private readonly IBuscarHistoricoOutputPort _outputPort;

    public BuscarHistoricoUseCase(
        IOrdemServicoGateway ordemServicoGateway,
        IHistoricoStatusOrdemServicoGateway historicoGateway,
        IBuscarHistoricoOutputPort outputPort)
    {
        _ordemServicoGateway = ordemServicoGateway;
        _historicoGateway = historicoGateway;
        _outputPort = outputPort;
    }

    public async Task Execute(BuscarHistoricoInput input, CancellationToken ct = default)
    {
        var ordemServico = await _ordemServicoGateway.BuscarPorId(input.IdOrdemServico, ct);
        if (ordemServico is null) { _outputPort.NaoEncontrado(); return; }

        var historico = await _historicoGateway.BuscarPorOrdemServico(input.IdOrdemServico, ct);

        _outputPort.Ok(historico.Select(h => new HistoricoStatusOutput(h.Status.ToString(), h.OcorridoEm)).ToList());
    }
}
