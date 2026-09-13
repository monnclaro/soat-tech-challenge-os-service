using Application.Common.Interfaces;
using Application.Login.UseCases.DTOs;
using Application.Login.UseCases.Interfaces;
using Domain.Usuarios.Gateways;

namespace Application.Login.UseCases;

public class LoginUseCase : IUseCase
{
    private readonly IUsuarioGateway _gateway;
    private readonly IPasswordHasher _hasher;
    private readonly ITokenProvider _tokenProvider;
    private readonly ILoginOutputPort _outputPort;

    public LoginUseCase(
        IUsuarioGateway gateway,
        IPasswordHasher hasher,
        ITokenProvider tokenProvider,
        ILoginOutputPort outputPort)
    {
        _gateway = gateway;
        _hasher = hasher;
        _tokenProvider = tokenProvider;
        _outputPort = outputPort;
    }

    public async Task Execute(LoginInput input, CancellationToken ct = default)
    {
        var usuario = await _gateway.BuscarPorEmail(input.Email, ct);

        if (usuario is null)
        {
            _outputPort.UsuarioNaoEncontrado();
            return;
        }

        if (!_hasher.Verificar(input.Senha, usuario.SenhaHash))
        {
            _outputPort.SenhaInvalida();
            return;
        }

        var token = _tokenProvider.GerarToken(usuario);
        _outputPort.LoginRealizado(new LoginOutput(token));
    }
}
