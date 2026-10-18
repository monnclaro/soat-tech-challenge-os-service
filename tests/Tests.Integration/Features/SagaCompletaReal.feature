# language: pt-BR
Funcionalidade: Saga completa entre os 3 microsserviços reais
  Como orquestrador da saga entre OS Service, Billing Service e Execução Service,
  quero que o fluxo completo de atendimento funcione de ponta a ponta subindo os
  três microsserviços de verdade (containers reais, RabbitMQ real, bancos reais),
  não só em processo com fakes, para ter confiança de que a saga funciona na
  integração real entre os serviços.

  Cenário: Fluxo completo de aprovação e execução com os 3 serviços no ar
    Dado que os 3 microsserviços estão no ar
    E uma ordem de serviço aberta para um cliente e veículo recém-cadastrados
    Quando o diagnóstico é iniciado e finalizado no Execução Service com um serviço e um produto
    Então a ordem de serviço deve avançar para o status "AguardandoAprovacao"
    Quando o pagamento é aprovado via webhook do Mercado Pago no Billing Service
    Então a ordem de serviço deve avançar para o status "AguardandoExecucao"
    Quando a execução do serviço é iniciada e finalizada no Execução Service
    Então a ordem de serviço deve avançar para o status "Finalizada"
