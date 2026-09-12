using Application.Common.Interfaces;
using Application.OrdensServico.EventHandlers;
using Application.OrdensServico.UseCases.AprovarPagamento;
using Application.OrdensServico.UseCases.Cancelar;
using Application.OrdensServico.UseCases.Finalizar;
using Application.OrdensServico.UseCases.IniciarDiagnostico;
using Application.OrdensServico.UseCases.Inserir;
using Application.OrdensServico.UseCases.RegistrarDiagnostico;
using Domain.Clientes;
using Domain.Clientes.Gateways;
using Domain.Clientes.ValueObjects;
using Domain.Clientes.Veiculos;
using Domain.Clientes.Veiculos.Gateways;
using Domain.Clientes.Veiculos.ValueObjects;
using Domain.Common.Events;
using Domain.OrdensServico;
using Domain.OrdensServico.Events;
using Domain.OrdensServico.Gateways;
using FluentAssertions;
using Infrastructure.Messaging.Consumers;
using MassTransit;
using Moq;
using Reqnroll;
using SharedKernel.DTOs;
using Soat.Contracts.Saga;

namespace Tests.Features;

// Fake do gateway de OrdemServico que, em Salvar/Atualizar, imita o que
// OsServiceDbContext.SaveChangesAsync + DomainEventsDispatcher fariam em
// produção: coleta os domain events levantados pela entidade, limpa-os, e
// invoca os event handlers "de saída" da saga (que publicam comandos via o
// ISagaCommandBus fake). Sem EF Core / broker real — só o suficiente pra
// exercitar o fluxo de ponta a ponta dentro do processo de teste.
internal sealed class FakeOrdemServicoGateway : IOrdemServicoGateway
{
    private readonly Dictionary<Guid, OrdemServico> _armazenamento = new();
    private readonly EnviarIniciarDiagnosticoHandler _enviarIniciarDiagnostico;
    private readonly EnviarGerarOrcamentoHandler _enviarGerarOrcamento;
    private readonly EnviarIniciarExecucaoHandler _enviarIniciarExecucao;

    public FakeOrdemServicoGateway(ISagaCommandBus bus)
    {
        _enviarIniciarDiagnostico = new EnviarIniciarDiagnosticoHandler(bus);
        _enviarGerarOrcamento = new EnviarGerarOrcamentoHandler(bus);
        _enviarIniciarExecucao = new EnviarIniciarExecucaoHandler(bus);
    }

    public Task<OrdemServico?> BuscarPorId(Guid id, CancellationToken ct = default) =>
        Task.FromResult(_armazenamento.GetValueOrDefault(id));

    public Task<OrdemServico?> BuscarComItens(Guid id, CancellationToken ct = default) =>
        Task.FromResult(_armazenamento.GetValueOrDefault(id));

    public Task<(IReadOnlyList<OrdemServico> Items, int Total)> BuscarPaginado(PagedRequest p, CancellationToken ct = default) =>
        throw new NotSupportedException("Não usado neste cenário de BDD.");

    public Task<(IReadOnlyList<OrdemServico> Items, int Total)> BuscarPaginadoPorDocumentoCliente(string documento, PagedRequest p, CancellationToken ct = default) =>
        throw new NotSupportedException("Não usado neste cenário de BDD.");

    public async Task Salvar(OrdemServico ordemServico, CancellationToken ct = default)
    {
        _armazenamento[ordemServico.Id] = ordemServico;
        await DespacharEventos(ordemServico, ct);
    }

    public async Task Atualizar(OrdemServico ordemServico, CancellationToken ct = default)
    {
        _armazenamento[ordemServico.Id] = ordemServico;
        await DespacharEventos(ordemServico, ct);
    }

    public Task Remover(OrdemServico ordemServico, CancellationToken ct = default)
    {
        _armazenamento.Remove(ordemServico.Id);
        return Task.CompletedTask;
    }

