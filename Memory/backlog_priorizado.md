# Backlog Priorizado - Hype Cartel

## Regras de priorizacao
- P0: bloqueia operacao real.
- P1: necessario para operacao madura.
- P2: aceleradores de crescimento.

## P0 - Bloqueadores de producao

### BG-001 Integracao de pagamento real
- Objetivo: aceitar pagamento real com conciliacao de estado.
- Escopo inicial: Stripe Checkout + webhook idempotente.
- Criterios de aceite:
  - checkout cria sessao de pagamento valida,
  - webhook atualiza pedido para Paid sem duplicacao,
  - falha de pagamento nao conclui encomenda.
- Dependencias: [[checkout_pagamentos_estrategia]], [[seguranca_owasp_checklist]].

### BG-002 Frete e endereco
- Objetivo: capturar endereco e calcular frete.
- Criterios de aceite:
  - checkout exige endereco valido,
  - total final inclui frete + IVA de forma consistente,
  - pedido grava dados minimos de envio.
- Dependencias: BG-001.

### BG-003 Seguranca minima operacional
- Objetivo: reduzir risco imediato (auth/abuso/segredos).
- Criterios de aceite:
  - rate limit em login,
  - politica de segredo sem credenciais em config versionada,
  - checklist OWASP P0 concluido.
- Dependencias: [[autenticacao_autorizacao_politicas]], [[gestao_segredos_e_chaves]].

### BG-004 Testes criticos automatizados
- Objetivo: confianca para deploy continuo.
- Criterios de aceite:
  - testes de checkout transacional,
  - testes de admin mutacoes principais,
  - smoke de login/dashboard em pipeline.
- Dependencias: nenhuma (deve correr em paralelo).

### BG-005 Observabilidade base
- Objetivo: detectar problemas antes de impactar receita.
- Criterios de aceite:
  - logs estruturados por request e erro,
  - endpoint de health check,
  - runbook de incidente com passos de rollback.
- Dependencias: [[runbook_incidentes]], [[kpis_dashboard_negocio]].

## P1 - Consolidacao de operacao ecommerce

### BG-101 Pos-venda e notificacoes
- Email transacional para estados do pedido.
- Historico auditavel de comunicacoes.

### BG-102 Logistica completa
- Tracking da expedicao.
- Politica de devolucao/troca operacionalizada.

### BG-103 Relatorios exportaveis
- Export CSV/PDF para pedidos, receita e top produtos.

### BG-104 SEO tecnico
- sitemap, robots, canonical, dados estruturados Product/Breadcrumb.

### BG-105 Governanca de acessos admin
- perfis admin com permissoes graduais (ex.: operador vs super-admin).

## P2 - Growth e vantagem competitiva

### BG-201 CRO continuo
- Testes A/B em PDP, carrinho e checkout.

### BG-202 CRM e retencao
- Newsletter com segmentos, recuperacao de abandono, campanhas de recompra.

### BG-203 Reputacao e prova social
- Reviews verificadas por compra.

### BG-204 Wishlist persistente
- Lista de desejos por utilizador autenticado.

## Notas de implementacao
- Cada item deve virar epics/issues com dono, definicao de pronto e plano de rollout.
- Antes de cada sprint, refletir mudancas em [[matriz_requisitos_rastreabilidade]].

## Relacoes
- [[roadmap_produto]]
- [[matriz_riscos_dependencias]]
- [[qa_checklist_funcional]]
