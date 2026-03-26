# Roadmap Produto - Hype Cartel

## Objetivo
Transformar o projeto atual em uma loja online pronta para operacao real, mantendo a estetica premium dark/minimalista ja existente.

## Estado atual resumido
- Base funcional: catalogo, carrinho, checkout transacional sem gateway real, historico de encomendas, dashboard admin unificada.
- Base tecnica: ASP.NET Core MVC + EF Core + Identity + SQLite (dev), com migracoes aplicadas.
- Gap principal: operacao real de ecommerce (pagamento, logistica, compliance, observabilidade, QA automatizado, growth).

## Norte do produto
- North Star Metric: Taxa de Conversao Checkout Concluido.
- Metas de suporte:
  - reduzir abandono de checkout,
  - reduzir tempo de resolucao de tickets,
  - elevar receita por pedido (AOV),
  - manter taxa de erro operacional baixa.

## Roadmap por fases (sem alterar codigo nesta fase de planejamento)

### Fase P0 - Fundacao de producao (bloqueadores)
- Integrar gateway de pagamento real (Stripe Checkout + webhook).
- Implementar observabilidade minima (logs estruturados + health checks + alertas basicos).
- Endurecer seguranca essencial (rate limit login, politicas de segredo, checklist OWASP).
- Estruturar suite minima de testes (unitarios + integracao + smoke).

### Fase P1 - Operacao ecommerce completa
- Enderecos de entrega, calculo de frete e tracking.
- Politica de devolucao/troca implementada no fluxo.
- Notificacoes transacionais por email (pedido, pagamento, envio, cancelamento).
- Exportacao de relatorios (CSV/PDF) e auditoria operacional.

### Fase P2 - Crescimento e otimizacao
- SEO tecnico completo (sitemap, robots, canonical, schema.org Product/Breadcrumb).
- Analytics e funil de conversao (GA4/GTM + eventos).
- CRO continuo (A/B tests de PDP/cart/checkout).
- CRM e marketing de conteudo com automacoes.

## Guardrails de UX e marca
- Manter identidade visual escura, elegante e editorial.
- Evitar elementos "promo agressivos" que quebrem a proposta premium.
- Priorizar performance visual e legibilidade em mobile.

## Criterios de saida por fase
- P0: checkout com pagamento real + logs + testes minimos + seguranca essencial.
- P1: operacao ponta-a-ponta (pagamento -> envio -> pos-venda) auditavel.
- P2: crescimento medido por KPI de conversao, SEO e retencao.

## Referencias relacionadas
- [[backlog_priorizado]]
- [[matriz_requisitos_rastreabilidade]]
- [[gaps_producao_readiness]]
- [[matriz_riscos_dependencias]]
