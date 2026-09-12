using Domain.Clientes;
using Domain.Clientes.ValueObjects;
using Domain.Clientes.Veiculos;
using Domain.Clientes.Veiculos.ValueObjects;
using Domain.OrdensServico;
using Domain.OrdensServico.Historico;
using Domain.Produtos;
using Domain.Servicos;
using Domain.Usuarios;
using Domain.Usuarios.Roles;
using FluentAssertions;
using Infrastructure.Database;
using Infrastructure.DomainEvents;
using Infrastructure.Gateways.Clientes;
using Infrastructure.Gateways.OrdensServico;
using Infrastructure.Gateways.Produtos;
using Infrastructure.Gateways.Servicos;
using Infrastructure.Gateways.Usuarios;
using Microsoft.EntityFrameworkCore;
using Moq;
using SharedKernel.DTOs;

namespace Tests.Infrastructure.Unit;

// Gateways são adaptadores EF Core dos ports do domínio — cobertos aqui com o
// provider InMemory (sem precisar de um Postgres real), exercitando as
// consultas e operações de escrita reais de cada um.
public class GatewaysTests
{
    private static OsServiceDbContext CriarContexto() =>
        new(new DbContextOptionsBuilder<OsServiceDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options,
            Mock.Of<IDomainEventsDispatcher>());

    [Fact]
    public async Task ClienteGateway_CrudECunsultasFuncionamCorretamente()
    {
        await using var db = CriarContexto();
        var gateway = new ClienteGateway(db);

        var cliente = new Cliente();
        cliente.Inserir("João Silva", DocumentoCliente.Criar("12345678909"));
        var veiculo = new Veiculo();
        veiculo.Inserir(cliente.Id, Placa.Criar("ABC1234"), "Toyota", "Corolla", 2020);
        cliente.Veiculos.Add(veiculo);

        await gateway.Salvar(cliente);

        (await gateway.BuscarPorId(cliente.Id)).Should().NotBeNull();
        (await gateway.BuscarPorDocumento("12345678909")).Should().NotBeNull();
        (await gateway.BuscarComVeiculos(cliente.Id))!.Veiculos.Should().ContainSingle();
        (await gateway.ExisteComDocumento("12345678909")).Should().BeTrue();
        (await gateway.ExisteComDocumento("00000000000")).Should().BeFalse();

        var (items, total) = await gateway.BuscarPaginado(new PagedRequest(1, 10));
        items.Should().ContainSingle();
        total.Should().Be(1);

        cliente.Atualizar("Novo Nome");
        await gateway.Atualizar(cliente);
        (await gateway.BuscarPorId(cliente.Id))!.Nome.Should().Be("Novo Nome");

        await gateway.Remover(cliente);
        (await gateway.BuscarPorId(cliente.Id)).Should().BeNull();
    }

    [Fact]
    public async Task VeiculoGateway_CrudECunsultasFuncionamCorretamente()
    {
        await using var db = CriarContexto();
        var gateway = new VeiculoGateway(db);

        var idCliente = Guid.NewGuid();
        var veiculo = new Veiculo();
        veiculo.Inserir(idCliente, Placa.Criar("ABC1234"), "Toyota", "Corolla", 2020);

        await gateway.Inserir(veiculo);

        (await gateway.BuscarPorId(veiculo.Id)).Should().NotBeNull();
        (await gateway.BuscarPorPlaca("ABC1234")).Should().NotBeNull();
        (await gateway.ExisteComPlaca("ABC1234")).Should().BeTrue();
        (await gateway.ExisteComPlacaExcetoId("ABC1234", veiculo.Id)).Should().BeFalse();
        (await gateway.ExisteComPlacaExcetoId("ABC1234", Guid.NewGuid())).Should().BeTrue();

        var (items, total) = await gateway.BuscarPaginadoPorCliente(idCliente, new PagedRequest(1, 10));
        items.Should().ContainSingle();
        total.Should().Be(1);

        veiculo.Atualizar(Placa.Criar("DEF5678"), "Honda", "Civic", 2021);
        await gateway.Atualizar(veiculo);
        (await gateway.BuscarPorPlaca("DEF5678")).Should().NotBeNull();

        await gateway.Remover(veiculo);
        (await gateway.BuscarPorId(veiculo.Id)).Should().BeNull();
    }

    [Fact]
    public async Task ServicoGateway_CrudECunsultasFuncionamCorretamente()
    {
        await using var db = CriarContexto();
        var gateway = new ServicoGateway(db);

        var servico = new Servico();
        servico.Inserir("Troca de óleo", "Substituição do óleo do motor", 120m);

        await gateway.Salvar(servico);

        (await gateway.BuscarPorId(servico.Id)).Should().NotBeNull();
        (await gateway.BuscarPorIds([servico.Id])).Should().ContainKey(servico.Id);

        var (items, total) = await gateway.BuscarPaginado(new PagedRequest(1, 10));
        items.Should().ContainSingle();
        total.Should().Be(1);

        servico.Atualizar("Novo Nome", "Nova Descrição", 150m);
        await gateway.Atualizar(servico);
        (await gateway.BuscarPorId(servico.Id))!.Nome.Should().Be("Novo Nome");

        await gateway.Remover(servico);
        (await gateway.BuscarPorId(servico.Id)).Should().BeNull();
    }

