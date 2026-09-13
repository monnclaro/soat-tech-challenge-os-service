using Application.Produtos.DTOs;
using Application.Produtos.UseCases.AtualizarProduto;
using Application.Produtos.UseCases.BuscarListaPaginada;
using Application.Produtos.UseCases.BuscarProduto;
using Application.Produtos.UseCases.DecrementarEstoque;
using Application.Produtos.UseCases.IncrementarEstoque;
using Application.Produtos.UseCases.InserirProduto;
using Application.Produtos.UseCases.RemoverProduto;
using Domain.Produtos;
using Domain.Produtos.Gateways;
using FluentAssertions;
using Moq;
using SharedKernel.DTOs;

namespace Tests.Produtos.Unit;

public class ProdutoUseCasesTests
{
    private static Produto CriarProduto(string nome = "Óleo Motor 5W30", decimal estoque = 10)
    {
        var produto = new Produto();
        produto.Inserir(nome, "Óleo sintético", 79.90m, estoque);
        return produto;
    }

    [Fact]
    public async Task Inserir_SalvaEChamaOk()
    {
        var gateway = new Mock<IProdutoGateway>();
        var outputPort = new Mock<IInserirProdutoOutputPort>();
        var useCase = new InserirProdutoUseCase(gateway.Object, outputPort.Object);

        await useCase.Execute(new InserirProdutoInput("Óleo Motor 5W30", "Óleo sintético", 79.90m, 10));

        gateway.Verify(g => g.Salvar(It.IsAny<Produto>(), It.IsAny<CancellationToken>()), Times.Once);
        outputPort.Verify(p => p.Ok(It.Is<ProdutoOutput>(o => o.Nome == "Óleo Motor 5W30")), Times.Once);
    }

    [Fact]
    public async Task AtualizarProduto_QuandoNaoEncontrado_ChamaNaoEncontrado()
    {
        var gateway = new Mock<IProdutoGateway>();
        gateway.Setup(g => g.BuscarPorId(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Produto?)null);

        var outputPort = new Mock<IAtualizarProdutoOutputPort>();
        var useCase = new AtualizarProdutoUseCase(gateway.Object, outputPort.Object);

        await useCase.Execute(new AtualizarProdutoInput(Guid.NewGuid(), "Novo Nome", "Nova Descrição", 100m));

        outputPort.Verify(p => p.NaoEncontrado(), Times.Once);
    }

    [Fact]
    public async Task AtualizarProduto_QuandoEncontrado_AtualizaEChamaOk()
    {
        var produto = CriarProduto();
        var gateway = new Mock<IProdutoGateway>();
        gateway.Setup(g => g.BuscarPorId(produto.Id, It.IsAny<CancellationToken>())).ReturnsAsync(produto);

        var outputPort = new Mock<IAtualizarProdutoOutputPort>();
        var useCase = new AtualizarProdutoUseCase(gateway.Object, outputPort.Object);

        await useCase.Execute(new AtualizarProdutoInput(produto.Id, "Novo Nome", "Nova Descrição", 100m));

        produto.Nome.Should().Be("Novo Nome");
        gateway.Verify(g => g.Atualizar(produto, It.IsAny<CancellationToken>()), Times.Once);
        outputPort.Verify(p => p.Ok(It.IsAny<ProdutoOutput>()), Times.Once);
    }

    [Fact]
    public async Task BuscarProduto_QuandoNaoEncontrado_ChamaNaoEncontrado()
    {
        var gateway = new Mock<IProdutoGateway>();
        gateway.Setup(g => g.BuscarPorId(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Produto?)null);

        var outputPort = new Mock<IBuscarProdutoOutputPort>();
        var useCase = new BuscarProdutoUseCase(gateway.Object, outputPort.Object);

        await useCase.Execute(new BuscarProdutoInput(Guid.NewGuid()));

        outputPort.Verify(p => p.NaoEncontrado(), Times.Once);
    }

    [Fact]
    public async Task BuscarProduto_QuandoEncontrado_ChamaOk()
    {
        var produto = CriarProduto();
        var gateway = new Mock<IProdutoGateway>();
        gateway.Setup(g => g.BuscarPorId(produto.Id, It.IsAny<CancellationToken>())).ReturnsAsync(produto);

        var outputPort = new Mock<IBuscarProdutoOutputPort>();
        var useCase = new BuscarProdutoUseCase(gateway.Object, outputPort.Object);

        await useCase.Execute(new BuscarProdutoInput(produto.Id));

        outputPort.Verify(p => p.Ok(It.Is<ProdutoOutput>(o => o.Id == produto.Id)), Times.Once);
    }

