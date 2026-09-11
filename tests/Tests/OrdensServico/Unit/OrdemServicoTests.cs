using Domain.Common.Exceptions;
using Domain.OrdensServico;
using Domain.OrdensServico.Enums;
using Domain.OrdensServico.Produtos;
using Domain.OrdensServico.Servicos;
using FluentAssertions;

namespace Tests.OrdensServico.Unit;

// Cobre o agregado OrdemServico após o redesenho da Fase 4: o diagnóstico e a
// execução item a item saíram daqui (agora vivem no Execução Service) e a
// máquina de estados passou a refletir os passos da saga — ver
// PLANO-FASE-4-MICROSSERVICOS.md.
public class OrdemServicoTests
{
    private static OrdemServico CriarOrdemServicoRecebida()
    {
        var os = new OrdemServico();
        os.Inserir(Guid.NewGuid(), Guid.NewGuid());
        return os;
    }

    private static OrdemServico CriarOrdemServicoEmDiagnostico()
    {
        var os = CriarOrdemServicoRecebida();
        os.IniciarDiagnostico();
        return os;
    }

    private static (List<OrdemServicoServico> Servicos, List<OrdemServicoProduto> Produtos) CriarItensDiagnostico(Guid idOrdemServico) =>
        (
            [new OrdemServicoServico(idOrdemServico, Guid.NewGuid(), "Troca de óleo", 120m)],
            [new OrdemServicoProduto(idOrdemServico, Guid.NewGuid(), "Óleo 5W30", 79.90m, 2)]
        );

    private static OrdemServico CriarOrdemServicoAguardandoAprovacao()
    {
        var os = CriarOrdemServicoEmDiagnostico();
        var (servicos, produtos) = CriarItensDiagnostico(os.Id);
        os.RegistrarDiagnostico(servicos, produtos);
        return os;
    }

    private static OrdemServico CriarOrdemServicoEmExecucao()
    {
        var os = CriarOrdemServicoAguardandoAprovacao();
        os.AprovarPagamento();
        return os;
    }

    [Fact]
    public void Inserir_DefineStatusRecebidaEDataCriacao()
    {
        var idCliente = Guid.NewGuid();
        var idVeiculo = Guid.NewGuid();

        var os = new OrdemServico();
        os.Inserir(idCliente, idVeiculo);

        os.Status.Should().Be(StatusOrdemServico.Recebida);
        os.IdCliente.Should().Be(idCliente);
        os.IdVeiculo.Should().Be(idVeiculo);
        os.DomainEvents.Should().ContainSingle(e => e is Domain.OrdensServico.Events.OrdemServicoStatusAlteradoDomainEvent);
    }

    [Fact]
    public void IniciarDiagnostico_QuandoRecebida_MudaParaEmDiagnostico()
    {
        var os = CriarOrdemServicoRecebida();

        os.IniciarDiagnostico();

        os.Status.Should().Be(StatusOrdemServico.EmDiagnostico);
    }

    [Fact]
    public void IniciarDiagnostico_QuandoNaoRecebida_LancaExcecao()
    {
        var os = CriarOrdemServicoEmDiagnostico();

        var act = () => os.IniciarDiagnostico();

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void RegistrarDiagnostico_QuandoEmDiagnostico_CalculaValorTotalEAvancaParaAguardandoAprovacao()
    {
        var os = CriarOrdemServicoEmDiagnostico();
        var (servicos, produtos) = CriarItensDiagnostico(os.Id);

        os.RegistrarDiagnostico(servicos, produtos);

        os.Status.Should().Be(StatusOrdemServico.AguardandoAprovacao);
        os.Servicos.Should().HaveCount(1);
        os.Produtos.Should().HaveCount(1);
        os.ValorTotal.Should().Be(120m + 79.90m * 2);
    }

    [Fact]
    public void RegistrarDiagnostico_SemServicos_LancaExcecao()
    {
        var os = CriarOrdemServicoEmDiagnostico();

        var act = () => os.RegistrarDiagnostico([], []);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void RegistrarDiagnostico_QuandoNaoEmDiagnostico_LancaExcecao()
    {
        var os = CriarOrdemServicoRecebida();
        var (servicos, produtos) = CriarItensDiagnostico(os.Id);

        var act = () => os.RegistrarDiagnostico(servicos, produtos);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void AprovarPagamento_QuandoAguardandoAprovacao_IniciaExecucao()
    {
        var os = CriarOrdemServicoAguardandoAprovacao();

        os.AprovarPagamento();

        os.Status.Should().Be(StatusOrdemServico.EmExecucao);
        os.DataInicioExecucao.Should().NotBeNull();
    }

    [Fact]
    public void AprovarPagamento_QuandoNaoAguardandoAprovacao_LancaExcecao()
    {
        var os = CriarOrdemServicoRecebida();

        var act = () => os.AprovarPagamento();

        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(StatusOrdemServico.Recebida)]
    [InlineData(StatusOrdemServico.EmDiagnostico)]
    [InlineData(StatusOrdemServico.AguardandoAprovacao)]
    [InlineData(StatusOrdemServico.EmExecucao)]
    public void Cancelar_QuandoNaoFinalizadaNemEntregue_MudaParaCancelada(StatusOrdemServico statusInicial)
    {
        OrdemServico os = statusInicial switch
        {
            StatusOrdemServico.Recebida => CriarOrdemServicoRecebida(),
            StatusOrdemServico.EmDiagnostico => CriarOrdemServicoEmDiagnostico(),
            StatusOrdemServico.AguardandoAprovacao => CriarOrdemServicoAguardandoAprovacao(),
            StatusOrdemServico.EmExecucao => CriarOrdemServicoEmExecucao(),
            _ => throw new ArgumentOutOfRangeException(nameof(statusInicial))
        };

        os.Cancelar();

        os.Status.Should().Be(StatusOrdemServico.Cancelada);
    }

    [Fact]
    public void Cancelar_QuandoJaCancelada_LancaExcecao()
    {
        var os = CriarOrdemServicoRecebida();
        os.Cancelar();

        var act = () => os.Cancelar();

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Finalizar_QuandoEmExecucao_MudaParaFinalizadaERaiseEventoDeProdutos()
    {
        var os = CriarOrdemServicoEmExecucao();

        os.Finalizar();

        os.Status.Should().Be(StatusOrdemServico.Finalizada);
        os.DataFinalizacao.Should().NotBeNull();
        os.DomainEvents.Should().Contain(e => e is Domain.OrdensServico.Events.OrdemServicoFinalizadaDomainEvent);
    }

    [Fact]
    public void Finalizar_QuandoNaoEmExecucao_LancaExcecao()
    {
        var os = CriarOrdemServicoRecebida();

        var act = () => os.Finalizar();

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Entregar_QuandoFinalizada_MudaParaEntregue()
    {
        var os = CriarOrdemServicoEmExecucao();
        os.Finalizar();

        os.Entregar();

        os.Status.Should().Be(StatusOrdemServico.Entregue);
    }

    [Fact]
    public void Entregar_QuandoNaoFinalizada_LancaExcecao()
    {
        var os = CriarOrdemServicoRecebida();

        var act = () => os.Entregar();

        act.Should().Throw<DomainException>();
    }
}