    private async Task DespacharEventos(OrdemServico ordemServico, CancellationToken ct)
    {
        var eventos = ordemServico.DomainEvents;
        ordemServico.ClearDomainEvents();

        foreach (IDomainEvent evento in eventos)
        {
            switch (evento)
            {
                case OrdemServicoAbertaDomainEvent aberta:
                    await _enviarIniciarDiagnostico.Handle(aberta, ct);
                    break;
                case DiagnosticoRegistradoDomainEvent diagnosticoRegistrado:
                    await _enviarGerarOrcamento.Handle(diagnosticoRegistrado, ct);
                    break;
                case OrdemServicoStatusAlteradoDomainEvent statusAlterado:
                    await _enviarIniciarExecucao.Handle(statusAlterado, ct);
                    break;
            }
        }
    }
}

// Fake do port de saída da saga: só registra os comandos publicados, pra
// asserção posterior (equivalente ao "capture the ISagaCommands" do enunciado).
internal sealed class FakeSagaCommandBus : ISagaCommandBus
{
    public List<IniciarDiagnostico> IniciarDiagnosticoComandos { get; } = [];
    public List<GerarOrcamento> GerarOrcamentoComandos { get; } = [];
    public List<IniciarExecucao> IniciarExecucaoComandos { get; } = [];

    public Task EnviarIniciarDiagnostico(Guid idOrdemServico, Guid idCliente, Guid idVeiculo, CancellationToken ct = default)
    {
        IniciarDiagnosticoComandos.Add(new IniciarDiagnostico(idOrdemServico, idCliente, idVeiculo));
        return Task.CompletedTask;
    }

    public Task EnviarGerarOrcamento(
        Guid idOrdemServico,
        IReadOnlyList<ItemServicoDiagnosticado> servicos,
        IReadOnlyList<ItemProdutoDiagnosticado> produtos,
        decimal valorTotal,
        CancellationToken ct = default)
    {
        GerarOrcamentoComandos.Add(new GerarOrcamento(idOrdemServico, servicos, produtos, valorTotal));
        return Task.CompletedTask;
    }

    public Task EnviarIniciarExecucao(Guid idOrdemServico, CancellationToken ct = default)
    {
        IniciarExecucaoComandos.Add(new IniciarExecucao(idOrdemServico));
        return Task.CompletedTask;
    }
}

[Binding]
public class SagaOrdemServicoSteps
{
    private readonly FakeSagaCommandBus _bus = new();
    private FakeOrdemServicoGateway _gateway = null!;
    private Guid _idOrdemServico;
    private Guid _idCliente;
    private Guid _idVeiculo;

    private static Mock<ConsumeContext<T>> CriarContexto<T>(T mensagem) where T : class
    {
        var context = new Mock<ConsumeContext<T>>();
        context.SetupGet(c => c.Message).Returns(mensagem);
        context.SetupGet(c => c.CancellationToken).Returns(CancellationToken.None);
        return context;
    }

    private async Task<OrdemServico> BuscarOrdemServico()
    {
        var os = await _gateway.BuscarPorId(_idOrdemServico);
        os.Should().NotBeNull();
        return os!;
    }

    [Given(@"uma ordem de serviço aberta para um cliente e veículo")]
    public async Task DadoUmaOrdemDeServicoAbertaParaUmClienteEVeiculo()
    {
        _gateway = new FakeOrdemServicoGateway(_bus);

        var cliente = new Cliente();
        cliente.Inserir("João Silva", DocumentoCliente.Criar("12345678909"));

        var veiculo = new Veiculo();
        veiculo.Inserir(cliente.Id, Placa.Criar("ABC1234"), "Toyota", "Corolla", 2020);
        cliente.Veiculos.Add(veiculo);

        var clienteGateway = new Mock<IClienteGateway>();
        clienteGateway.Setup(g => g.BuscarComVeiculos(cliente.Id, It.IsAny<CancellationToken>())).ReturnsAsync(cliente);

        _idCliente = cliente.Id;
        _idVeiculo = veiculo.Id;

        var outputPort = new Mock<IInserirOrdemServicoOutputPort>();
        outputPort.Setup(p => p.Ok(It.IsAny<Guid>())).Callback<Guid>(id => _idOrdemServico = id);

        var useCase = new InserirOrdemServicoUseCase(_gateway, clienteGateway.Object, outputPort.Object);
        await useCase.Execute(new InserirOrdemServicoInput(cliente.Id, veiculo.Id));

        _idOrdemServico.Should().NotBeEmpty();
        (await BuscarOrdemServico()).Status.Should().Be(Domain.OrdensServico.Enums.StatusOrdemServico.Recebida);

        // A abertura da OS já deve ter disparado (via domain event) o comando
        // que notifica o Execução Service a começar o diagnóstico.
        _bus.IniciarDiagnosticoComandos.Should().ContainSingle(c => c.IdOrdemServico == _idOrdemServico);
    }

