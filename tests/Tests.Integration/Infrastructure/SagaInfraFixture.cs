using System.Runtime.CompilerServices;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Images;
using DotNet.Testcontainers.Networks;
using Testcontainers.MongoDb;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace Tests.Integration.Infrastructure;

// Sobe os 3 microsserviços de verdade (imagens construídas a partir dos Dockerfiles de
// cada repo) + Postgres x2/Mongo/RabbitMQ via Testcontainers, todos na mesma rede Docker,
// pra exercitar a saga completa ponta a ponta (ver Features/SagaCompletaReal.feature).
// Único pra todo o test run (via Hooks/TestRunHooks.cs) — construir 3 imagens e subir
// 7 containers por cenário seria lento demais pra valer a pena repetir por teste.
//
// JWT secret e credenciais do RabbitMQ são valores fixos só pra este teste (sem
// relação com nenhum ambiente real).
public static class SagaInfraFixture
{
    private const string JwtSecret = "chave-de-teste-so-para-o-bdd-de-integracao-1234567890";
    private const string RabbitMqUsername = "soat";
    private const string RabbitMqPassword = "soat-teste-integracao";
    private const string MongoUsername = "soat";
    private const string MongoPassword = "soat-teste-integracao";
    private const ushort ApiPort = 8080;

    // Build de 3 imagens .NET do zero + start de 7 containers não deveria passar disso
    // nem no CI mais lento/sem cache de camada do Docker (localmente fica em ~2 min a
    // frio). Sem isso, um container que nunca fica saudável (ex.: um dos 3 serviços
    // crashando silenciosamente) prende o job inteiro até o timeout do GitHub Actions
    // (6h por padrão) em vez de falhar rápido com um erro claro.
    private static readonly TimeSpan StartupTimeout = TimeSpan.FromMinutes(10);

    private static INetwork? _network;
    private static PostgreSqlContainer? _osPostgres;
    private static PostgreSqlContainer? _billingPostgres;
    private static MongoDbContainer? _executionMongo;
    private static RabbitMqContainer? _rabbitMq;
    private static IContainer? _osService;
    private static IContainer? _billingService;
    private static IContainer? _executionService;

    public static string OsServiceUrl { get; private set; } = string.Empty;
    public static string BillingServiceUrl { get; private set; } = string.Empty;
    public static string ExecutionServiceUrl { get; private set; } = string.Empty;

    public static async Task StartAsync()
    {
        using var cts = new CancellationTokenSource(StartupTimeout);
        var ct = cts.Token;

        _network = new NetworkBuilder().Build();
        await _network.CreateAsync(ct);

        _osPostgres = BuildPostgres("os-postgres", "soat_os");
        _billingPostgres = BuildPostgres("billing-postgres", "soat_billing");

        // Usuário/senha explícitos: o módulo MongoDb do Testcontainers sempre sobe o
        // Mongo com autenticação exigida (usuário root em MONGO_INITDB_ROOT_USERNAME) —
        // sem credencial, "find"/"insert" falham com "Command ... requires authentication".
        _executionMongo = new MongoDbBuilder()
            .WithImage("mongo:7")
            .WithUsername(MongoUsername)
            .WithPassword(MongoPassword)
            .WithNetwork(_network)
            .WithNetworkAliases("execution-mongo")
            .Build();

        // Usuário dedicado (não "guest"): o RabbitMQ só aceita login do "guest" via
        // localhost por padrão (restrição de loopback do próprio broker) — os 3 serviços
        // conectam pelo alias de rede "rabbitmq", não localhost, e o guest seria rejeitado
        // com ACCESS_REFUSED.
        _rabbitMq = new RabbitMqBuilder()
            .WithImage("rabbitmq:3-management-alpine")
            .WithUsername(RabbitMqUsername)
            .WithPassword(RabbitMqPassword)
            .WithNetwork(_network)
            .WithNetworkAliases("rabbitmq")
            .Build();

        var infraReady = Task.WhenAll(
            _osPostgres.StartAsync(ct),
            _billingPostgres.StartAsync(ct),
            _executionMongo.StartAsync(ct),
            _rabbitMq.StartAsync(ct));

        var osImageTask = BuildServiceImage(RepoRoot(), "soat-os-service-it", ct);
        var billingImageTask = BuildServiceImage(SiblingRepoRoot("soat-tech-challenge-billing-service"), "soat-billing-service-it", ct);
        var executionImageTask = BuildServiceImage(SiblingRepoRoot("soat-tech-challenge-execution-service"), "soat-execution-service-it", ct);

        await Task.WhenAll(infraReady, osImageTask, billingImageTask, executionImageTask);

        _osService = BuildServiceContainer(await osImageTask, "os-service", new()
        {
            ["ConnectionStrings__Default"] = "Host=os-postgres;Port=5432;Database=soat_os;Username=postgres;Password=postgres",
            ["RabbitMq__Host"] = "rabbitmq",
            ["RabbitMq__VirtualHost"] = "/",
            ["RabbitMq__Username"] = RabbitMqUsername,
            ["RabbitMq__Password"] = RabbitMqPassword,
            ["JwtSettings__Secret"] = JwtSecret,
            ["JwtSettings__ExpirationHours"] = "2",
        });

        _billingService = BuildServiceContainer(await billingImageTask, "billing-service", new()
        {
            ["ConnectionStrings__Default"] = "Host=billing-postgres;Port=5432;Database=soat_billing;Username=postgres;Password=postgres",
            ["RabbitMq__Host"] = "rabbitmq",
            ["RabbitMq__VirtualHost"] = "/",
            ["RabbitMq__Username"] = RabbitMqUsername,
            ["RabbitMq__Password"] = RabbitMqPassword,
            ["JwtSettings__Secret"] = JwtSecret,
            // Sem credencial de sandbox do Mercado Pago disponível em CI — usa o fake
            // gateway (ver MercadoPagoGatewayFake no repo do Billing Service), que também
            // dispensa validação de assinatura HMAC do webhook (WebhookSecret fica vazio).
            ["MercadoPago__UseFake"] = "true",
        });

        _executionService = BuildServiceContainer(await executionImageTask, "execution-service", new()
        {
            ["MongoSettings__ConnectionString"] = $"mongodb://{MongoUsername}:{MongoPassword}@execution-mongo:27017/?authSource=admin",
            ["MongoSettings__Database"] = "soat_execucao",
            ["RabbitMq__Host"] = "rabbitmq",
            ["RabbitMq__VirtualHost"] = "/",
            ["RabbitMq__Username"] = RabbitMqUsername,
            ["RabbitMq__Password"] = RabbitMqPassword,
            ["JwtSettings__Secret"] = JwtSecret,
        });

        await Task.WhenAll(_osService.StartAsync(ct), _billingService.StartAsync(ct), _executionService.StartAsync(ct));

        OsServiceUrl = $"http://{_osService.Hostname}:{_osService.GetMappedPublicPort(ApiPort)}";
        BillingServiceUrl = $"http://{_billingService.Hostname}:{_billingService.GetMappedPublicPort(ApiPort)}";
        ExecutionServiceUrl = $"http://{_executionService.Hostname}:{_executionService.GetMappedPublicPort(ApiPort)}";
    }

