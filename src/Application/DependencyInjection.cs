using System.Diagnostics.CodeAnalysis;
using Application.Common.Interfaces;
using Application.Produtos.UseCases.DecrementarEstoque;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Interfaces;

namespace Application;

// Composição de DI (scan de assembly) — sem lógica de negócio a testar.
[ExcludeFromCodeCoverage]
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Único output port sem Presenter na camada Api: DecrementarEstoqueUseCase só é
        // chamado internamente por OrdemServicoEventHandler (reação a domain event), nunca
        // por um controller HTTP — por isso é registrado aqui, não descoberto por scan.
        services.AddScoped<IDecrementarEstoqueOutputPort, DecrementarEstoqueOutputPort>();

        services.Scan(scan => scan
            .FromAssemblyOf<IUseCase>()
            .AddClasses(c => c.AssignableTo<IUseCase>())
            .AsSelf()
            .WithScopedLifetime());

        services.Scan(scan => scan
            .FromAssemblyOf<IUseCase>()
            .AddClasses(c => c.AssignableTo<IScoped>())
            .AsSelf()
            .WithScopedLifetime());

        services.Scan(scan => scan
            .FromAssemblyOf<IUseCase>()
            // publicOnly: false — handlers de domain event costumam ser internal ao assembly
            // Application; o Scrutor só escaneia classes públicas por padrão.
            .AddClasses(c => c.AssignableTo(typeof(IDomainEventHandler<>)), publicOnly: false)
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        return services;
    }
}
