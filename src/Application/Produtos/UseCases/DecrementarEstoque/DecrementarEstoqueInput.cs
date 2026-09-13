namespace Application.Produtos.UseCases.DecrementarEstoque;

public record DecrementarEstoqueItem(Guid Id, decimal Quantidade);

public record DecrementarEstoqueInput(IReadOnlyList<DecrementarEstoqueItem> Produtos);
