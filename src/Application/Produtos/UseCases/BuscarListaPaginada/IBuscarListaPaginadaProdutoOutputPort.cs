using Application.Produtos.DTOs;
using SharedKernel.DTOs;

namespace Application.Produtos.UseCases.BuscarListaPaginada;

public interface IBuscarListaPaginadaProdutoOutputPort
{
    void Ok(PagedResult<ProdutoOutput> resultado);
}
