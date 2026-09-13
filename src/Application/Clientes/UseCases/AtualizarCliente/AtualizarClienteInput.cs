namespace Application.Clientes.UseCases.AtualizarCliente;

public record AtualizarClienteInput(Guid Id, string Nome, bool Ativo);
