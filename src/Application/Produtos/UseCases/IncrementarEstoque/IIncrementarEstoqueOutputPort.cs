using Application.Produtos.DTOs;

namespace Application.Produtos.UseCases.IncrementarEstoque;

public interface IIncrementarEstoqueOutputPort
{
    void NaoEncontrado();
    void Ok(ProdutoOutput output);
}
