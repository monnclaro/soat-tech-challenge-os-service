using Application.Produtos.DTOs;

namespace Application.Produtos.UseCases.BuscarProduto;

public interface IBuscarProdutoOutputPort
{
    void NaoEncontrado();
    void Ok(ProdutoOutput output);
}