    [Fact]
    public async Task ProdutoGateway_CrudECunsultasFuncionamCorretamente()
    {
        await using var db = CriarContexto();
        var gateway = new ProdutoGateway(db);

        var produto = new Produto();
        produto.Inserir("Óleo Motor 5W30", "Óleo sintético", 79.90m, 10);

        await gateway.Salvar(produto);

        (await gateway.BuscarPorId(produto.Id)).Should().NotBeNull();
        (await gateway.BuscarDicionarioPorIds([produto.Id])).Should().ContainKey(produto.Id);
        (await gateway.BuscarPorIds([produto.Id])).Should().ContainSingle();

        var (items, total) = await gateway.BuscarPaginado(null, new PagedRequest(1, 10));
        items.Should().ContainSingle();
        total.Should().Be(1);

        produto.Atualizar("Novo Nome", "Nova Descrição", 100m);
        await gateway.Atualizar(produto);
        (await gateway.BuscarPorId(produto.Id))!.Nome.Should().Be("Novo Nome");

        await gateway.AtualizarLote([produto]);

        await gateway.Remover(produto);
        (await gateway.BuscarPorId(produto.Id)).Should().BeNull();
    }

    [Fact]
    public async Task OrdemServicoGateway_CrudECunsultasFuncionamCorretamente()
    {
        await using var db = CriarContexto();
        var gateway = new OrdemServicoGateway(db);

        var cliente = new Cliente();
        cliente.Inserir("João Silva", DocumentoCliente.Criar("12345678909"));
        db.Cliente.Add(cliente);
        await db.SaveChangesAsync();

        var os = new OrdemServico();
        os.Inserir(cliente.Id, Guid.NewGuid());

        await gateway.Salvar(os);

        (await gateway.BuscarPorId(os.Id)).Should().NotBeNull();
        (await gateway.BuscarComItens(os.Id))!.Servicos.Should().BeEmpty();

        var (items, total) = await gateway.BuscarPaginado(new PagedRequest(1, 10));
        items.Should().ContainSingle();
        total.Should().Be(1);

        var (itensPorDocumento, totalPorDocumento) = await gateway.BuscarPaginadoPorDocumentoCliente("12345678909", new PagedRequest(1, 10));
        itensPorDocumento.Should().ContainSingle();
        totalPorDocumento.Should().Be(1);

        var (itensDocumentoInexistente, totalDocumentoInexistente) = await gateway.BuscarPaginadoPorDocumentoCliente("00000000000", new PagedRequest(1, 10));
        itensDocumentoInexistente.Should().BeEmpty();
        totalDocumentoInexistente.Should().Be(0);

        os.IniciarDiagnostico();
        await gateway.Atualizar(os);
        (await gateway.BuscarPorId(os.Id))!.Status.Should().Be(Domain.OrdensServico.Enums.StatusOrdemServico.EmDiagnostico);

        await gateway.Remover(os);
        (await gateway.BuscarPorId(os.Id)).Should().BeNull();
    }

    [Fact]
    public async Task HistoricoStatusOrdemServicoGateway_SalvaEConsultaOrdenadoPorData()
    {
        await using var db = CriarContexto();
        var gateway = new HistoricoStatusOrdemServicoGateway(db);

        var idOrdemServico = Guid.NewGuid();
        var maisAntigo = new HistoricoStatusOrdemServico(idOrdemServico, Domain.OrdensServico.Enums.StatusOrdemServico.Recebida, DateTime.UtcNow.AddMinutes(-10));
        var maisRecente = new HistoricoStatusOrdemServico(idOrdemServico, Domain.OrdensServico.Enums.StatusOrdemServico.EmDiagnostico, DateTime.UtcNow);

        await gateway.Salvar(maisRecente);
        await gateway.Salvar(maisAntigo);

        var historico = await gateway.BuscarPorOrdemServico(idOrdemServico);

        historico.Should().HaveCount(2);
        historico[0].Status.Should().Be(Domain.OrdensServico.Enums.StatusOrdemServico.Recebida);
    }

    [Fact]
    public async Task UsuarioGateway_SalvaEBuscaPorEmailComRoles()
    {
        await using var db = CriarContexto();
        var gateway = new UsuarioGateway(db);

        var usuario = new Usuario("Admin", "admin@teste.com", "hash", "12345678909");
        usuario.AdicionarRoles([new UsuarioRole("Admin")]);

        await gateway.Salvar(usuario, CancellationToken.None);

        var encontrado = await gateway.BuscarPorEmail("admin@teste.com");
        encontrado.Should().NotBeNull();
        encontrado!.Roles.Should().ContainSingle(r => r.Role == "Admin");

        (await gateway.BuscarPorEmail("naoexiste@teste.com")).Should().BeNull();
    }
}
