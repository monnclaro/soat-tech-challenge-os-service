using System.Reflection;
using Application;
using Domain;
using Infrastructure.Database;

namespace Tests.Camadas;

public abstract class CamadasBaseTest
{
    protected static readonly Assembly DomainAssembly = typeof(AssemblyMarker).Assembly;
    protected static readonly Assembly ApplicationAssembly = typeof(DependencyInjection).Assembly;
    protected static readonly Assembly InfrastructureAssembly = typeof(OsServiceDbContext).Assembly;
    protected static readonly Assembly PresentationAssembly = typeof(Program).Assembly;
}
