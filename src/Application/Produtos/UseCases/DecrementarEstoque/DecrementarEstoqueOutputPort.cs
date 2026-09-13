namespace Application.Produtos.UseCases.DecrementarEstoque;

// Implementação "sem apresentação" do output port — usada apenas pelo handler
// de domain event (OrdemServicoEventHandler), nunca por um controller HTTP
// (por isso não é um Presenter da camada Api: DecrementarEstoque nunca é
// chamado diretamente por uma requisição). Registrada explicitamente em
// Application.DependencyInjection, já que não há UI alguma a resolver aqui.
internal sealed class DecrementarEstoqueOutputPort : IDecrementarEstoqueOutputPort
{
    public void Ok()
    {
    }
}
