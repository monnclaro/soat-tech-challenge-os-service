namespace Domain.Usuarios.Gateways;

public interface IUsuarioGateway
{
    Task<Usuario?> BuscarPorEmail(string email, CancellationToken ct = default);
    Task Salvar(Usuario usuario, CancellationToken ct);
}
