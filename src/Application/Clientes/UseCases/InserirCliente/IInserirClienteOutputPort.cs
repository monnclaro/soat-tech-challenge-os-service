namespace Application.Clientes.UseCases.InserirCliente;

public interface IInserirClienteOutputPort
{
    void DocumentoDuplicado(string mensagem);
    void Ok(ClienteOutput output);
}
