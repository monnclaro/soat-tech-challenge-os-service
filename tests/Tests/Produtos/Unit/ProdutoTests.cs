using Domain.Common.Exceptions;
using Domain.Produtos;
using FluentAssertions;

namespace Tests.Produtos.Unit;

public class ProdutoTests
{
    private static Produto CriarProduto(decimal quantidadeEmEstoque = 10)
    {
        var produto = new Produto();
        produto.Inserir("Óleo 5W30", "Óleo sintético", 79.90m, quantidadeEmEstoque);
        return produto;
    }

    [Fact]
    public void Inserir_ComValorNegativo_LancaExcecao()
    {
        var produto = new Produto();

        var act = () => produto.Inserir("Óleo", "desc", -1, 10);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void IncrementarQuantidadeEmEstoque_SomaQuantidade()
    {
        var produto = CriarProduto(10);

        produto.IncrementarQuantidadeEmEstoque(5);

        produto.QuantidadeEmEstoque.Should().Be(15);
    }

    [Fact]
    public void IncrementarQuantidadeEmEstoque_ComQuantidadeZeroOuNegativa_LancaExcecao()
    {
        var produto = CriarProduto();

        var act = () => produto.IncrementarQuantidadeEmEstoque(0);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void DecrementarQuantidadeEmEstoque_SubtraiQuantidade()
    {
        var produto = CriarProduto(10);

        produto.DecrementarQuantidadeEmEstoque(4);

        produto.QuantidadeEmEstoque.Should().Be(6);
    }

    [Fact]
    public void DecrementarQuantidadeEmEstoque_AlemDoDisponivel_ZeraEmVezDeFicarNegativo()
    {
        var produto = CriarProduto(3);

        produto.DecrementarQuantidadeEmEstoque(10);

        produto.QuantidadeEmEstoque.Should().Be(0);
    }
}
