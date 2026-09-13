namespace Application.Clientes.UseCases.AtualizarCliente;

public interface IAtualizarClienteOutputPort
{
    void NaoEncontrado();
    void Ok(ClienteOutput output);
}
