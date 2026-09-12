using Application.Common.Interfaces;
using Domain.OrdensServico.Events;
using Soat.Contracts.Saga;

namespace Application.OrdensServico.EventHandlers;

// Passo 3 -> 4 da saga: ao concluir o registro do diagnóstico, comanda o
// Billing Service a gerar o orçamento.
internal sealed class EnviarGerarOrcamentoHandler : IDomainEventHandler<DiagnosticoRegistradoDomainEvent>
{
    private readonly ISagaCommandBus _bus;

    public EnviarGerarOrcamentoHandler(ISagaCommandBus bus)
    {
        _bus = bus;
    }

    public async Task Handle(DiagnosticoRegistradoDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var servicos = domainEvent.Servicos
            .Select(s => new ItemServicoDiagnosticado(s.IdServico, s.NomeServico, s.Valor))
            .ToList();

        var produtos = domainEvent.Produtos
            .Select(p => new ItemProdutoDiagnosticado(p.IdProduto, p.NomeProduto, p.ValorUnitario, p.Quantidade))
            .ToList();

        await _bus.EnviarGerarOrcamento(domainEvent.IdOrdemServico, servicos, produtos, domainEvent.ValorTotal, cancellationToken);
    }
}
