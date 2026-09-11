using Application.Common.Interfaces;
using Application.Produtos.DTOs;
using Domain.Produtos.Gateways;

namespace Application.Produtos.UseCases.BuscarProduto;

public class BuscarProdutoUseCase : IUseCase
{
    private readonly IProdutoGateway _gateway;
    private readonly IBuscarProdutoOutputPort _outputPort;

    public BuscarProdutoUseCase(IProdutoGateway gateway, IBuscarProdutoOutputPort outputPort)
    {
        _gateway = gateway;
        _outputPort = outputPort;
    }

    public async Task Execute(BuscarProdutoInput input, CancellationToken ct = default)
    {
        var produto = await _gateway.BuscarPorId(input.Id, ct);

        if (produto is null)
        {
            _outputPort.NaoEncontrado();
            return;
        }

        _outputPort.Ok(new ProdutoOutput(produto.Id, produto.Nome, produto.Descricao, produto.Valor, produto.QuantidadeEmEstoque));
    }
}
