using Application.Common.Interfaces;
using Application.Produtos.DTOs;
using Domain.Produtos.Gateways;
using SharedKernel.DTOs;

namespace Application.Produtos.UseCases.BuscarListaPaginada;

public class BuscarListaPaginadaProdutoUseCase : IUseCase
{
    private readonly IProdutoGateway _gateway;
    private readonly IBuscarListaPaginadaProdutoOutputPort _outputPort;

    public BuscarListaPaginadaProdutoUseCase(IProdutoGateway gateway, IBuscarListaPaginadaProdutoOutputPort outputPort)
    {
        _gateway = gateway;
        _outputPort = outputPort;
    }

    public async Task Execute(BuscarListaPaginadaInput input, CancellationToken ct = default)
    {
        var (items, total) = await _gateway.BuscarPaginado(null, input.Paginacao, ct);

        var output = new PagedResult<ProdutoOutput>(
            items.Select(p => new ProdutoOutput(p.Id, p.Nome, p.Descricao, p.Valor, p.QuantidadeEmEstoque)).ToList(),
            total,
            input.Paginacao.Pagina,
            input.Paginacao.Tamanho);

        _outputPort.Ok(output);
    }
}
