namespace Application.Clientes.UseCases.BuscarCliente;

public interface IBuscarClienteOutputPort
{
    void NaoEncontrado();
    void Ok(ClienteOutput output);
}
