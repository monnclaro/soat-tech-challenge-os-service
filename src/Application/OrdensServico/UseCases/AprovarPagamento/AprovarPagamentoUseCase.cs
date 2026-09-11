using Application.Common.Interfaces;
using Domain.OrdensServico.Gateways;

namespace Application.OrdensServico.UseCases.AprovarPagamento;

// Passo 4 da saga: hoje disparado via endpoint interno; futuramente reage ao
// evento "PagamentoAprovado" publicado pelo Billing Service. Ao concluir, o
// próximo passo da saga é comandar o Execução Service a iniciar a execução
// (ainda não ligado) — ver PLANO-FASE-4-MICROSSERVICOS.md.
public class AprovarPagamentoUseCase : IUseCase
{
    private readonly IOrdemServicoGateway _gateway;
    private readonly IAprovarPagamentoOutputPort _outputPort;

    public AprovarPagamentoUseCase(IOrdemServicoGateway gateway, IAprovarPagamentoOutputPort outputPort)
    {
        _gateway = gateway;
        _outputPort = outputPort;
    }

    public async Task Execute(AprovarPagamentoInput input, CancellationToken ct = default)
    {
        var ordemServico = await _gateway.BuscarPorId(input.IdOrdemServico, ct);
        if (ordemServico is null) { _outputPort.NaoEncontrado(); return; }

        ordemServico.AprovarPagamento();
        await _gateway.Atualizar(ordemServico, ct);

        _outputPort.Ok();
    }
}
