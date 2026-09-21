using System.Text;
using System.Text.Json;
using Api.Common.Exceptions;
using Api.Middlewares;
using Domain.Common.Exceptions;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;

namespace Tests.Api.Unit;

public class MiddlewaresTests
{
    [Fact]
    public async Task CorrelationIdMiddleware_SemHeaderNaRequisicao_GeraNovoIdEPropagaNaResposta()
    {
        var ctx = new DefaultHttpContext();
        var middleware = new CorrelationIdMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(ctx);

        ctx.Response.Headers.Should().ContainKey(CorrelationIdMiddleware.HeaderName);
        Guid.TryParse(ctx.Response.Headers[CorrelationIdMiddleware.HeaderName].ToString(), out _).Should().BeTrue();
    }

    [Fact]
    public async Task CorrelationIdMiddleware_ComHeaderNaRequisicao_PropagaOMesmoId()
    {
        var ctx = new DefaultHttpContext();
        ctx.Request.Headers[CorrelationIdMiddleware.HeaderName] = "id-existente";
        var middleware = new CorrelationIdMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(ctx);

        ctx.Response.Headers[CorrelationIdMiddleware.HeaderName].ToString().Should().Be("id-existente");
    }

    private static async Task<(int Status, string Body)> ExecutarComExcecao(Exception excecao)
    {
        var ctx = new DefaultHttpContext();
        ctx.Response.Body = new MemoryStream();

        var logger = Mock.Of<ILogger<ExceptionHandlerMiddleware>>();
        var middleware = new ExceptionHandlerMiddleware(_ => throw excecao, logger);

        await middleware.InvokeAsync(ctx);

        ctx.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(ctx.Response.Body).ReadToEndAsync();
        return (ctx.Response.StatusCode, body);
    }

    [Fact]
    public async Task ExceptionHandlerMiddleware_QuandoNenhumaExcecao_NaoAlteraResposta()
    {
        var ctx = new DefaultHttpContext();
        var middleware = new ExceptionHandlerMiddleware(_ => Task.CompletedTask, Mock.Of<ILogger<ExceptionHandlerMiddleware>>());

        await middleware.InvokeAsync(ctx);

        ctx.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Fact]
    public async Task ExceptionHandlerMiddleware_ComDomainException_Retorna400()
    {
        var (status, body) = await ExecutarComExcecao(new DomainException("Regra de negócio violada."));

        status.Should().Be(StatusCodes.Status400BadRequest);
        JsonDocument.Parse(body).RootElement.GetProperty("erro").GetString().Should().Be("Regra de negócio violada.");
    }

    [Fact]
    public async Task ExceptionHandlerMiddleware_ComNotFoundException_Retorna404()
    {
        var (status, body) = await ExecutarComExcecao(new NotFoundException("Recurso não encontrado."));

        status.Should().Be(StatusCodes.Status404NotFound);
        JsonDocument.Parse(body).RootElement.GetProperty("erro").GetString().Should().Be("Recurso não encontrado.");
    }

    [Fact]
    public async Task ExceptionHandlerMiddleware_ComConflictException_Retorna409()
    {
        var (status, body) = await ExecutarComExcecao(new ConflictException("Conflito de estado."));

        status.Should().Be(StatusCodes.Status409Conflict);
        JsonDocument.Parse(body).RootElement.GetProperty("erro").GetString().Should().Be("Conflito de estado.");
    }

    [Fact]
    public async Task ExceptionHandlerMiddleware_ComExcecaoInesperada_Retorna500EFazLog()
    {
        var (status, body) = await ExecutarComExcecao(new InvalidOperationException("Falha inesperada."));

        status.Should().Be(StatusCodes.Status500InternalServerError);
        JsonDocument.Parse(body).RootElement.GetProperty("erro").GetString().Should().Be("Ocorreu um erro interno. Tente novamente.");
    }
}
