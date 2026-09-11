namespace Application.Clientes.UseCases;

public record ClienteOutput(Guid Id, string Nome, string Documento, bool Ativo, DateTime DataCriacao);
