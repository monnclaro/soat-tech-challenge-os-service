using Application.Common.Interfaces;
using Application.Produtos.DTOs;
using Domain.Produtos.Gateways;

namespace Application.Produtos.UseCases.IncrementarEstoque;

public class IncrementarEstoqueUseCase : IUseCase
{
    private readonly IProdutoGateway _gateway;
    private readonly IIncrementarEstoqueOutputPort _outputPort;

    public IncrementarEstoqueUseCase(IProdutoGateway gateway, IIncrementarEstoqueOutputPort outputPort)
    {
        _gateway = gateway;
        _outputPort = outputPort;
    }

    public async Task Execute(IncrementarEstoqueInput input, CancellationToken ct = default)
    {
        var produto = await _gateway.BuscarPorId(input.Id, ct);

        if (produto is null)
        {
            _outputPort.NaoEncontrado();
            return;
        }

        produto.IncrementarQuantidadeEmEstoque(input.Quantidade);
        await _gateway.Atualizar(produto, ct);

        _outputPort.Ok(new ProdutoOutput(produto.Id, produto.Nome, produto.Descricao, produto.Valor, produto.QuantidadeEmEstoque));
    }
}
