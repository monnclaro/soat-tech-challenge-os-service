namespace Application.Servicos.UseCases.AtualizarServico;

public record AtualizarServicoInput(Guid Id, string Nome, string Descricao, decimal Valor);