    [When(@"o diagnóstico é registrado com serviços e produtos")]
    public async Task QuandoODiagnosticoERegistradoComServicosEProdutos()
    {
        // Passo 2 da saga (hoje manual — ver comentário em IniciarDiagnosticoUseCase):
        // o OS Service marca a OS como "em diagnóstico" enquanto aguarda o
        // Execução Service.
        var iniciarOutputPort = new Mock<IIniciarDiagnosticoOutputPort>();
        var iniciarUseCase = new IniciarDiagnosticoUseCase(_gateway, iniciarOutputPort.Object);
        await iniciarUseCase.Execute(new IniciarDiagnosticoInput(_idOrdemServico));
        iniciarOutputPort.Verify(p => p.Ok(), Times.Once);

        // Simula o evento "DiagnosticoFinalizado" publicado pelo Execução Service,
        // consumido pelo consumer MassTransit real (sem broker de verdade).
        var consumer = new DiagnosticoFinalizadoConsumer(_gateway);
        var evento = new DiagnosticoFinalizado(
            _idOrdemServico,
            [new ItemServicoDiagnosticado(Guid.NewGuid(), "Troca de óleo", 120m)],
            [new ItemProdutoDiagnosticado(Guid.NewGuid(), "Óleo 5W30", 79.90m, 2m)]);

        await consumer.Consume(CriarContexto(evento).Object);

        var os = await BuscarOrdemServico();
        os.Status.Should().Be(Domain.OrdensServico.Enums.StatusOrdemServico.AguardandoAprovacao);
        os.ValorTotal.Should().Be(279.80m);

        // O registro do diagnóstico deve ter disparado (via domain event) o
        // comando que pede ao Billing Service pra gerar o orçamento.
        _bus.GerarOrcamentoComandos.Should().ContainSingle(c => c.IdOrdemServico == _idOrdemServico && c.ValorTotal == 279.80m);
    }

    [When(@"o orçamento é gerado e o pagamento é aprovado")]
    public async Task QuandoOOrcamentoEGeradoEOPagamentoEAprovado()
    {
        // "OrcamentoGerado" é só informativo pro Billing Service ligar a
        // cobrança — a OS Service não reage a ele (sem consumer dedicado).
        // O que avança a saga é o evento seguinte, "PagamentoAprovado".
        var consumer = new PagamentoAprovadoConsumer(_gateway);
        var evento = new PagamentoAprovado(_idOrdemServico, Guid.NewGuid());

        await consumer.Consume(CriarContexto(evento).Object);

        var os = await BuscarOrdemServico();
        os.Status.Should().Be(Domain.OrdensServico.Enums.StatusOrdemServico.EmExecucao);

        // A aprovação do pagamento muda o status pra EmExecucao, o que deve
        // ter disparado (via domain event) o comando que manda o Execução
        // Service iniciar a execução dos serviços.
        _bus.IniciarExecucaoComandos.Should().ContainSingle(c => c.IdOrdemServico == _idOrdemServico);
    }

    [When(@"a execução dos serviços é finalizada")]
    public async Task QuandoAExecucaoDosServicosEFinalizada()
    {
        var consumer = new ExecucaoFinalizadaConsumer(_gateway);
        var evento = new ExecucaoFinalizada(_idOrdemServico);

        await consumer.Consume(CriarContexto(evento).Object);
    }

    [When(@"o pagamento é recusado")]
    public async Task QuandoOPagamentoERecusado()
    {
        var consumer = new PagamentoRecusadoConsumer(_gateway);
        var evento = new PagamentoRecusado(_idOrdemServico, Guid.NewGuid(), "Cartão recusado pela operadora");

        await consumer.Consume(CriarContexto(evento).Object);
    }

    [Then(@"a ordem de serviço deve estar com status ""(.*)""")]
    public async Task EntaoAOrdemDeServicoDeveEstarComStatus(string statusEsperado)
    {
        var os = await BuscarOrdemServico();

        os.Status.ToString().Should().Be(statusEsperado);
    }
}
