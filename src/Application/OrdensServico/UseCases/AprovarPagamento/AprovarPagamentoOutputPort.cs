namespace Application.OrdensServico.UseCases.AprovarPagamento;

// Implementação "sem apresentação" do output port — usada apenas pelo consumer
// RabbitMQ/MassTransit (PagamentoAprovadoConsumer, que monta seu próprio
// CapturingOutputPort), nunca por um controller HTTP. Registrada explicitamente
// em Application.DependencyInjection, já que não há UI alguma a resolver aqui —
// mesmo padrão de DecrementarEstoqueOutputPort/RegistrarLinkPagamentoOutputPort.
internal sealed class AprovarPagamentoOutputPort : IAprovarPagamentoOutputPort
{
    public void NaoEncontrado()
    {
    }

    public void Ok()
    {
    }
}
