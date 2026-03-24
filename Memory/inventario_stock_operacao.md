# Inventario e Stock - Operacao

## Objetivo
Reduzir ruptura e sobre-venda com processos simples e auditaveis.

## Estado atual
- Stock global por produto (`Product.Stock`).
- Checkout valida stock e decrementa em transacao (`OrderService`).
- Cancelamento de pedido pago/enviado repoe stock (dashboard admin).

## Riscos atuais
- Sem reserva de stock por tempo durante checkout.
- Sem alerta automatico por limiar.
- Sem inventario por variacao de tamanho.

## Politica operacional proposta

### Limiar e alertas
- Low stock: <= 5 unidades (ja usado em KPI).
- Definir alerta operacional para <= 3 unidades.

### Ciclo de reposicao
1. Relatorio semanal de baixa de stock.
2. Lista priorizada por giro e margem.
3. Reposicao com data prevista.

### Ajustes manuais
- Todo ajuste manual deve registrar:
  - motivo,
  - operador,
  - data/hora.

## Evolucao de modelo de stock
- Fase atual: stock global (manter).
- Fase futura: stock por variacao (tamanho/cor) para reduzir erro operacional.

## Indicadores minimos
- Taxa de ruptura por categoria.
- Tempo medio de reposicao.
- Percentual de pedidos impactados por indisponibilidade.

## Relacoes
- [[fluxos_criticos_negocio]]
- [[kpis_dashboard_negocio]]
- [[backlog_priorizado]]
