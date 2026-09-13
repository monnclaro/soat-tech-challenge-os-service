using Domain.Usuarios;

namespace Application.Login.UseCases.Interfaces;

public interface ITokenProvider
{
    string GerarToken(Usuario usuario);
}
