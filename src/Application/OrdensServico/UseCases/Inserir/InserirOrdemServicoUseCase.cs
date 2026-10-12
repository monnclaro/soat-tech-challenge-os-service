using Application.Common.Interfaces;
using Domain.Clientes.Gateways;
using Domain.OrdensServico;
using Domain.OrdensServico.Gateways;

namespace Application.OrdensServico.UseCases.Inserir;

// Abertura da OS: só cabeçalho (cliente + veículo), status Recebida. O
// diagnóstico (que identifica os serviços/produtos necessários) passou a ser
// responsabilidade do Execução Service, comandado de forma assíncrona a
// partir daqui (EnviarIniciarDiagnosticoHandler, reagindo a
// OrdemServicoAbertaDomainEvent) — mas a OS só avança para EmDiagnostico
// quando o Execução Service confirma de volta (evento DiagnosticoIniciado,
// ver DiagnosticoIniciadoConsumer), nunca de forma otimista aqui. Mesmo
// padrão reativo usado em todas as outras transições da saga.
public class InserirOrdemServicoUseCase : IUseCase
{
    private readonly IOrdemServicoGateway _gateway;
    private readonly IClienteGateway _clienteGateway;
    private readonly IInserirOrdemServicoOutputPort _outputPort;

    public InserirOrdemServicoUseCase(
        IOrdemServicoGateway gateway,
        IClienteGateway clienteGateway,
        IInserirOrdemServicoOutputPort outputPort)
    {
        _gateway = gateway;
        _clienteGateway = clienteGateway;
        _outputPort = outputPort;
    }

    public async Task Execute(InserirOrdemServicoInput input, CancellationToken ct = default)
    {
        var cliente = await _clienteGateway.BuscarComVeiculos(input.IdCliente, ct);
        if (cliente is null) { _outputPort.ClienteNaoEncontrado(); return; }

        if (cliente.Veiculos.All(v => v.Id != input.IdVeiculo))
        {
            _outputPort.VeiculoNaoPertenceAoCliente(cliente.Nome);
            return;
        }

        var ordemServico = new OrdemServico();
        ordemServico.Inserir(input.IdCliente, input.IdVeiculo);

        await _gateway.Salvar(ordemServico, ct);
        _outputPort.Ok(ordemServico.Id);
    }
}
