using Application.Produtos.DTOs;

namespace Application.Produtos.UseCases.AtualizarProduto;

public interface IAtualizarProdutoOutputPort
{
    void NaoEncontrado();
    void Ok(ProdutoOutput output);
}
