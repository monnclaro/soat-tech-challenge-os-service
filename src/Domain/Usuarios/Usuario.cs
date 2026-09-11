using Domain.Common;
using Domain.Common.ValueObjects;
using Domain.Usuarios.Roles;
using DomainException = Domain.Common.Exceptions.DomainException;

namespace Domain.Usuarios;

public class Usuario : Entity
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; }
    public string Email { get; private set; }
    public string SenhaHash { get; private set; }
    public string Cpf { get; private set; }
    public bool Ativo { get; private set; }
    public DateTime DataCriacao { get; private set; }
    public List<UsuarioRole> Roles { get; private init; } = new();

    public Usuario(string nome, string email, string senhaHash, string cpf)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("O nome é obrigatório");

        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("O email é obrigatório");

        if (string.IsNullOrWhiteSpace(senhaHash))
            throw new DomainException("A senha é obrigatória");

        var cpfDigitos = new string((cpf ?? string.Empty).Where(char.IsDigit).ToArray());

        if (!CpfChecksum.Valido(cpfDigitos))
            throw new DomainException("O CPF informado é inválido.");

        Id = Guid.NewGuid();
        Nome = nome;
        Email = email;
        SenhaHash = senhaHash;
        Cpf = cpfDigitos;
        Ativo = true;
        DataCriacao = DateTime.UtcNow;
    }

    public void AdicionarRoles(List<UsuarioRole> roles)
    {
        var rolesExistentes = Roles.Select(r => r.Role).ToHashSet();

        var rolesNovas = roles
            .Where(r => !rolesExistentes.Contains(r.Role))
            .DistinctBy(r => r.Role)
            .ToList();

        Roles.AddRange(rolesNovas);
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
