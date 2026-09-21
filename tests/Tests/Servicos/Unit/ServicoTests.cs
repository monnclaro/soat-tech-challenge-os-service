using Domain.Common.Exceptions;
using Domain.Servicos;
using FluentAssertions;

namespace Tests.Servicos.Unit;

public class ServicoTests
{
    [Fact]
    public void Inserir_ComDadosValidos_CriaServico()
    {
        var servico = new Servico();
        servico.Inserir("Troca de óleo", "Substituição do óleo do motor", 120);

        servico.Nome.Should().Be("Troca de óleo");
        servico.Valor.Should().Be(120);
    }

    [Fact]
    public void Inserir_ComNomeVazio_LancaExcecao()
    {
        var servico = new Servico();

        var act = () => servico.Inserir("", "desc", 100);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Inserir_ComValorNegativo_LancaExcecao()
    {
        var servico = new Servico();

        var act = () => servico.Inserir("Troca de óleo", "desc", -10);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Atualizar_AlteraNomeDescricaoEValor()
    {
        var servico = new Servico();
        servico.Inserir("Troca de óleo", "desc", 100);

        servico.Atualizar("Alinhamento", "nova desc", 90);

        servico.Nome.Should().Be("Alinhamento");
        servico.Valor.Should().Be(90);
    }
}
