using Application.Produtos.DTOs;

namespace Application.Produtos.UseCases.InserirProduto;

public interface IInserirProdutoOutputPort
{
    void Ok(ProdutoOutput output);
}
