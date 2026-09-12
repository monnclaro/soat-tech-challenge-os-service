using Application.OrdensServico.UseCases.RegistrarDiagnostico;
using Domain.OrdensServico.Gateways;
using MassTransit;
using Soat.Contracts.Saga;

namespace Infrastructure.Messaging.Consumers;

// Consome o evento publicado pelo Execução Service ao concluir o diagnóstico,
// reaproveitando o mesmo RegistrarDiagnosticoUseCase usado pelo endpoint
// interno equivalente (única fonte de verdade da regra de negócio,
// independente do meio de entrada).
public class DiagnosticoFinalizadoConsumer : IConsumer<DiagnosticoFinalizado>
{
    private readonly IOrdemServicoGateway _gateway;

    public DiagnosticoFinalizadoConsumer(IOrdemServicoGateway gateway)
    {
        _gateway = gateway;
    }

    public async Task Consume(ConsumeContext<DiagnosticoFinalizado> context)
    {
        var msg = context.Message;
        var outputPort = new CapturingOutputPort();
        var useCase = new RegistrarDiagnosticoUseCase(_gateway, outputPort);

        var input = new RegistrarDiagnosticoInput(
            msg.IdOrdemServico,
            msg.Servicos.Select(s => new RegistrarDiagnosticoServicoInput(s.IdServico, s.NomeServico, s.Valor)).ToList(),
            msg.Produtos.Select(p => new RegistrarDiagnosticoProdutoInput(p.IdProduto, p.NomeProduto, p.ValorUnitario, p.Quantidade)).ToList());

        await useCase.Execute(input, context.CancellationToken);

        if (!outputPort.Sucesso)
        {
            throw new InvalidOperationException($"OrdemServico '{msg.IdOrdemServico}' não encontrada ao registrar diagnóstico.");
        }
    }

    private sealed class CapturingOutputPort : IRegistrarDiagnosticoOutputPort
    {
        public bool Sucesso { get; private set; }
        public void NaoEncontrado() => Sucesso = false;
        public void Ok() => Sucesso = true;
    }
}
