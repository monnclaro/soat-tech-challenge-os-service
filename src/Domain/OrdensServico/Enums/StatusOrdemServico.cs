namespace Domain.OrdensServico.Enums;

public enum StatusOrdemServico
{
    Recebida = 0,
    EmDiagnostico = 1,
    AguardandoAprovacao = 2,
    EmExecucao = 3,
    Finalizada = 4,
    Entregue = 5,

    // Novo na Fase 4 (saga) — compensação: pagamento recusado/expirado (Billing Service)
    // ou veículo não atendível durante o diagnóstico (Execução Service).
    Cancelada = 6
}
