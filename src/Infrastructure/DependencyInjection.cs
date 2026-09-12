using System.Diagnostics.CodeAnalysis;
using Application.Common.Interfaces;
using Application.Login.UseCases.Interfaces;
using Infrastructure.Database;
using Infrastructure.DomainEvents;
using Infrastructure.Messaging;
using Infrastructure.Messaging.Consumers;
using Infrastructure.Security.BCrypt;
using Infrastructure.Security.Jwt;
using Infrastructure.Seeders;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

// Composição de DI (DbContext/MassTransit/serviços) — sem lógica de negócio a testar.
[ExcludeFromCodeCoverage]
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration) =>
        services
            .AddServices()
            .AddDatabase(configuration)
            .AddMessaging(configuration);

    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddTransient<IDomainEventsDispatcher, DomainEventsDispatcher>();

        services.AddScoped<IDatabaseSeeder, DatabaseSeeder>();
        services.AddScoped<ITokenProvider, JwtTokenProvider>();
        services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();

        services.Scan(scan => scan
            .FromApplicationDependencies()
            .AddClasses(c => c.Where(t => t.Name.EndsWith("Gateway")))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        return services;
    }

    private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString("Default");

        services.AddDbContext<OsServiceDbContext>(options =>
            options.UseNpgsql(connectionString,
                npgsqlOptions => npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName)));

        return services;
    }

    private static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RabbitMqSettings>(configuration.GetSection("RabbitMq"));
        services.AddScoped<ISagaCommandBus, MassTransitSagaCommandBus>();

        var rabbitMq = configuration.GetSection("RabbitMq").Get<RabbitMqSettings>() ?? new RabbitMqSettings();

        services.AddMassTransit(x =>
        {
            x.AddConsumer<DiagnosticoFinalizadoConsumer>();
            x.AddConsumer<DiagnosticoFalhouConsumer>();
            x.AddConsumer<PagamentoAprovadoConsumer>();
            x.AddConsumer<PagamentoRecusadoConsumer>();
            x.AddConsumer<ExecucaoFinalizadaConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(rabbitMq.Host, rabbitMq.VirtualHost, h =>
                {
                    h.Username(rabbitMq.Username);
                    h.Password(rabbitMq.Password);
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