    [Fact]
    public async Task BuscarListaPaginada_ChamaOkComResultadoPaginado()
    {
        var produto = CriarProduto();
        var gateway = new Mock<IProdutoGateway>();
        gateway.Setup(g => g.BuscarPaginado(null, It.IsAny<PagedRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Produto> { produto }, 1));

        var outputPort = new Mock<IBuscarListaPaginadaProdutoOutputPort>();
        var useCase = new BuscarListaPaginadaProdutoUseCase(gateway.Object, outputPort.Object);

        await useCase.Execute(new BuscarListaPaginadaInput(new PagedRequest(1, 10)));

        outputPort.Verify(p => p.Ok(It.Is<PagedResult<ProdutoOutput>>(r => r.TotalCount == 1 && r.Items.Count == 1)), Times.Once);
    }

    [Fact]
    public async Task IncrementarEstoque_QuandoNaoEncontrado_ChamaNaoEncontrado()
    {
        var gateway = new Mock<IProdutoGateway>();
        gateway.Setup(g => g.BuscarPorId(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Produto?)null);

        var outputPort = new Mock<IIncrementarEstoqueOutputPort>();
        var useCase = new IncrementarEstoqueUseCase(gateway.Object, outputPort.Object);

        await useCase.Execute(new IncrementarEstoqueInput(Guid.NewGuid(), 5m));

        outputPort.Verify(p => p.NaoEncontrado(), Times.Once);
    }

    [Fact]
    public async Task IncrementarEstoque_QuandoEncontrado_IncrementaEChamaOk()
    {
        var produto = CriarProduto(estoque: 10);
        var gateway = new Mock<IProdutoGateway>();
        gateway.Setup(g => g.BuscarPorId(produto.Id, It.IsAny<CancellationToken>())).ReturnsAsync(produto);

        var outputPort = new Mock<IIncrementarEstoqueOutputPort>();
        var useCase = new IncrementarEstoqueUseCase(gateway.Object, outputPort.Object);

        await useCase.Execute(new IncrementarEstoqueInput(produto.Id, 5m));

        produto.QuantidadeEmEstoque.Should().Be(15m);
        gateway.Verify(g => g.Atualizar(produto, It.IsAny<CancellationToken>()), Times.Once);
        outputPort.Verify(p => p.Ok(It.IsAny<ProdutoOutput>()), Times.Once);
    }

    [Fact]
    public async Task DecrementarEstoque_DecrementaTodosOsItensEmLoteEChamaOk()
    {
        var produto1 = CriarProduto(estoque: 10);
        var produto2 = CriarProduto("Filtro de Óleo", 20);

        var gateway = new Mock<IProdutoGateway>();
        gateway.Setup(g => g.BuscarPorIds(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyList<Produto>)[produto1, produto2]);

        var outputPort = new Mock<IDecrementarEstoqueOutputPort>();
        var useCase = new DecrementarEstoqueUseCase(gateway.Object, outputPort.Object);

        var input = new DecrementarEstoqueInput([
            new DecrementarEstoqueItem(produto1.Id, 3m),
            new DecrementarEstoqueItem(produto2.Id, 5m)
        ]);

        await useCase.Execute(input);

        produto1.QuantidadeEmEstoque.Should().Be(7m);
        produto2.QuantidadeEmEstoque.Should().Be(15m);
        gateway.Verify(g => g.AtualizarLote(It.IsAny<IReadOnlyList<Produto>>(), It.IsAny<CancellationToken>()), Times.Once);
        outputPort.Verify(p => p.Ok(), Times.Once);
    }

    [Fact]
    public async Task RemoverProduto_QuandoNaoEncontrado_ChamaOkIdempotente()
    {
        var gateway = new Mock<IProdutoGateway>();
        gateway.Setup(g => g.BuscarPorId(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Produto?)null);

        var outputPort = new Mock<IRemoverProdutoOutputPort>();
        var useCase = new RemoverProdutoUseCase(gateway.Object, outputPort.Object);

        await useCase.Execute(new RemoverProdutoInput(Guid.NewGuid()));

        outputPort.Verify(p => p.Ok(), Times.Once);
        gateway.Verify(g => g.Remover(It.IsAny<Produto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RemoverProduto_QuandoEncontrado_RemoveEChamaOk()
    {
        var produto = CriarProduto();
        var gateway = new Mock<IProdutoGateway>();
        gateway.Setup(g => g.BuscarPorId(produto.Id, It.IsAny<CancellationToken>())).ReturnsAsync(produto);

        var outputPort = new Mock<IRemoverProdutoOutputPort>();
        var useCase = new RemoverProdutoUseCase(gateway.Object, outputPort.Object);

        await useCase.Execute(new RemoverProdutoInput(produto.Id));

        gateway.Verify(g => g.Remover(produto, It.IsAny<CancellationToken>()), Times.Once);
        outputPort.Verify(p => p.Ok(), Times.Once);
    }
}
