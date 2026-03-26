# Logistica, Envio e Devolucao

## Objetivo
Definir operacao de fulfillment e pos-entrega para reduzir atrito e aumentar confianca.

## Estado atual
- Checkout nao captura endereco.
- Nao ha modulo de frete nem tracking.
- Devolucao/troca nao operacionalizadas em fluxo.

## Escopo minimo (P0/P1)

### Endereco
- Captura de endereco no checkout.
- Validacao de campos obrigatorios.

### Frete
- Regra inicial por faixa (ex.: gratis acima de X).
- Prazo estimado por regiao.

### Expedicao
- Estado de pedido com marco de envio.
- Codigo de tracking associado ao pedido.

### Devolucao
- Janela de devolucao definida.
- Fluxo padrao de aprovacao/reembolso.

## Politica operacional sugerida
- SLA de expedicao: pedido pago ate horario limite sai no mesmo dia util.
- SLA de resposta suporte: ate 24h util.
- Reembolso: ate X dias uteis apos recebimento/devolucao.

## Dados a modelar futuramente
- Address
- Shipment (carrier, tracking code, shippedAt)
- ReturnRequest (reason, status, refund status)

## KPI logisticos
- On-time shipment rate.
- Tempo medio de entrega.
- Taxa de devolucao por categoria.

## Relacoes
- [[checkout_pagamentos_estrategia]]
- [[atendimento_pos_venda]]
- [[kpis_dashboard_negocio]]
