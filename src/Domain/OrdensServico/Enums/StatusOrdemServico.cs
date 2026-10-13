namespace Domain.OrdensServico.Enums;

public enum StatusOrdemServico
{
    Recebida = 0,
    EmDiagnostico = 1,
    AguardandoAprovacao = 2,

    // Pagamento aprovado, mas nenhum serviço começou a ser executado ainda — a
    // execução em si é sempre ação manual no Execução Service (ver
    // OrdemServico.IniciarExecucao). Nunca otimista: só avança daqui quando o
    // Execução Service confirma via evento ExecucaoIniciada.
    AguardandoExecucao = 3,

    EmExecucao = 4,
    Finalizada = 5,
    Entregue = 6,

    // Novo na Fase 4 (saga) — compensação: pagamento recusado/expirado (Billing Service)
    // ou veículo não atendível durante o diagnóstico (Execução Service).
    Cancelada = 7
}
