using Application.Clientes.Controllers;
using Application.Clientes.UseCases.AtualizarCliente;
using Application.Clientes.UseCases.BuscarCliente;
using Application.Clientes.UseCases.BuscarListaPaginada;
using Application.Clientes.UseCases.InserirCliente;
using Application.Clientes.UseCases.RemoverCliente;
using Application.Clientes.Veiculos.Controllers;
using Application.Clientes.Veiculos.UseCases.AtualizarVeiculo;
using Application.Clientes.Veiculos.UseCases.BuscarVeiculo;
using Application.Clientes.Veiculos.UseCases.InserirVeiculo;
using Application.Clientes.Veiculos.UseCases.RemoverVeiculo;
using Application.Login.Controllers;
using Application.Login.UseCases;
using Application.Login.UseCases.DTOs;
using Application.Login.UseCases.Interfaces;
using Application.OrdensServico.Controllers;
using Application.OrdensServico.UseCases.AprovarPagamento;
using Application.OrdensServico.UseCases.BuscarHistorico;
using Application.OrdensServico.UseCases.BuscarListaPaginadaPorDocumento;
using Application.OrdensServico.UseCases.BuscarOrdemServico;
using Application.OrdensServico.UseCases.Cancelar;
using Application.OrdensServico.UseCases.Entregar;
using Application.OrdensServico.UseCases.Finalizar;
using Application.OrdensServico.UseCases.IniciarDiagnostico;
using Application.OrdensServico.UseCases.Inserir;
using Application.OrdensServico.UseCases.RegistrarDiagnostico;
using Application.OrdensServico.UseCases.Remover;
using Application.Produtos.Controllers;
using Application.Produtos.UseCases.AtualizarProduto;
using Application.Produtos.UseCases.BuscarProduto;
using Application.Produtos.UseCases.IncrementarEstoque;
using Application.Produtos.UseCases.InserirProduto;
using Application.Produtos.UseCases.RemoverProduto;
using Application.Servicos.Controllers;
using Application.Servicos.UseCases.AtualizarServico;
using Application.Servicos.UseCases.BuscarServico;
using Application.Servicos.UseCases.InserirServico;
using Application.Servicos.UseCases.RemoverServico;
using Domain.Clientes.Gateways;
using Domain.Clientes.Veiculos.Gateways;
using Domain.OrdensServico.Gateways;
using Domain.Produtos.Gateways;
using Domain.Servicos.Gateways;
using Domain.Usuarios.Gateways;
using Moq;
using SharedKernel.DTOs;

using BuscarListaPaginadaClienteUC = Application.Clientes.UseCases.BuscarListaPaginada.BuscarListaPaginadaClienteUseCase;
using BuscarListaPaginadaClienteInput = Application.Clientes.UseCases.BuscarListaPaginada.BuscarListaPaginadaClienteInput;
using BuscarListaPaginadaVeiculoUC = Application.Clientes.Veiculos.UseCases.BuscarListaPaginada.BuscarListaPaginadaVeiculoUseCase;
using BuscarListaPaginadaVeiculoInput = Application.Clientes.Veiculos.UseCases.BuscarListaPaginada.BuscarListaPaginadaVeiculoInput;
using BuscarListaPaginadaProdutoUC = Application.Produtos.UseCases.BuscarListaPaginada.BuscarListaPaginadaProdutoUseCase;
using BuscarListaPaginadaProdutoInput = Application.Produtos.UseCases.BuscarListaPaginada.BuscarListaPaginadaInput;
using BuscarListaPaginadaServicoUC = Application.Servicos.UseCases.BuscarListaPaginada.BuscarListaPaginadaUseCase;
using BuscarListaPaginadaServicoInput = Application.Servicos.UseCases.BuscarListaPaginada.BuscarListaPaginadaInput;
using BuscarListaPaginadaOrdemServicoUC = Application.OrdensServico.UseCases.BuscarListaPaginada.BuscarListaPaginadaOrdemServicoUseCase;
using BuscarListaPaginadaOrdemServicoInput = Application.OrdensServico.UseCases.BuscarListaPaginada.BuscarListaPaginadaOrdemServicoInput;
using IBuscarListaPaginadaVeiculoOutputPort = Application.Clientes.Veiculos.UseCases.BuscarListaPaginada.IBuscarListaPaginadaVeiculoOutputPort;
using IBuscarListaPaginadaProdutoOutputPort = Application.Produtos.UseCases.BuscarListaPaginada.IBuscarListaPaginadaProdutoOutputPort;
using IBuscarListaPaginadaOrdemServicoOutputPort = Application.OrdensServico.UseCases.BuscarListaPaginada.IBuscarListaPaginadaOrdemServicoOutputPort;

