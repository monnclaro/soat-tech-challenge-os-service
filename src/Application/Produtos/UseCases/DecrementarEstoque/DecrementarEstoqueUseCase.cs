using Application.Common.Interfaces;
using Domain.Produtos.Gateways;

namespace Application.Produtos.UseCases.DecrementarEstoque;

public class DecrementarEstoqueUseCase : IUseCase
{
    private readonly IProdutoGateway _gateway;
    private readonly IDecrementarEstoqueOutputPort _outputPort;

    public DecrementarEstoqueUseCase(IProdutoGateway gateway, IDecrementarEstoqueOutputPort outputPort)
    {
        _gateway = gateway;
        _outputPort = outputPort;
    }

    public async Task Execute(DecrementarEstoqueInput input, CancellationToken ct = default)
    {
        var ids = input.Produtos.Select(p => p.Id).ToList();
        var produtos = await _gateway.BuscarPorIds(ids, ct);

        var dicionario = input.Produtos.ToDictionary(p => p.Id);

        foreach (var produto in produtos)
        {
            var item = dicionario[produto.Id];
            produto.DecrementarQuantidadeEmEstoque(item.Quantidade);
        }

        await _gateway.AtualizarLote(produtos, ct);

        _outputPort.Ok();
    }
}
