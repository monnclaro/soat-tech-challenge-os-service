using System.Diagnostics.CodeAnalysis;
using Api.Extensions.Markers;
using SharedKernel.Interfaces;

namespace Api.Extensions;

// Composição de DI (scan de Presenters/serviços) — sem lógica de negócio a testar.
[ExcludeFromCodeCoverage]
public static class PresentationExtensions
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddControllers();

        services.Scan(scan => scan
            .FromApplicationDependencies()
            .AddClasses(c => c.AssignableTo<IPresenter>())
            .AsSelfWithInterfaces()
            .WithScopedLifetime());

        services.Scan(scan => scan
            .FromApplicationDependencies()
            .AddClasses(c => c.AssignableTo<IScoped>())
            .AsSelfWithInterfaces()
            .WithScopedLifetime());

        services.AddOpenApi();

        return services;
    }
}
