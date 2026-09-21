namespace Application.Produtos.UseCases.AtualizarProduto;

public record AtualizarProdutoInput(Guid Id, string Nome, string Descricao, decimal Valor);
