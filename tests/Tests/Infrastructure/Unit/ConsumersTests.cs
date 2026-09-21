using Domain.Clientes;
using Domain.Clientes.ValueObjects;
using Domain.Clientes.Veiculos;
using Domain.Clientes.Veiculos.ValueObjects;
using Domain.OrdensServico;
using Domain.OrdensServico.Gateways;
using Domain.OrdensServico.Servicos;
using FluentAssertions;
using Infrastructure.Messaging.Consumers;
using MassTransit;
using Moq;
using Soat.Contracts.Saga;

namespace Tests.Infrastructure.Unit;

// Os consumers MassTransit são o "lado de entrada" da saga: adaptam o evento
// vindo do Billing/Execução Service pro use case existente (mesma regra usada
// pelos endpoints internos equivalentes) e traduzem "não encontrada" em
// exceção (o MassTransit trata isso como falha de entrega/retry).
public class ConsumersTests
{
    private static OrdemServico CriarOrdemServicoRecebida()
    {
        var cliente = new Cliente();
        cliente.Inserir("João Silva", DocumentoCliente.Criar("12345678909"));
        var veiculo = new Veiculo();
        veiculo.Inserir(cliente.Id, Placa.Criar("ABC1234"), "Toyota", "Corolla", 2020);

        var os = new OrdemServico();
        os.Inserir(cliente.Id, veiculo.Id);
        return os;
    }

    private static Mock<ConsumeContext<T>> CriarContexto<T>(T mensagem) where T : class
    {
        var context = new Mock<ConsumeContext<T>>();
        context.SetupGet(c => c.Message).Returns(mensagem);
        context.SetupGet(c => c.CancellationToken).Returns(CancellationToken.None);
        return context;
    }

