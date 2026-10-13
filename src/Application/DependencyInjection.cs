using System.Diagnostics.CodeAnalysis;
using Application.Common.Interfaces;
using Application.OrdensServico.UseCases.AprovarPagamento;
using Application.OrdensServico.UseCases.Finalizar;
using Application.OrdensServico.UseCases.IniciarDiagnostico;
using Application.OrdensServico.UseCases.IniciarExecucao;
using Application.OrdensServico.UseCases.RegistrarDiagnostico;
using Application.OrdensServico.UseCases.RegistrarLinkPagamento;
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
        // Output ports sem Presenter na camada Api: esses use cases só são chamados
        // internamente (domain event handler / consumer RabbitMQ), nunca por um
        // controller HTTP — por isso são registrados aqui, não descobertos por scan.
        services.AddScoped<IDecrementarEstoqueOutputPort, DecrementarEstoqueOutputPort>();
        services.AddScoped<IRegistrarLinkPagamentoOutputPort, RegistrarLinkPagamentoOutputPort>();
        services.AddScoped<IRegistrarDiagnosticoOutputPort, RegistrarDiagnosticoOutputPort>();
        services.AddScoped<IIniciarDiagnosticoOutputPort, IniciarDiagnosticoOutputPort>();
        services.AddScoped<IIniciarExecucaoOutputPort, IniciarExecucaoOutputPort>();
        services.AddScoped<IAprovarPagamentoOutputPort, AprovarPagamentoOutputPort>();
        services.AddScoped<IFinalizarOutputPort, FinalizarOutputPort>();

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
