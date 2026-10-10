namespace Application.OrdensServico.UseCases.RegistrarLinkPagamento;

// Implementação "sem apresentação" do output port — usada apenas pelo consumer
// RabbitMQ/MassTransit (OrcamentoGeradoConsumer), nunca por um controller HTTP
// (por isso não é um Presenter da camada Api). Registrada explicitamente em
// Application.DependencyInjection, já que não há UI alguma a resolver aqui —
// mesmo padrão de DecrementarEstoqueOutputPort.
internal sealed class RegistrarLinkPagamentoOutputPort : IRegistrarLinkPagamentoOutputPort
{
    public void NaoEncontrado()
    {
    }

    public void Ok()
    {
    }
}
