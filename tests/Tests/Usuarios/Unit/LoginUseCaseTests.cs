using Application.Login.UseCases;
using Application.Login.UseCases.DTOs;
using Application.Login.UseCases.Interfaces;
using Domain.Usuarios;
using Domain.Usuarios.Gateways;
using Moq;

namespace Tests.Usuarios.Unit;

public class LoginUseCaseTests
{
    private static Usuario CriarUsuario() => new("Admin", "admin@teste.com", "hash-valido", "12345678909");

    [Fact]
    public async Task Execute_QuandoUsuarioNaoEncontrado_ChamaUsuarioNaoEncontrado()
    {
        var gateway = new Mock<IUsuarioGateway>();
        gateway.Setup(g => g.BuscarPorEmail(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((Usuario?)null);

        var hasher = new Mock<IPasswordHasher>();
        var tokenProvider = new Mock<ITokenProvider>();
        var outputPort = new Mock<ILoginOutputPort>();
        var useCase = new LoginUseCase(gateway.Object, hasher.Object, tokenProvider.Object, outputPort.Object);

        await useCase.Execute(new LoginInput("naoexiste@teste.com", "123456"));

        outputPort.Verify(p => p.UsuarioNaoEncontrado(), Times.Once);
        hasher.Verify(h => h.Verificar(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Execute_QuandoSenhaInvalida_ChamaSenhaInvalida()
    {
        var usuario = CriarUsuario();
        var gateway = new Mock<IUsuarioGateway>();
        gateway.Setup(g => g.BuscarPorEmail(usuario.Email, It.IsAny<CancellationToken>())).ReturnsAsync(usuario);

        var hasher = new Mock<IPasswordHasher>();
        hasher.Setup(h => h.Verificar("senha-errada", usuario.SenhaHash)).Returns(false);

        var tokenProvider = new Mock<ITokenProvider>();
        var outputPort = new Mock<ILoginOutputPort>();
        var useCase = new LoginUseCase(gateway.Object, hasher.Object, tokenProvider.Object, outputPort.Object);

        await useCase.Execute(new LoginInput(usuario.Email, "senha-errada"));

        outputPort.Verify(p => p.SenhaInvalida(), Times.Once);
        tokenProvider.Verify(t => t.GerarToken(It.IsAny<Usuario>()), Times.Never);
    }

    [Fact]
    public async Task Execute_ComCredenciaisValidas_ChamaLoginRealizadoComToken()
    {
        var usuario = CriarUsuario();
        var gateway = new Mock<IUsuarioGateway>();
        gateway.Setup(g => g.BuscarPorEmail(usuario.Email, It.IsAny<CancellationToken>())).ReturnsAsync(usuario);

        var hasher = new Mock<IPasswordHasher>();
        hasher.Setup(h => h.Verificar("senha-correta", usuario.SenhaHash)).Returns(true);

        var tokenProvider = new Mock<ITokenProvider>();
        tokenProvider.Setup(t => t.GerarToken(usuario)).Returns("token-jwt");

        var outputPort = new Mock<ILoginOutputPort>();
        var useCase = new LoginUseCase(gateway.Object, hasher.Object, tokenProvider.Object, outputPort.Object);

        await useCase.Execute(new LoginInput(usuario.Email, "senha-correta"));

        outputPort.Verify(p => p.LoginRealizado(It.Is<LoginOutput>(o => o.Token == "token-jwt")), Times.Once);
    }
}
