namespace Application.OrdensServico.UseCases.RegistrarDiagnostico;

public record RegistrarDiagnosticoServicoInput(Guid IdServico, string NomeServico, decimal Valor);
public record RegistrarDiagnosticoProdutoInput(Guid IdProduto, string NomeProduto, decimal ValorUnitario, decimal Quantidade);

public record RegistrarDiagnosticoInput(
    Guid IdOrdemServico,
    List<RegistrarDiagnosticoServicoInput> Servicos,
    List<RegistrarDiagnosticoProdutoInput> Produtos);
