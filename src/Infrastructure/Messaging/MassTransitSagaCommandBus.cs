using Application.Common.Interfaces;
using MassTransit;
using Soat.Contracts.Saga;

namespace Infrastructure.Messaging;

// Implementação do port de saída da saga via MassTransit/RabbitMQ. Usa Publish
// (não Send) de propósito, mesmo para os tipos "comando": nesta topologia
// simples, cada tipo de mensagem tem exatamente um consumidor esperado (o
// outro microsserviço dono daquele passo), então publish/subscribe já resolve
// o roteamento sem precisar endereçar filas explicitamente — ver
// PLANO-FASE-4-MICROSSERVICOS.md.
public class MassTransitSagaCommandBus : ISagaCommandBus
{
    private readonly IPublishEndpoint _publishEndpoint;

    public MassTransitSagaCommandBus(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public Task EnviarIniciarDiagnostico(Guid idOrdemServico, Guid idCliente, Guid idVeiculo, CancellationToken ct = default) =>
        _publishEndpoint.Publish(new IniciarDiagnostico(idOrdemServico, idCliente, idVeiculo), ct);

    public Task EnviarGerarOrcamento(
        Guid idOrdemServico,
        IReadOnlyList<ItemServicoDiagnosticado> servicos,
        IReadOnlyList<ItemProdutoDiagnosticado> produtos,
        decimal valorTotal,
        CancellationToken ct = default) =>
        _publishEndpoint.Publish(new GerarOrcamento(idOrdemServico, servicos, produtos, valorTotal), ct);

    public Task EnviarIniciarExecucao(Guid idOrdemServico, CancellationToken ct = default) =>
        _publishEndpoint.Publish(new IniciarExecucao(idOrdemServico), ct);
}
