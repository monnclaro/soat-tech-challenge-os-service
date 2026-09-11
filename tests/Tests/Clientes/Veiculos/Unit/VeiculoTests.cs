using Domain.Clientes.Veiculos;
using Domain.Clientes.Veiculos.ValueObjects;
using Domain.Common.Exceptions;
using FluentAssertions;

namespace Tests.Clientes.Veiculos.Unit;

public class VeiculoTests
{
    [Fact]
    public void Inserir_ComDadosValidos_CriaVeiculo()
    {
        var veiculo = new Veiculo();
        veiculo.Inserir(Guid.NewGuid(), Placa.Criar("ABC1234"), "Toyota", "Corolla", 2020);

        veiculo.Placa.Should().Be("ABC1234");
        veiculo.Marca.Should().Be("Toyota");
        veiculo.Ano.Should().Be(2020);
    }

    [Theory]
    [InlineData(1885)]
    public void Inserir_ComAnoAnteriorA1886_LancaExcecao(int ano)
    {
        var veiculo = new Veiculo();

        var act = () => veiculo.Inserir(Guid.NewGuid(), Placa.Criar("ABC1234"), "Toyota", "Corolla", ano);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Inserir_ComAnoMuitoFuturo_LancaExcecao()
    {
        var veiculo = new Veiculo();
        var anoInvalido = DateTime.Now.Year + 2;

        var act = () => veiculo.Inserir(Guid.NewGuid(), Placa.Criar("ABC1234"), "Toyota", "Corolla", anoInvalido);

        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("ABC1234")]
    [InlineData("ABC1D23")]
    public void Placa_Criar_AceitaFormatoAntigoEMercosul(string placa)
    {
        var act = () => Placa.Criar(placa);

        act.Should().NotThrow();
    }

    [Fact]
    public void Placa_Criar_ComFormatoInvalido_LancaExcecao()
    {
        var act = () => Placa.Criar("123ABC");

        act.Should().Throw<DomainException>();
    }
}
