using Domain.Clientes;
using Domain.OrdensServico;
using Domain.OrdensServico.Produtos;
using Domain.OrdensServico.Servicos;
using Domain.Produtos;
using Domain.Servicos;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Seeders.OrdensServico;

public static class OrdensServicoSeeder
{
    public static async Task SeedAsync(OsServiceDbContext context)
    {
        if (await context.OrdemServico.AsNoTracking().AnyAsync()) return;

        var clientes = await context.Cliente.Include(l => l.Veiculos).Take(3).ToListAsync();
        var produtos = await context.Produto.Take(3).ToListAsync();
        var servicos = await context.Servico.Take(3).ToListAsync();

        var ordens = new List<OrdemServico>
        {
            CriarOrdem(clientes[0], produtos, servicos),
            CriarOrdem(clientes[1], produtos, servicos),
            CriarOrdem(clientes[2], produtos, servicos)
        };

        await context.OrdemServico.AddRangeAsync(ordens);
        await context.SaveChangesAsync();
    }

    // Semeia direto em "AguardandoAprovacao" (como se o diagnóstico do Execução Service já
    // tivesse sido concluído) — sem simular a mensageria entre serviços aqui.
    private static OrdemServico CriarOrdem(Cliente cliente, List<Produto> produtos, List<Servico> servicos)
    {
        var ordem = new OrdemServico();
        ordem.Inserir(cliente.Id, cliente.Veiculos[0].Id);
        ordem.IniciarDiagnostico();

        var servicosDiagnosticados = servicos.Select(s => new OrdemServicoServico(ordem.Id, s.Id, s.Nome, s.Valor)).ToList();
        var produtosDiagnosticados = produtos.Select(p => new OrdemServicoProduto(ordem.Id, p.Id, p.Nome, p.Valor, 1)).ToList();

        ordem.RegistrarDiagnostico(servicosDiagnosticados, produtosDiagnosticados);

        return ordem;
    }
}
