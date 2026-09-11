using Api.Controllers.Authentication.Requests;
using Api.Presenters.Authentication;
using Application.Login.Controllers;
using Application.Login.UseCases.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Authentication;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthenticationController : ControllerBase
{
    private readonly LoginController _controller;
    private readonly LoginPresenter _presenter;

    public AuthenticationController(LoginController controller, LoginPresenter presenter)
    {
        _controller = controller;
        _presenter = presenter;
    }

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken ct)
    {
        await _controller.Execute(new LoginInput(request.Email, request.Senha), ct);
        return _presenter.Result!;
    }
}