namespace Tests.Common.Unit;

// As Controllers do Application layer são meras fachadas que encaminham para o
// use case correspondente (uma linha por método) — cobertas aqui simplesmente
// invocando cada método com dependências mockadas "leves" o bastante pra não
// lançar exceção, sem duplicar a cobertura de regra de negócio já feita nos
// testes de use case dedicados.
public class ApplicationControllersTests
{
    [Fact]
    public async Task ClienteController_EncaminhaTodosOsMetodosParaOsUseCases()
    {
        var gateway = new Mock<IClienteGateway>();
        gateway.Setup(g => g.BuscarPaginado(It.IsAny<PagedRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Domain.Clientes.Cliente>(), 0));

        var controller = new ClienteController(
            new BuscarClienteUseCase(gateway.Object, Mock.Of<IBuscarClienteOutputPort>()),
            new BuscarListaPaginadaClienteUC(gateway.Object, Mock.Of<IBuscarListaPaginadaClienteOutputPort>()),
            new InserirClienteUseCase(gateway.Object, Mock.Of<IInserirClienteOutputPort>()),
            new AtualizarClienteUseCase(gateway.Object, Mock.Of<IAtualizarClienteOutputPort>()),
            new RemoverClienteUseCase(gateway.Object, Mock.Of<IRemoverClienteOutputPort>()));

        await controller.Buscar(new BuscarClienteInput(Guid.NewGuid()));
        await controller.BuscarListaPaginada(new BuscarListaPaginadaClienteInput(new PagedRequest(1, 10)));
        await controller.Inserir(new InserirClienteInput("João Silva", "12345678909"));
        await controller.Atualizar(new AtualizarClienteInput(Guid.NewGuid(), "Novo Nome", true));
        await controller.Remover(new RemoverClienteInput(Guid.NewGuid()));
    }

    [Fact]
    public async Task VeiculoController_EncaminhaTodosOsMetodosParaOsUseCases()
    {
        var gateway = new Mock<IVeiculoGateway>();
        gateway.Setup(g => g.BuscarPaginadoPorCliente(It.IsAny<Guid>(), It.IsAny<PagedRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Domain.Clientes.Veiculos.Veiculo>(), 0));

        var controller = new VeiculoController(
            new BuscarVeiculoUseCase(gateway.Object, Mock.Of<IBuscarVeiculoOutputPort>()),
            new BuscarListaPaginadaVeiculoUC(gateway.Object, Mock.Of<IBuscarListaPaginadaVeiculoOutputPort>()),
            new InserirVeiculoUseCase(gateway.Object, Mock.Of<IClienteGateway>(), Mock.Of<IInserirVeiculoOutputPort>()),
            new AtualizarVeiculoUseCase(gateway.Object, Mock.Of<IAtualizarVeiculoOutputPort>()),
            new RemoverVeiculoUseCase(gateway.Object, Mock.Of<IRemoverVeiculoOutputPort>()));

        await controller.Buscar(new BuscarVeiculoInput(Guid.NewGuid()));
        await controller.BuscarListaPaginada(new BuscarListaPaginadaVeiculoInput(Guid.NewGuid(), new PagedRequest(1, 10)));
        await controller.Inserir(new InserirVeiculoInput(Guid.NewGuid(), "ABC1234", "Toyota", "Corolla", 2020));
        await controller.Atualizar(new AtualizarVeiculoInput(Guid.NewGuid(), "ABC1234", "Toyota", "Corolla", 2020));
        await controller.Remover(new RemoverVeiculoInput(Guid.NewGuid()));
    }

