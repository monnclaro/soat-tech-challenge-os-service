using Domain.Common;
using DomainException = Domain.Common.Exceptions.DomainException;

namespace Domain.Servicos;

public class Servico : Entity
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; } = null!;
    public string Descricao { get; private set; } = null!;
    public decimal Valor { get; private set; }

    public void Inserir(string nome, string descricao, decimal valor)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("O nome do serviço é obrigatório.");

        if (valor < 0)
            throw new DomainException("O valor deve ser um número positivo.");

        Id = Guid.NewGuid();
        Nome = nome;
        Descricao = descricao;
        Valor = valor;
    }

    public void Atualizar(string nome, string descricao, decimal valor)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("O nome do serviço é obrigatório.");

        if (valor < 0)
            throw new DomainException("O valor deve ser um número positivo.");

        Nome = nome;
        Descricao = descricao;
        Valor = valor;
    }
}
