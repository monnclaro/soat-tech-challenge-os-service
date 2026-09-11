using Domain.Clientes;
using Domain.Clientes.ValueObjects;
using Domain.Clientes.Veiculos;
using Domain.Clientes.Veiculos.ValueObjects;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Seeders.Clientes;

public static class ClienteSeeder
{
    public static async Task SeedAsync(OsServiceDbContext context)
    {
        if (await context.Cliente.AnyAsync()) return;

        var cliente1 = CriarCliente(
            "João Silva",
            DocumentoCliente.Criar("12345678909"),
            [
                CriarVeiculo("ABC1234", "Toyota", "Corolla", 2019),
                CriarVeiculo("BRA2E19", "Honda", "Civic", 2021)
            ]);

        var cliente2 = CriarCliente(
            "Maria Oliveira",
            DocumentoCliente.Criar("98765432100"),
            [
                CriarVeiculo("DEF5678", "Volkswagen", "Golf", 2018)
            ]);

        var cliente3 = CriarCliente(
            "Oficina Mecânica Brasil LTDA",
            DocumentoCliente.Criar("11222333000181"),
            [
                CriarVeiculo("DCF5678", "Volkswagen", "Golf", 2014)
            ]);

        await context.Cliente.AddRangeAsync(cliente1, cliente2, cliente3);
        await context.SaveChangesAsync();
    }

    private static Cliente CriarCliente(
        string nome,
        DocumentoCliente documento,
        List<Veiculo> veiculos)
    {
        var cliente = new Cliente();
        cliente.Inserir(nome, documento);
        cliente.Veiculos.AddRange(veiculos);
        return cliente;
    }

    private static Veiculo CriarVeiculo(
        string placa,
        string marca,
        string modelo,
        int ano)
    {
        var veiculo = new Veiculo();
        veiculo.Inserir(Guid.Empty, Placa.Criar(placa), marca, modelo, ano);
        return veiculo;
    }
}