    [Fact]
    public async Task ProdutoController_EncaminhaTodosOsMetodosParaOsUseCases()
    {
        var gateway = new Mock<IProdutoGateway>();
        gateway.Setup(g => g.BuscarPaginado(null, It.IsAny<PagedRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Domain.Produtos.Produto>(), 0));

        var controller = new ProdutoController(
            new BuscarProdutoUseCase(gateway.Object, Mock.Of<IBuscarProdutoOutputPort>()),
            new BuscarListaPaginadaProdutoUC(gateway.Object, Mock.Of<IBuscarListaPaginadaProdutoOutputPort>()),
            new InserirProdutoUseCase(gateway.Object, Mock.Of<IInserirProdutoOutputPort>()),
            new AtualizarProdutoUseCase(gateway.Object, Mock.Of<IAtualizarProdutoOutputPort>()),
            new IncrementarEstoqueUseCase(gateway.Object, Mock.Of<IIncrementarEstoqueOutputPort>()),
            new RemoverProdutoUseCase(gateway.Object, Mock.Of<IRemoverProdutoOutputPort>()));

        await controller.Buscar(new BuscarProdutoInput(Guid.NewGuid()));
        await controller.BuscarListaPaginada(new BuscarListaPaginadaProdutoInput(new PagedRequest(1, 10)));
        await controller.Inserir(new InserirProdutoInput("Óleo", "Óleo sintético", 79.90m, 10));
        await controller.Atualizar(new AtualizarProdutoInput(Guid.NewGuid(), "Óleo", "Óleo sintético", 79.90m));
        await controller.IncrementarEstoque(new IncrementarEstoqueInput(Guid.NewGuid(), 5m));
        await controller.Remover(new RemoverProdutoInput(Guid.NewGuid()));
    }

    [Fact]
    public async Task ServicoController_EncaminhaTodosOsMetodosParaOsUseCases()
    {
        var gateway = new Mock<IServicoGateway>();
        gateway.Setup(g => g.BuscarPaginado(It.IsAny<PagedRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Domain.Servicos.Servico>(), 0));

        var controller = new ServicoController(
            new BuscarServicoUseCase(gateway.Object, Mock.Of<IBuscarServicoOutputPort>()),
            new BuscarListaPaginadaServicoUC(gateway.Object, Mock.Of<Application.Servicos.UseCases.BuscarListaPaginada.IBuscarListaPaginadaOutputPort>()),
            new InserirServicoUseCase(gateway.Object, Mock.Of<IInserirServicoOutputPort>()),
            new AtualizarServicoUseCase(gateway.Object, Mock.Of<IAtualizarServicoOutputPort>()),
            new RemoverServicoUseCase(gateway.Object, Mock.Of<IRemoverServicoOutputPort>()));

        await controller.Buscar(new BuscarServicoInput(Guid.NewGuid()));
        await controller.BuscarListaPaginada(new BuscarListaPaginadaServicoInput(new PagedRequest(1, 10)));
        await controller.Inserir(new InserirServicoInput("Troca de óleo", "Substituição do óleo", 120m));
        await controller.Atualizar(new AtualizarServicoInput(Guid.NewGuid(), "Troca de óleo", "Substituição do óleo", 120m));
        await controller.Remover(new RemoverServicoInput(Guid.NewGuid()));
    }

