using Application.Login.UseCases.DTOs;

namespace Application.Login.UseCases;

public interface ILoginOutputPort
{
    void UsuarioNaoEncontrado();
    void SenhaInvalida();
    void LoginRealizado(LoginOutput output);
}
