using Application.Common.Interfaces;
using Domain.OrdensServico.Gateways;

namespace Application.OrdensServico.UseCases.IniciarExecucao;

// Consumido a partir do evento ExecucaoIniciada (Execução Service) — a OS só
// avança de AguardandoExecucao para EmExecucao quando o Execução Service
// confirma que o primeiro serviço entrou em execução de verdade (ver
// ExecucaoIniciadaConsumer).
public class IniciarExecucaoUseCase : IUseCase
{
    private readonly IOrdemServicoGateway _gateway;
    private readonly IIniciarExecucaoOutputPort _outputPort;

    public IniciarExecucaoUseCase(IOrdemServicoGateway gateway, IIniciarExecucaoOutputPort outputPort)
    {
        _gateway = gateway;
        _outputPort = outputPort;
    }

    public async Task Execute(IniciarExecucaoInput input, CancellationToken ct = default)
    {
        var ordemServico = await _gateway.BuscarPorId(input.IdOrdemServico, ct);
        if (ordemServico is null) { _outputPort.NaoEncontrado(); return; }

        ordemServico.IniciarExecucao();
        await _gateway.Atualizar(ordemServico, ct);

        _outputPort.Ok();
    }
}
