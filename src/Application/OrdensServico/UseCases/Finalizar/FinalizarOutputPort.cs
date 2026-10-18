namespace Application.OrdensServico.UseCases.Finalizar;

// Implementação "sem apresentação" do output port — usada apenas pelo consumer
// RabbitMQ/MassTransit (ExecucaoFinalizadaConsumer, que monta seu próprio
// CapturingOutputPort), nunca por um controller HTTP. Registrada explicitamente
// em Application.DependencyInjection, já que não há UI alguma a resolver aqui —
// mesmo padrão de DecrementarEstoqueOutputPort/IniciarDiagnosticoOutputPort.
internal sealed class FinalizarOutputPort : IFinalizarOutputPort
{
    public void NaoEncontrado()
    {
    }

    public void Ok()
    {
    }
}
