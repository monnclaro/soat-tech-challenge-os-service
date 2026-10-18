using System.Net;
using FluentAssertions;
using Reqnroll;
using Tests.Integration.Infrastructure;

namespace Tests.Integration.Features;

internal record TokenResponse(string Token);
internal record IdResponse(Guid Id);
internal record OrdemServicoStatusResponse(Guid Id, string Status, decimal ValorTotal);

[Binding]
public class SagaCompletaRealSteps
{
    private readonly ApiClient _os = new(SagaInfraFixture.OsServiceUrl);
    private readonly ApiClient _billing = new(SagaInfraFixture.BillingServiceUrl);
    private readonly ApiClient _execution = new(SagaInfraFixture.ExecutionServiceUrl);

    private string _adminToken = string.Empty;
    private Guid _idOrdemServico;
    private Guid _idServico;

    [Given(@"que os 3 microsserviços estão no ar")]
    public async Task DadoQueOs3MicrosservicosEstaoNoAr()
    {
        var login = await _os.PostAsync<TokenResponse>("api/auth/login", new { email = "admin@gmail.com", senha = "123" });

        login.Token.Should().NotBeNullOrEmpty();
        _adminToken = login.Token;
    }

    [Given(@"uma ordem de serviço aberta para um cliente e veículo recém-cadastrados")]
    public async Task DadoUmaOrdemDeServicoAbertaParaUmClienteEVeiculoRecemCadastrados()
    {
        var cliente = await _os.PostAsync<IdResponse>("api/v1/clientes", new
        {
            nome = "Cliente Teste E2E",
            // CPF válido mas diferente dos clientes de demonstração do ClienteSeeder
            // (que já usa "12345678909") — senão a criação dá 409 (documento duplicado).
            documento = "11144477735",
        }, _adminToken);

        var veiculo = await _os.PostAsync<IdResponse>($"api/v1/clientes/{cliente.Id}/veiculos", new
        {
            placa = "XYZ9876",
            marca = "Toyota",
            modelo = "Corolla",
            ano = 2020,
        }, _adminToken);

        var ordemServico = await _os.PostAsync<IdResponse>("api/v1/ordens-servico", new
        {
            idCliente = cliente.Id,
            idVeiculo = veiculo.Id,
        }, _adminToken);

        _idOrdemServico = ordemServico.Id;

        var os = await BuscarOrdemServico();
        os.Status.Should().Be("Recebida");
    }

    [When(@"o diagnóstico é iniciado e finalizado no Execução Service com um serviço e um produto")]
    public async Task QuandoODiagnosticoEIniciadoEFinalizadoNoExecucaoServiceComUmServicoEUmProduto()
    {
        await _execution.PatchAsync($"api/v1/execucoes/{_idOrdemServico}/diagnostico/iniciar", _adminToken);

        _idServico = Guid.NewGuid();

        await _execution.PatchAsync($"api/v1/execucoes/{_idOrdemServico}/diagnostico/finalizar", _adminToken, new
        {
            servicos = new[] { new { idServico = _idServico, nomeServico = "Troca de óleo", valor = 120m } },
            produtos = new[] { new { idProduto = Guid.NewGuid(), nomeProduto = "Óleo 5W30", valorUnitario = 79.90m, quantidade = 2m } },
        });
    }

    [When(@"o pagamento é aprovado via webhook do Mercado Pago no Billing Service")]
    public async Task QuandoOPagamentoEAprovadoViaWebhookDoMercadoPagoNoBillingService()
    {
        // GerarOrcamentoConsumer (Billing Service) reage de forma assíncrona ao comando
        // publicado pelo OS Service — espera o orçamento existir antes de simular o
        // webhook, senão o Billing não acha o que aprovar (e o webhook, nesse caso,
        // devolve 200 do mesmo jeito; ver RegistrarWebhookPagamentoPresenter.NaoEncontrado).
        await PollarAte(async () => await _billing.GetStatusAsync($"api/v1/orcamentos/{_idOrdemServico}", _adminToken) == HttpStatusCode.OK);

        // MercadoPagoGatewayFake (ligado via MercadoPago:UseFake=true) interpreta o
        // paymentId recebido aqui como o próprio IdOrdemServico — não precisa de
        // credencial de sandbox nem de assinatura HMAC (WebhookSecret vazio).
        await _billing.PostAsync("api/v1/webhooks/mercadopago", new
        {
            type = "payment",
            data = new { id = _idOrdemServico.ToString() },
        });
    }

    [When(@"a execução do serviço é iniciada e finalizada no Execução Service")]
    public async Task QuandoAExecucaoDoServicoEIniciadaEFinalizadaNoExecucaoService()
    {
        await _execution.PatchAsync($"api/v1/execucoes/{_idOrdemServico}/servicos/{_idServico}/iniciar-execucao", _adminToken);
        await _execution.PatchAsync($"api/v1/execucoes/{_idOrdemServico}/servicos/{_idServico}/finalizar-execucao", _adminToken);
    }

    [Then(@"a ordem de serviço deve avançar para o status ""(.*)""")]
    public async Task EntaoAOrdemDeServicoDeveAvancarParaOStatus(string statusEsperado)
    {
        OrdemServicoStatusResponse? ultimoEstado = null;

        await PollarAte(async () =>
        {
            ultimoEstado = await BuscarOrdemServico();
            return ultimoEstado.Status == statusEsperado;
        });

        ultimoEstado!.Status.Should().Be(statusEsperado);
    }

    private Task<OrdemServicoStatusResponse> BuscarOrdemServico() =>
        _os.GetAsync<OrdemServicoStatusResponse>($"api/v1/ordens-servico/{_idOrdemServico}", _adminToken);

    // A propagação entre os 3 serviços é assíncrona (RabbitMQ) — espera ativa em vez de
    // Task.Delay fixo, pra não deixar o cenário flaky nem mais lento que o necessário.
    private static async Task PollarAte(Func<Task<bool>> condicao, int tentativas = 30, int intervaloMs = 1000)
    {
        for (var i = 0; i < tentativas; i++)
        {
            try
            {
                if (await condicao())
                {
                    return;
                }
            }
            catch
            {
                // Ainda não propagou (404/erro transitório) — tenta de novo até esgotar.
            }

            await Task.Delay(intervaloMs);
        }

        throw new TimeoutException($"Condição não satisfeita após {tentativas * intervaloMs / 1000}s de espera.");
    }
}