    public static async Task StopAsync()
    {
        await DisposeIfStarted(_osService);
        await DisposeIfStarted(_billingService);
        await DisposeIfStarted(_executionService);
        await DisposeIfStarted(_rabbitMq);
        await DisposeIfStarted(_executionMongo);
        await DisposeIfStarted(_billingPostgres);
        await DisposeIfStarted(_osPostgres);
        await DisposeIfStarted(_network);
    }

    private static async Task DisposeIfStarted(IAsyncDisposable? resource)
    {
        if (resource is not null)
        {
            await resource.DisposeAsync();
        }
    }

    private static PostgreSqlContainer BuildPostgres(string alias, string database) =>
        new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase(database)
            .WithUsername("postgres")
            .WithPassword("postgres")
            .WithNetwork(_network)
            .WithNetworkAliases(alias)
            .Build();

    private static async Task<IFutureDockerImage> BuildServiceImage(string contextDirectory, string imageName, CancellationToken ct)
    {
        var image = new ImageFromDockerfileBuilder()
            .WithName(imageName)
            .WithDockerfileDirectory(contextDirectory)
            .WithDockerfile("src/Api/Dockerfile")
            .WithDeleteIfExists(true)
            .Build();

        await image.CreateAsync(ct);
        return image;
    }

    private static IContainer BuildServiceContainer(IFutureDockerImage image, string alias, Dictionary<string, string> environment)
    {
        var builder = new ContainerBuilder()
            .WithImage(image)
            .WithNetwork(_network)
            .WithNetworkAliases(alias)
            .WithPortBinding(ApiPort, assignRandomHostPort: true)
            .WithEnvironment("ASPNETCORE_URLS", $"http://+:{ApiPort}")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(r => r.ForPort(ApiPort).ForPath("/health")));

        foreach (var (key, value) in environment)
        {
            builder = builder.WithEnvironment(key, value);
        }

        return builder.Build();
    }

    // Raiz deste repo (os-service) — resolvida a partir do próprio arquivo fonte via
    // [CallerFilePath], não da CWD do processo de teste (que varia entre IDE/CLI/CI).
    // Este arquivo fica em tests/Tests.Integration/Infrastructure/, 3 níveis abaixo da raiz.
    private static string RepoRoot([CallerFilePath] string sourceFile = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(sourceFile)!, "..", "..", ".."));

    // Repos irmãos (Billing/Execução). Localmente, ficam na mesma pasta "Tech Challenge -
    // FIAP" (um nível acima deste repo) — layout padrão de quem clonou todos os repos
    // lado a lado. No CI, "actions/checkout" recusa qualquer "path" que escape do próprio
    // diretório do repo principal (não dá pra checkar fora dele nem indiretamente, mesmo
    // ainda dentro da pasta "work/<repo>" do runner) — por isso lá eles são checked out
    // numa subpasta (".sibling-repos/<repo>") e a env var SOAT_SIBLING_REPOS_DIR (setada
    // só no job de CI, ver ci-cd.yml) aponta pra ela.
    private static string SiblingRepoRoot(string repoName)
    {
        var baseDir = Environment.GetEnvironmentVariable("SOAT_SIBLING_REPOS_DIR");

        if (string.IsNullOrEmpty(baseDir))
        {
            baseDir = Path.Combine(RepoRoot(), "..");
        }

        return Path.GetFullPath(Path.Combine(baseDir, repoName));
    }
}