    [Fact]
    public async Task OrdemServicoController_EncaminhaTodosOsMetodosParaOsUseCases()
    {
        var gateway = new Mock<IOrdemServicoGateway>();
        gateway.Setup(g => g.BuscarPaginado(It.IsAny<PagedRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Domain.OrdensServico.OrdemServico>(), 0));

        var controller = new OrdemServicoController(
            new BuscarOrdemServicoUseCase(gateway.Object, Mock.Of<IClienteGateway>(), Mock.Of<IVeiculoGateway>(), Mock.Of<IBuscarOrdemServicoOutputPort>()),
            new BuscarListaPaginadaOrdemServicoUC(gateway.Object, Mock.Of<IClienteGateway>(), Mock.Of<IVeiculoGateway>(), Mock.Of<IBuscarListaPaginadaOrdemServicoOutputPort>()),
            new BuscarListaPaginadaPorDocumentoUseCase(gateway.Object, Mock.Of<IClienteGateway>(), Mock.Of<IVeiculoGateway>(), Mock.Of<IBuscarListaPaginadaPorDocumentoOutputPort>()),
            new BuscarHistoricoUseCase(gateway.Object, Mock.Of<Domain.OrdensServico.Historico.Gateways.IHistoricoStatusOrdemServicoGateway>(), Mock.Of<IBuscarHistoricoOutputPort>()),
            new InserirOrdemServicoUseCase(gateway.Object, Mock.Of<IClienteGateway>(), Mock.Of<IInserirOrdemServicoOutputPort>()),
            new IniciarDiagnosticoUseCase(gateway.Object, Mock.Of<IIniciarDiagnosticoOutputPort>()),
            new RegistrarDiagnosticoUseCase(gateway.Object, Mock.Of<IRegistrarDiagnosticoOutputPort>()),
            new AprovarPagamentoUseCase(gateway.Object, Mock.Of<IAprovarPagamentoOutputPort>()),
            new CancelarUseCase(gateway.Object, Mock.Of<ICancelarOutputPort>()),
            new FinalizarUseCase(gateway.Object, Mock.Of<IFinalizarOutputPort>()),
            new EntregarUseCase(gateway.Object, Mock.Of<IEntregarOutputPort>()),
            new RemoverOrdemServicoUseCase(gateway.Object, Mock.Of<IRemoverOrdemServicoOutputPort>()));

        await controller.Buscar(new BuscarOrdemServicoInput(Guid.NewGuid()));
        await controller.BuscarListaPaginada(new BuscarListaPaginadaOrdemServicoInput(new PagedRequest(1, 10)));
        // Documento diferente do caller: cai no early-return sem tocar o gateway.
        await controller.BuscarListaPaginadaPorDocumento(new BuscarListaPaginadaPorDocumentoInput("12345678909", new PagedRequest(1, 10), "99988877766"));
        await controller.BuscarHistorico(new BuscarHistoricoInput(Guid.NewGuid()));
        await controller.Inserir(new InserirOrdemServicoInput(Guid.NewGuid(), Guid.NewGuid()));
        await controller.IniciarDiagnostico(new IniciarDiagnosticoInput(Guid.NewGuid()));
        await controller.RegistrarDiagnostico(new RegistrarDiagnosticoInput(Guid.NewGuid(), [], []));
        await controller.AprovarPagamento(new AprovarPagamentoInput(Guid.NewGuid()));
        await controller.Cancelar(new CancelarInput(Guid.NewGuid()));
        await controller.Finalizar(new FinalizarInput(Guid.NewGuid()));
        await controller.Entregar(new EntregarInput(Guid.NewGuid()));
        await controller.Remover(new RemoverOrdemServicoInput(Guid.NewGuid()));
    }

    [Fact]
    public async Task LoginController_EncaminhaParaLoginUseCase()
    {
        var gateway = new Mock<IUsuarioGateway>();
        var useCase = new LoginUseCase(gateway.Object, Mock.Of<IPasswordHasher>(), Mock.Of<ITokenProvider>(), Mock.Of<ILoginOutputPort>());
        var controller = new LoginController(useCase);

        await controller.Execute(new LoginInput("admin@teste.com", "123456"));

        gateway.Verify(g => g.BuscarPorEmail("admin@teste.com", It.IsAny<CancellationToken>()), Times.Once);
    }
}
