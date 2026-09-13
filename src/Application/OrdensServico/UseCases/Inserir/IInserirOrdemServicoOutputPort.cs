namespace Application.OrdensServico.UseCases.Inserir;

public interface IInserirOrdemServicoOutputPort
{
    void ClienteNaoEncontrado();
    void VeiculoNaoPertenceAoCliente(string nomeCliente);
    void Ok(Guid id);
}
