namespace Application.Clientes.Veiculos.UseCases.AtualizarVeiculo;

public interface IAtualizarVeiculoOutputPort
{
    void NaoEncontrado();
    void PlacaDuplicada(string mensagem);
    void Ok(VeiculoOutput output);
}
