namespace Application.Clientes.Veiculos.UseCases.InserirVeiculo;

public interface IInserirVeiculoOutputPort
{
    void ClienteNaoEncontrado();
    void PlacaDuplicada(string mensagem);
    void Ok(VeiculoOutput output);
}
