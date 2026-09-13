using Domain.Clientes.Enums;
using Domain.Clientes.ValueObjects;
using Domain.Clientes.Veiculos;
using Domain.Common;
using DomainException = Domain.Common.Exceptions.DomainException;

namespace Domain.Clientes;

public class Cliente : Entity
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; } = null!;
    public string Documento { get; private set; } = null!;
    public TipoDocumentoCliente TipoDocumento { get; private set; }
    public bool Ativo { get; private set; }
    public DateTime DataCriacao { get; private set; }
    public List<Veiculo> Veiculos { get; init; } = new();

    public void Inserir(string nome, DocumentoCliente documento)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("O nome é obrigatório.");

        Id = Guid.NewGuid();
        Nome = nome;
        Documento = documento.Numero;
        TipoDocumento = documento.Tipo;
        Ativo = true;
        DataCriacao = DateTime.UtcNow;
    }

    public void Atualizar(string nome)
    {
        Nome = nome;
    }

    public void Ativar()
    {
        Ativo = true;
    }

    public void Inativar()
    {
        Ativo = false;
    }
}
