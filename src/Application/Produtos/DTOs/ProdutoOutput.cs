namespace Application.Produtos.DTOs;

public record ProdutoOutput(Guid Id, string Nome, string Descricao, decimal Valor, decimal QuantidadeEmEstoque);
