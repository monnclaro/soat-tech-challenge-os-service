using Application.Common.Interfaces;
using Domain.OrdensServico.Gateways;
using Domain.OrdensServico.Produtos;
using Domain.OrdensServico.Servicos;

namespace Application.OrdensServico.UseCases.RegistrarDiagnostico;

// Passo 3 da saga: hoje disparado via endpoint interno recebendo o mesmo
// payload que, futuramente, virá no evento "DiagnosticoFinalizado" publicado
// pelo Execução Service (nome/valor snapshot dos itens identificados) — ver
// PLANO-FASE-4-MICROSSERVICOS.md. Ao concluir, o próximo passo da saga é
// comandar o Billing Service a gerar o orçamento (ainda não ligado).
public class RegistrarDiagnosticoUseCase : IUseCase
{
    private readonly IOrdemServicoGateway _gateway;
    private readonly IRegistrarDiagnosticoOutputPort _outputPort;

    public RegistrarDiagnosticoUseCase(IOrdemServicoGateway gateway, IRegistrarDiagnosticoOutputPort outputPort)
    {
        _gateway = gateway;
        _outputPort = outputPort;
    }

    public async Task Execute(RegistrarDiagnosticoInput input, CancellationToken ct = default)
    {
        var ordemServico = await _gateway.BuscarPorId(input.IdOrdemServico, ct);
        if (ordemServico is null) { _outputPort.NaoEncontrado(); return; }

        var servicos = input.Servicos
            .Select(s => new OrdemServicoServico(ordemServico.Id, s.IdServico, s.NomeServico, s.Valor))
            .ToList();

        var produtos = input.Produtos
            .Select(p => new OrdemServicoProduto(ordemServico.Id, p.IdProduto, p.NomeProduto, p.ValorUnitario, p.Quantidade))
            .ToList();

        ordemServico.RegistrarDiagnostico(servicos, produtos);
        await _gateway.Atualizar(ordemServico, ct);

        _outputPort.Ok();
    }
}
