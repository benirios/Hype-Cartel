# Checkout e Pagamentos - Estrategia

## Objetivo
Evoluir o checkout atual para pagamento real, confiavel e auditavel.

## Estado atual
- Checkout cria `Order` com estado `Pending` e valida stock.
- Nao existe captura de pagamento real.

## Referencia externa usada
- Stripe Checkout docs: https://stripe.com/docs/payments/checkout
- Ponto-chave: usar Checkout Sessions e webhook para reconciliacao confiavel.

## Estrategia recomendada (fase 1)

### Gateway inicial
- Stripe Checkout (hosted page) para reduzir complexidade inicial.

### Fluxo alvo
1. Carrinho valida itens e cria "order draft" (Pending).
2. Sistema cria Checkout Session no Stripe.
3. Cliente conclui pagamento no Stripe.
4. Webhook `checkout.session.completed` atualiza pedido para Paid.
5. Falhas/cancelamentos mantem estado consistente e rastreavel.

### Requisitos tecnicos obrigatorios
- Idempotencia no processamento de webhook.
- Validacao de assinatura de webhook.
- Reconciliacao diaria de pagamentos x pedidos.

## Estrategia futura
- adicionar metodos locais (ex.: Pix) com provedor confiavel,
- fallback de pagamento para disponibilidade.

## Campos de dados a incluir futuramente
- PaymentProvider
- PaymentIntent/SessionId
- PaymentStatus
- PaidAtUtc
- FailureReason

## Riscos e mitigacao
- Risco: pagamento duplicado por reenvio de webhook.
  - Mitigacao: chave idempotente e estado terminal protegido.
- Risco: divergencia entre valor cobrado e pedido.
  - Mitigacao: assinar valor no momento de criar sessao.

## Relacoes
- [[fluxos_criticos_negocio]]
- [[seguranca_owasp_checklist]]
- [[gestao_segredos_e_chaves]]
