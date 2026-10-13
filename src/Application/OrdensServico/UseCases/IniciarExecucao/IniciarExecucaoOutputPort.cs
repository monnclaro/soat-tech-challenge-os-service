namespace Application.OrdensServico.UseCases.IniciarExecucao;

// Implementação "sem apresentação" do output port — usada apenas pelo consumer
// RabbitMQ/MassTransit (ExecucaoIniciadaConsumer), nunca por um controller
// HTTP. Registrada explicitamente em Application.DependencyInjection, já que
// não há UI alguma a resolver aqui — mesmo padrão dos demais output ports só
// de consumer.
internal sealed class IniciarExecucaoOutputPort : IIniciarExecucaoOutputPort
{
    public void NaoEncontrado()
    {
    }

    public void Ok()
    {
    }
}
