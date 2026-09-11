using Application.Login.UseCases;
using Application.Login.UseCases.DTOs;
using SharedKernel.Interfaces;

namespace Application.Login.Controllers;

public class LoginController : IScoped
{
    private readonly LoginUseCase _useCase;

    public LoginController(LoginUseCase useCase) => _useCase = useCase;

    public async Task Execute(LoginInput input, CancellationToken ct = default) => await _useCase.Execute(input, ct);
}
