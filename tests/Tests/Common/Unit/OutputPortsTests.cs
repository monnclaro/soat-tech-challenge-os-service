using Application.OrdensServico.UseCases.AprovarPagamento;
using Application.OrdensServico.UseCases.Finalizar;
using Application.OrdensServico.UseCases.RegistrarDiagnostico;
using Application.OrdensServico.UseCases.RegistrarLinkPagamento;
using Application.Produtos.UseCases.DecrementarEstoque;
using FluentAssertions;

namespace Tests.Common.Unit;

// Implementações "sem apresentação" dos output ports usados só por consumers/domain
// event handlers (nunca por um controller HTTP, por isso não têm Presenter na Api) —
// registradas manualmente em Application.DependencyInjection. Cobertas aqui só
// chamando cada método (não há branching nem lógica, são no-ops por design).
public class OutputPortsTests
{
    [Fact]
    public void DecrementarEstoqueOutputPort_Ok_NaoLancaExcecao()
    {
        var outputPort = new DecrementarEstoqueOutputPort();

        var acao = () => outputPort.Ok();

        acao.Should().NotThrow();
    }

    [Fact]
    public void RegistrarLinkPagamentoOutputPort_MapeiaTodosOsCasos()
    {
        var outputPort = new RegistrarLinkPagamentoOutputPort();

        var acao = () =>
        {
            outputPort.NaoEncontrado();
            outputPort.Ok();
        };

        acao.Should().NotThrow();
    }

    [Fact]
    public void RegistrarDiagnosticoOutputPort_MapeiaTodosOsCasos()
    {
        var outputPort = new RegistrarDiagnosticoOutputPort();

        var acao = () =>
        {
            outputPort.NaoEncontrado();
            outputPort.Ok();
        };

        acao.Should().NotThrow();
    }

    [Fact]
    public void AprovarPagamentoOutputPort_MapeiaTodosOsCasos()
    {
        var outputPort = new AprovarPagamentoOutputPort();

        var acao = () =>
        {
            outputPort.NaoEncontrado();
            outputPort.Ok();
        };

        acao.Should().NotThrow();
    }

    [Fact]
    public void FinalizarOutputPort_MapeiaTodosOsCasos()
    {
        var outputPort = new FinalizarOutputPort();

        var acao = () =>
        {
            outputPort.NaoEncontrado();
            outputPort.Ok();
        };

        acao.Should().NotThrow();
    }
}
