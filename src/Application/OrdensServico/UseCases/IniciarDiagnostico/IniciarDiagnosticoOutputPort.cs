namespace Application.OrdensServico.UseCases.IniciarDiagnostico;

// Implementação "sem apresentação" do output port — usada apenas pelo consumer
// RabbitMQ/MassTransit (DiagnosticoIniciadoConsumer), nunca por um controller
// HTTP. Registrada explicitamente em Application.DependencyInjection, já que
// não há UI alguma a resolver aqui — mesmo padrão dos demais output ports só
// de consumer.
internal sealed class IniciarDiagnosticoOutputPort : IIniciarDiagnosticoOutputPort
{
    public void NaoEncontrado()
    {
    }

    public void Ok()
    {
    }
}
