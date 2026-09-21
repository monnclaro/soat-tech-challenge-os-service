# language: pt-BR
Funcionalidade: Saga da Ordem de Serviço
  Como orquestrador da saga entre OS Service, Billing Service e Execução Service,
  quero avançar o status da ordem de serviço a cada evento recebido dos outros
  microsserviços, para que o fluxo completo de atendimento seja concluído.

  Cenário: Fluxo completo de aprovação e execução
    Dado uma ordem de serviço aberta para um cliente e veículo
    Quando o diagnóstico é registrado com serviços e produtos
    E o orçamento é gerado e o pagamento é aprovado
    E a execução dos serviços é finalizada
    Então a ordem de serviço deve estar com status "Finalizada"

  Cenário: Compensação quando o pagamento é recusado
    Dado uma ordem de serviço aberta para um cliente e veículo
    Quando o diagnóstico é registrado com serviços e produtos
    E o pagamento é recusado
    Então a ordem de serviço deve estar com status "Cancelada"
