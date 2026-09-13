namespace Application.Clientes.Veiculos.UseCases.BuscarVeiculo;

public interface IBuscarVeiculoOutputPort
{
    void NaoEncontrado();
    void Ok(VeiculoOutput output);
}
