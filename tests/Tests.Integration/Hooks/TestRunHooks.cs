using Reqnroll;
using Tests.Integration.Infrastructure;

namespace Tests.Integration.Hooks;

// Sobe/derruba a infraestrutura (3 serviços + Postgres x2/Mongo/RabbitMQ) uma única vez
// pro test run inteiro, não por cenário — ver justificativa em SagaInfraFixture.
[Binding]
public static class TestRunHooks
{
    [BeforeTestRun]
    public static async Task AntesDoTestRun() => await SagaInfraFixture.StartAsync();

    [AfterTestRun]
    public static async Task DepoisDoTestRun() => await SagaInfraFixture.StopAsync();
}
