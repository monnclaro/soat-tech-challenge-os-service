using System.Reflection;
using Application;
using Domain.Clientes;
using Infrastructure.Database;

namespace Tests.Camadas;

public abstract class CamadasBaseTest
{
    protected static readonly Assembly DomainAssembly = typeof(Cliente).Assembly;
    protected static readonly Assembly ApplicationAssembly = typeof(DependencyInjection).Assembly;
    protected static readonly Assembly InfrastructureAssembly = typeof(OsServiceDbContext).Assembly;
    protected static readonly Assembly PresentationAssembly = typeof(Program).Assembly;
}
