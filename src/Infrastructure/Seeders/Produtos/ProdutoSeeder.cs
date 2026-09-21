using Domain.Produtos;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Seeders.Produtos;

public static class ProdutoSeeder
{
    public static async Task SeedAsync(OsServiceDbContext context)
    {
        if (await context.Produto.AnyAsync()) return;

        var produtos = new List<Produto>
        {
            CriarProduto("Óleo Motor 5W30", "Óleo sintético para motor", 79.90m, 50),
            CriarProduto("Filtro de Óleo", "Filtro de óleo para motor", 35.00m, 100),
            CriarProduto("Filtro de Ar", "Filtro de ar do motor", 40.00m, 80),
            CriarProduto("Pastilha de Freio", "Pastilha de freio dianteira", 120.00m, 60),
            CriarProduto("Bateria 60Ah", "Bateria automotiva 60Ah", 450.00m, 20)
        };

        await context.Produto.AddRangeAsync(produtos);
        await context.SaveChangesAsync();
    }

    private static Produto CriarProduto(string nome, string descricao, decimal valor, decimal estoque)
    {
        var produto = new Produto();
        produto.Inserir(nome, descricao, valor, estoque);
        return produto;
    }
}
