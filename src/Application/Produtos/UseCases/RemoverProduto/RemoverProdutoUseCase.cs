using Application.Common.Interfaces;
using Domain.Produtos.Gateways;

namespace Application.Produtos.UseCases.RemoverProduto;

public class RemoverProdutoUseCase : IUseCase
{
    private readonly IProdutoGateway _gateway;
    private readonly IRemoverProdutoOutputPort _outputPort;

    public RemoverProdutoUseCase(IProdutoGateway gateway, IRemoverProdutoOutputPort outputPort)
    {
        _gateway = gateway;
        _outputPort = outputPort;
    }

    public async Task Execute(RemoverProdutoInput input, CancellationToken ct = default)
    {
        var produto = await _gateway.BuscarPorId(input.Id, ct);

        if (produto is null)
        {
            _outputPort.Ok(); // idempotente
            return;
        }

        await _gateway.Remover(produto, ct);
        _outputPort.Ok();
    }
}
