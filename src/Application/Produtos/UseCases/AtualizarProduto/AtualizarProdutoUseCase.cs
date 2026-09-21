using Application.Common.Interfaces;
using Application.Produtos.DTOs;
using Domain.Produtos.Gateways;

namespace Application.Produtos.UseCases.AtualizarProduto;

public class AtualizarProdutoUseCase : IUseCase
{
    private readonly IProdutoGateway _gateway;
    private readonly IAtualizarProdutoOutputPort _outputPort;

    public AtualizarProdutoUseCase(IProdutoGateway gateway, IAtualizarProdutoOutputPort outputPort)
    {
        _gateway = gateway;
        _outputPort = outputPort;
    }

    public async Task Execute(AtualizarProdutoInput input, CancellationToken ct = default)
    {
        var produto = await _gateway.BuscarPorId(input.Id, ct);

        if (produto is null)
        {
            _outputPort.NaoEncontrado();
            return;
        }

        produto.Atualizar(input.Nome, input.Descricao, input.Valor);
        await _gateway.Atualizar(produto, ct);

        _outputPort.Ok(new ProdutoOutput(produto.Id, produto.Nome, produto.Descricao, produto.Valor, produto.QuantidadeEmEstoque));
    }
}
