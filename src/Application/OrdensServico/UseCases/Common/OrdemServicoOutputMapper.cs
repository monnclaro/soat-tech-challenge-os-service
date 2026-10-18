using Domain.Clientes;
using Domain.Clientes.Veiculos;

namespace Application.OrdensServico.UseCases.Common;

// Sem CQRS/query gateway dedicado (como no monolito de origem) — o gateway
// único de OrdemServico devolve a entidade, e este mapper monta o output
// juntando Cliente/Veiculo via seus próprios gateways. Simplificação
// deliberada para o escopo deste microsserviço (poucas entidades, poucas
// linhas por consulta) — trocar por uma projeção OrdemServicoQueryGateway se
// o volume justificar no futuro.
public static class OrdemServicoOutputMapper
{
    public static OrdemServicoOutput Map(Domain.OrdensServico.OrdemServico os, Cliente cliente, Veiculo veiculo) =>
        new(
            os.Id,
            new OrdemServicoClienteOutput(cliente.Id, cliente.Nome, cliente.Documento),
            new OrdemServicoVeiculoOutput(veiculo.Id, veiculo.Placa, veiculo.Marca, veiculo.Modelo, veiculo.Ano),
            os.DataCriacao,
            os.DataInicioExecucao,
            os.DataFinalizacao,
            os.Status.ToString(),
            os.ValorTotal,
            os.Servicos.Select(s => new OrdemServicoServicoOutput(s.Id, s.IdServico, s.NomeServico, s.Valor)).ToList(),
            os.Produtos.Select(p => new OrdemServicoProdutoOutput(p.Id, p.IdProduto, p.NomeProduto, p.ValorUnitario, p.Quantidade)).ToList());
}
