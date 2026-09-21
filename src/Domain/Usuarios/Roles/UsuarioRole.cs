using Domain.Common;

namespace Domain.Usuarios.Roles;

public class UsuarioRole : Entity
{
    public Guid Id { get; private set; }
    public Guid IdUsuario { get; private set; }
    public string Role { get; private set; }

    public UsuarioRole(string role)
    {
        Id = Guid.NewGuid();
        Role = role;
    }
}
