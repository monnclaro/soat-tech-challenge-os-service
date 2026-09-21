using Application.Common.Interfaces;
using Application.Produtos.UseCases.DecrementarEstoque;
using Domain.OrdensServico.Events;

namespace Application.OrdensServico.EventHandlers;

internal sealed class OrdemServicoEventHandler : IDomainEventHandler<OrdemServicoFinalizadaDomainEvent>
{
    private readonly DecrementarEstoqueUseCase _useCase;
    private readonly IDecrementarEstoqueOutputPort _outputPort;

    public OrdemServicoEventHandler(
        DecrementarEstoqueUseCase useCase,
        IDecrementarEstoqueOutputPort outputPort)
    {
        _useCase = useCase;
        _outputPort = outputPort;
    }

    public async Task Handle(OrdemServicoFinalizadaDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var input = new DecrementarEstoqueInput(
            domainEvent.Produtos
                .Select(p => new DecrementarEstoqueItem(p.IdProduto, p.Quantidade))
                .ToList());

        await _useCase.Execute(input, cancellationToken);
    }
}
