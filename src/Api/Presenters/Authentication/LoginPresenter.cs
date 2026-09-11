using Api.Extensions.Markers;
using Application.Login.UseCases;
using Application.Login.UseCases.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Api.Presenters.Authentication;

public class LoginPresenter : ILoginOutputPort, IPresenter
{
    public IActionResult? Result { get; private set; }

    public void UsuarioNaoEncontrado() => Result = new NotFoundResult();
    public void SenhaInvalida() => Result = new UnauthorizedResult();
    public void LoginRealizado(LoginOutput output) => Result = new OkObjectResult(new { output.Token });
}
