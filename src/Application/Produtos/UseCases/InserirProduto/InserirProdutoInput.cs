namespace Application.Produtos.UseCases.InserirProduto;

public record InserirProdutoInput(string Nome, string Descricao, decimal Valor, int QuantidadeEmEstoque);
