using Domain.Clientes.Veiculos.ValueObjects;
using Domain.Common;
using DomainException = Domain.Common.Exceptions.DomainException;

namespace Domain.Clientes.Veiculos;

public class Veiculo : Entity
{
    public Guid Id { get; private set; }
    public Guid IdCliente { get; private set; }
    public string Placa { get; private set; } = null!;
    public string Marca { get; private set; } = null!;
    public string Modelo { get; private set; } = null!;
    public int Ano { get; private set; }
    public DateTime DataCriacao { get; private set; }

    public void Inserir(Guid idCliente, Placa placa, string marca, string modelo, int ano)
    {
        if (string.IsNullOrWhiteSpace(marca))
            throw new DomainException("A marca do veículo é obrigatória.");

        if (string.IsNullOrWhiteSpace(modelo))
            throw new DomainException("O modelo do veículo é obrigatório.");

        if (string.IsNullOrWhiteSpace(placa.Valor))
            throw new DomainException("A placa do veículo é obrigatória.");

        var anoAtual = DateTime.Now.Year;
        if (ano < 1886 || ano > anoAtual + 1)
        {
            throw new DomainException($"Ano do veículo inválido. O ano deve estar entre 1886 e {anoAtual + 1}.");
        }

        Id = Guid.NewGuid();
        IdCliente = idCliente;
        Placa = placa.Valor;
        Marca = marca;
        Modelo = modelo;
        Ano = ano;
        DataCriacao = DateTime.UtcNow;
    }

    public void Atualizar(Placa placa, string marca, string modelo, int ano)
    {
        if (string.IsNullOrWhiteSpace(marca))
            throw new DomainException("A marca do veículo é obrigatória.");

        if (string.IsNullOrWhiteSpace(modelo))
            throw new DomainException("O modelo do veículo é obrigatória.");

        if (string.IsNullOrWhiteSpace(placa.Valor))
            throw new DomainException("A placa do veículo é obrigatória.");

        var anoAtual = DateTime.Now.Year;
        if (ano < 1886 || ano > anoAtual + 1)
        {
            throw new DomainException($"Ano do veículo inválido. Informe um ano entre 1886 e {anoAtual + 1}.");
        }

        Placa = placa.Valor;
        Marca = marca;
        Modelo = modelo;
        Ano = ano;
    }
}