    [Fact]
    public async Task DiagnosticoFalhouConsumer_QuandoOsExiste_Cancela()
    {
        var os = CriarOrdemServicoRecebida();
        var gateway = new Mock<IOrdemServicoGateway>();
        gateway.Setup(g => g.BuscarPorId(os.Id, It.IsAny<CancellationToken>())).ReturnsAsync(os);

        var consumer = new DiagnosticoFalhouConsumer(gateway.Object);
        var context = CriarContexto(new DiagnosticoFalhou(os.Id, "Veículo não atendível"));

        await consumer.Consume(context.Object);

        os.Status.Should().Be(Domain.OrdensServico.Enums.StatusOrdemServico.Cancelada);
        gateway.Verify(g => g.Atualizar(os, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DiagnosticoFalhouConsumer_QuandoOsNaoExiste_LancaExcecao()
    {
        var gateway = new Mock<IOrdemServicoGateway>();
        gateway.Setup(g => g.BuscarPorId(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((OrdemServico?)null);

        var consumer = new DiagnosticoFalhouConsumer(gateway.Object);
        var context = CriarContexto(new DiagnosticoFalhou(Guid.NewGuid(), "Veículo não atendível"));

        var act = () => consumer.Consume(context.Object);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task DiagnosticoFinalizadoConsumer_QuandoOsExiste_RegistraDiagnostico()
    {
        var os = CriarOrdemServicoRecebida();
        os.IniciarDiagnostico();

        var gateway = new Mock<IOrdemServicoGateway>();
        gateway.Setup(g => g.BuscarPorId(os.Id, It.IsAny<CancellationToken>())).ReturnsAsync(os);

        var consumer = new DiagnosticoFinalizadoConsumer(gateway.Object);
        var evento = new DiagnosticoFinalizado(
            os.Id,
            [new ItemServicoDiagnosticado(Guid.NewGuid(), "Troca de óleo", 120m)],
            [new ItemProdutoDiagnosticado(Guid.NewGuid(), "Óleo 5W30", 79.90m, 2m)]);
        var context = CriarContexto(evento);

        await consumer.Consume(context.Object);

        os.Status.Should().Be(Domain.OrdensServico.Enums.StatusOrdemServico.AguardandoAprovacao);
        os.ValorTotal.Should().Be(279.80m);
    }

    [Fact]
    public async Task DiagnosticoFinalizadoConsumer_QuandoOsNaoExiste_LancaExcecao()
    {
        var gateway = new Mock<IOrdemServicoGateway>();
        gateway.Setup(g => g.BuscarPorId(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((OrdemServico?)null);

        var consumer = new DiagnosticoFinalizadoConsumer(gateway.Object);
        var context = CriarContexto(new DiagnosticoFinalizado(Guid.NewGuid(), [], []));

        var act = () => consumer.Consume(context.Object);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task ExecucaoFinalizadaConsumer_QuandoOsExiste_Finaliza()
    {
        var os = CriarOrdemServicoRecebida();
        os.IniciarDiagnostico();
        os.RegistrarDiagnostico([new OrdemServicoServico(os.Id, Guid.NewGuid(), "Troca de óleo", 120m)], []);
        os.AprovarPagamento();

        var gateway = new Mock<IOrdemServicoGateway>();
        gateway.Setup(g => g.BuscarPorId(os.Id, It.IsAny<CancellationToken>())).ReturnsAsync(os);

        var consumer = new ExecucaoFinalizadaConsumer(gateway.Object);
        var context = CriarContexto(new ExecucaoFinalizada(os.Id));

        await consumer.Consume(context.Object);

        os.Status.Should().Be(Domain.OrdensServico.Enums.StatusOrdemServico.Finalizada);
    }

    [Fact]
    public async Task ExecucaoFinalizadaConsumer_QuandoOsNaoExiste_LancaExcecao()
    {
        var gateway = new Mock<IOrdemServicoGateway>();
        gateway.Setup(g => g.BuscarPorId(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((OrdemServico?)null);

        var consumer = new ExecucaoFinalizadaConsumer(gateway.Object);
        var context = CriarContexto(new ExecucaoFinalizada(Guid.NewGuid()));

        var act = () => consumer.Consume(context.Object);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task PagamentoAprovadoConsumer_QuandoOsExiste_AprovaPagamento()
    {
        var os = CriarOrdemServicoRecebida();
        os.IniciarDiagnostico();
        os.RegistrarDiagnostico([new OrdemServicoServico(os.Id, Guid.NewGuid(), "Troca de óleo", 120m)], []);

        var gateway = new Mock<IOrdemServicoGateway>();
        gateway.Setup(g => g.BuscarPorId(os.Id, It.IsAny<CancellationToken>())).ReturnsAsync(os);

        var consumer = new PagamentoAprovadoConsumer(gateway.Object);
        var context = CriarContexto(new PagamentoAprovado(os.Id, Guid.NewGuid()));

        await consumer.Consume(context.Object);

        os.Status.Should().Be(Domain.OrdensServico.Enums.StatusOrdemServico.EmExecucao);
    }

    [Fact]
    public async Task PagamentoAprovadoConsumer_QuandoOsNaoExiste_LancaExcecao()
    {
        var gateway = new Mock<IOrdemServicoGateway>();
        gateway.Setup(g => g.BuscarPorId(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((OrdemServico?)null);

        var consumer = new PagamentoAprovadoConsumer(gateway.Object);
        var context = CriarContexto(new PagamentoAprovado(Guid.NewGuid(), Guid.NewGuid()));

        var act = () => consumer.Consume(context.Object);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task PagamentoRecusadoConsumer_QuandoOsExiste_Cancela()
    {
        var os = CriarOrdemServicoRecebida();
        var gateway = new Mock<IOrdemServicoGateway>();
        gateway.Setup(g => g.BuscarPorId(os.Id, It.IsAny<CancellationToken>())).ReturnsAsync(os);

        var consumer = new PagamentoRecusadoConsumer(gateway.Object);
        var context = CriarContexto(new PagamentoRecusado(os.Id, Guid.NewGuid(), "Pagamento expirado"));

        await consumer.Consume(context.Object);

        os.Status.Should().Be(Domain.OrdensServico.Enums.StatusOrdemServico.Cancelada);
    }

    [Fact]
    public async Task PagamentoRecusadoConsumer_QuandoOsNaoExiste_LancaExcecao()
    {
        var gateway = new Mock<IOrdemServicoGateway>();
        gateway.Setup(g => g.BuscarPorId(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((OrdemServico?)null);

        var consumer = new PagamentoRecusadoConsumer(gateway.Object);
        var context = CriarContexto(new PagamentoRecusado(Guid.NewGuid(), Guid.NewGuid(), "Pagamento expirado"));

        var act = () => consumer.Consume(context.Object);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task OrcamentoFalhouConsumer_QuandoOsExiste_Cancela()
    {
        var os = CriarOrdemServicoRecebida();
        var gateway = new Mock<IOrdemServicoGateway>();
        gateway.Setup(g => g.BuscarPorId(os.Id, It.IsAny<CancellationToken>())).ReturnsAsync(os);

        var consumer = new OrcamentoFalhouConsumer(gateway.Object);
        var context = CriarContexto(new OrcamentoFalhou(os.Id, "Mercado Pago indisponível"));

        await consumer.Consume(context.Object);

        os.Status.Should().Be(Domain.OrdensServico.Enums.StatusOrdemServico.Cancelada);
        gateway.Verify(g => g.Atualizar(os, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task OrcamentoFalhouConsumer_QuandoOsNaoExiste_LancaExcecao()
    {
        var gateway = new Mock<IOrdemServicoGateway>();
        gateway.Setup(g => g.BuscarPorId(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((OrdemServico?)null);

        var consumer = new OrcamentoFalhouConsumer(gateway.Object);
        var context = CriarContexto(new OrcamentoFalhou(Guid.NewGuid(), "Mercado Pago indisponível"));

        var act = () => consumer.Consume(context.Object);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task ExecucaoFalhouConsumer_QuandoOsExiste_Cancela()
    {
        var os = CriarOrdemServicoRecebida();
        var gateway = new Mock<IOrdemServicoGateway>();
        gateway.Setup(g => g.BuscarPorId(os.Id, It.IsAny<CancellationToken>())).ReturnsAsync(os);

        var consumer = new ExecucaoFalhouConsumer(gateway.Object);
        var context = CriarContexto(new ExecucaoFalhou(os.Id, "Peça indisponível"));

        await consumer.Consume(context.Object);

        os.Status.Should().Be(Domain.OrdensServico.Enums.StatusOrdemServico.Cancelada);
        gateway.Verify(g => g.Atualizar(os, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecucaoFalhouConsumer_QuandoOsNaoExiste_LancaExcecao()
    {
        var gateway = new Mock<IOrdemServicoGateway>();
        gateway.Setup(g => g.BuscarPorId(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((OrdemServico?)null);

        var consumer = new ExecucaoFalhouConsumer(gateway.Object);
        var context = CriarContexto(new ExecucaoFalhou(Guid.NewGuid(), "Peça indisponível"));

        var act = () => consumer.Consume(context.Object);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
