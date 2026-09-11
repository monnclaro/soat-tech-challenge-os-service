using Application.Common.Interfaces;
using Application.Produtos.DTOs;
using Domain.Produtos;
using Domain.Produtos.Gateways;

namespace Application.Produtos.UseCases.InserirProduto;

public class InserirProdutoUseCase : IUseCase
{
    private readonly IProdutoGateway _gateway;
    private readonly IInserirProdutoOutputPort _outputPort;

    public InserirProdutoUseCase(IProdutoGateway gateway, IInserirProdutoOutputPort outputPort)
    {
        _gateway = gateway;
        _outputPort = outputPort;
    }

    public async Task Execute(InserirProdutoInput input, CancellationToken ct = default)
    {
        var produto = new Produto();
        produto.Inserir(input.Nome, input.Descricao, input.Valor, input.QuantidadeEmEstoque);

        await _gateway.Salvar(produto, ct);

        _outputPort.Ok(new ProdutoOutput(produto.Id, produto.Nome, produto.Descricao, produto.Valor, produto.QuantidadeEmEstoque));
    }
}
