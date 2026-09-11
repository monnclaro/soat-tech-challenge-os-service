using Application.Common.Interfaces;
using Domain.Clientes;
using Domain.Clientes.Gateways;
using Domain.Clientes.Veiculos;
using Domain.Clientes.Veiculos.Gateways;
using Domain.OrdensServico.Gateways;
using SharedKernel.DTOs;

namespace Application.OrdensServico.UseCases.BuscarListaPaginadaPorDocumento;

public class BuscarListaPaginadaPorDocumentoUseCase : IUseCase
{
    private readonly IOrdemServicoGateway _gateway;
    private readonly IClienteGateway _clienteGateway;
    private readonly IVeiculoGateway _veiculoGateway;
    private readonly IBuscarListaPaginadaPorDocumentoOutputPort _outputPort;

    public BuscarListaPaginadaPorDocumentoUseCase(
        IOrdemServicoGateway gateway,
        IClienteGateway clienteGateway,
        IVeiculoGateway veiculoGateway,
        IBuscarListaPaginadaPorDocumentoOutputPort outputPort)
    {
        _gateway = gateway;
        _clienteGateway = clienteGateway;
        _veiculoGateway = veiculoGateway;
        _outputPort = outputPort;
    }

    public async Task Execute(BuscarListaPaginadaPorDocumentoInput input, CancellationToken ct = default)
    {
        var documentoLimpo = new string(input.Documento.Where(char.IsDigit).ToArray());

        if (input.CallerDocumento is { } callerDocumento)
        {
            var callerDocumentoLimpo = new string(callerDocumento.Where(char.IsDigit).ToArray());
            if (callerDocumentoLimpo != documentoLimpo)
            {
                // Resultado vazio (não erro) de propósito: não confirma pra um
                // Cliente se existe OS pra outro documento.
                _outputPort.Ok(new PagedResult<OrdemServicoPorDocumentoOutput>([], 0, input.Paginacao.Pagina, input.Paginacao.Tamanho));
                return;
            }
        }

        var (items, total) = await _gateway.BuscarPaginadoPorDocumentoCliente(documentoLimpo, input.Paginacao, ct);

        var clientesCache = new Dictionary<Guid, Cliente>();
        var veiculosCache = new Dictionary<Guid, Veiculo>();
        var outputs = new List<OrdemServicoPorDocumentoOutput>();

        foreach (var os in items)
        {
            if (!clientesCache.TryGetValue(os.IdCliente, out var cliente))
            {
                cliente = (await _clienteGateway.BuscarPorId(os.IdCliente, ct))!;
                clientesCache[os.IdCliente] = cliente;
            }

            if (!veiculosCache.TryGetValue(os.IdVeiculo, out var veiculo))
            {
                veiculo = (await _veiculoGateway.BuscarPorId(os.IdVeiculo, ct))!;
                veiculosCache[os.IdVeiculo] = veiculo;
            }

            outputs.Add(new OrdemServicoPorDocumentoOutput(
                os.Id,
                os.Status.ToString(),
                new OrdemServicoClientePorDocumentoOutput(cliente.Nome, cliente.Documento),
                new OrdemServicoVeiculoPorDocumentoOutput(veiculo.Placa, veiculo.Marca, veiculo.Modelo, veiculo.Ano),
                os.Servicos.Select(s => new OrdemServicoServicoPorDocumentoOutput(s.NomeServico)).ToList()));
        }

        _outputPort.Ok(new PagedResult<OrdemServicoPorDocumentoOutput>(outputs, total, input.Paginacao.Pagina, input.Paginacao.Tamanho));
    }
}
