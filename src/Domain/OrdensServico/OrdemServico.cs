using Domain.Common;
using Domain.OrdensServico.Enums;
using Domain.OrdensServico.Events;
using Domain.OrdensServico.Produtos;
using Domain.OrdensServico.Servicos;
using DomainException = Domain.Common.Exceptions.DomainException;

namespace Domain.OrdensServico;

// Dono do ciclo de vida da OS e da orquestração da saga (Fase 4). Ao contrário do
// monolito de origem, o diagnóstico e a execução (item a item) não acontecem mais
// aqui — vivem no Execução Service (fila de execução, MongoDB). Este agregado
// guarda o cabeçalho, um snapshot dos itens identificados no diagnóstico e reage
// aos eventos assíncronos dos outros dois microsserviços (Billing/Execução) para
// avançar (ou compensar) a máquina de estados.
public class OrdemServico : Entity
{
    public Guid Id { get; private set; }
    public Guid IdCliente { get; private set; }
    public Guid IdVeiculo { get; private set; }
    public DateTime DataCriacao { get; private set; }
    public DateTime? DataInicioExecucao { get; private set; }
    public DateTime? DataFinalizacao { get; private set; }
    public StatusOrdemServico Status { get; private set; }
    public decimal ValorTotal { get; private set; }
    public List<OrdemServicoServico> Servicos { get; init; } = new();
    public List<OrdemServicoProduto> Produtos { get; init; } = new();

    public void Inserir(Guid idCliente, Guid idVeiculo)
    {
        Id = Guid.NewGuid();
        IdCliente = idCliente;
        IdVeiculo = idVeiculo;
        Status = StatusOrdemServico.Recebida;
        DataCriacao = DateTime.UtcNow;

        Raise(new OrdemServicoStatusAlteradoDomainEvent(Id, Status));
    }

    // Disparado pelo orquestrador da saga ao comandar "IniciarDiagnostico" no Execução Service.
    public void IniciarDiagnostico()
    {
        if (Status != StatusOrdemServico.Recebida)
        {
            throw new DomainException("O diagnóstico só pode ser iniciado após o recebimento da OS.");
        }

        Status = StatusOrdemServico.EmDiagnostico;

        Raise(new OrdemServicoStatusAlteradoDomainEvent(Id, Status));
    }

    // Consumido a partir do evento DiagnosticoFinalizado (Execução Service) — snapshot dos
    // itens identificados, mesma ideia de "congelar" nome/valor já usada no monolito de origem.
    public void RegistrarDiagnostico(List<OrdemServicoServico> servicos, List<OrdemServicoProduto> produtos)
    {
        if (Status != StatusOrdemServico.EmDiagnostico)
        {
            throw new DomainException("Só é possível registrar o diagnóstico enquanto a OS estiver em diagnóstico.");
        }

        if (servicos.Count == 0)
        {
            throw new DomainException("Não é possível concluir o diagnóstico sem serviços identificados.");
        }

        Servicos.AddRange(servicos);
        Produtos.AddRange(produtos);
        CalcularTotal();

        Status = StatusOrdemServico.AguardandoAprovacao;

        Raise(new OrdemServicoStatusAlteradoDomainEvent(Id, Status));
    }

    // Consumido a partir do evento PagamentoAprovado (Billing Service).
    public void AprovarPagamento()
    {
        if (Status != StatusOrdemServico.AguardandoAprovacao)
        {
            throw new DomainException("O pagamento não está aguardando aprovação.");
        }

        DataInicioExecucao = DateTime.UtcNow;
        Status = StatusOrdemServico.EmExecucao;

        Raise(new OrdemServicoStatusAlteradoDomainEvent(Id, Status));
    }

    // Compensação: pagamento recusado/expirado (Billing) ou veículo não atendível (Execução).
    public void Cancelar()
    {
        if (Status is StatusOrdemServico.Finalizada or StatusOrdemServico.Entregue or StatusOrdemServico.Cancelada)
        {
            throw new DomainException("Não é possível cancelar uma ordem de serviço já finalizada, entregue ou cancelada.");
        }

        Status = StatusOrdemServico.Cancelada;

        Raise(new OrdemServicoStatusAlteradoDomainEvent(Id, Status));
    }

    // Consumido a partir do evento ExecucaoFinalizada (Execução Service).
    public void Finalizar()
    {
        if (Status != StatusOrdemServico.EmExecucao)
        {
            throw new DomainException("Não é possível finalizar porque a ordem de serviço não está em execução.");
        }

        DataFinalizacao = DateTime.UtcNow;
        Status = StatusOrdemServico.Finalizada;

        Raise(new OrdemServicoFinalizadaDomainEvent(Id, Produtos));
        Raise(new OrdemServicoStatusAlteradoDomainEvent(Id, Status));
    }

    public void Entregar()
    {
        if (Status != StatusOrdemServico.Finalizada)
        {
            throw new DomainException("A entrega só pode ocorrer após a finalização de todos os serviços.");
        }

        Status = StatusOrdemServico.Entregue;

        Raise(new OrdemServicoStatusAlteradoDomainEvent(Id, Status));
    }

    private void CalcularTotal()
    {
        ValorTotal = Servicos.Sum(s => s.Valor) + Produtos.Sum(p => p.Subtotal);
    }
}
