using Domain.Common;

namespace Domain.OrdensServico.Servicos;

// Snapshot do item de serviço identificado no diagnóstico (nome/valor no momento do
// registro). O ciclo de vida de execução em si (iniciar/finalizar reparo) passou a
// viver no Execução Service (fila de execução, MongoDB) — este serviço só guarda
// o resultado consolidado para consulta/histórico e como base de cálculo do valor
// total da OS.
public class OrdemServicoServico : Entity
{
    public Guid Id { get; private set; }
    public Guid IdOrdemServico { get; private set; }
    public Guid IdServico { get; private set; }
    public string NomeServico { get; private set; }
    public decimal Valor { get; private set; }

    public OrdemServicoServico(Guid idOrdemServico, Guid idServico, string nomeServico, decimal valor)
    {
        Id = Guid.NewGuid();
        IdOrdemServico = idOrdemServico;
        IdServico = idServico;
        NomeServico = nomeServico;
        Valor = valor;
    }
}
