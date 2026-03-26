# Ops — Operations, DB & QA

Summary
Essential operational notes to prepare the project for production: inventory/stock, logistics, readiness, KPIs, QA and DB notes. For full details see the archived files in _archive.

Production readiness (executive)
- Current status: partial readiness.
- P0 blockers: payment gateway, address/freight in checkout, automated tests, observability/health checks, security hardening (secrets rotation).
- Recommended immediate focus: payments, security, tests, observability.

Inventory & stock
- Current model: global Product.Stock.
- Checkout validates and decrements stock inside a transaction.
- Risks: no temporary reservation; possible oversell under high concurrency.
- Recommendations:
  - Low stock threshold: <= 5 units; operational alert <= 3 units.
  - Log manual adjustments with reason, operator and timestamp.
  - Consider per-variant stock (size/color) when required.
- KPIs: stockout rate, mean time to replenish, % orders impacted.

Logistics, shipping & returns
- Current: checkout does not capture address; no shipping/tracking module.
- Minimum scope: capture address, basic freight rules, shipment state + tracking, return workflow.
- Model suggestions: Address, Shipment (carrier, tracking), ReturnRequest.

QA & tests
- Critical E2E scenarios: catalog -> PDP -> add to cart -> checkout (success/fail), admin flows (edit product, change order state), login/account flows.
- Automation: cover critical E2E in first sprints; CI pipeline: smoke + critical regression.
- Test environment: ephemeral SQLite DB per run with seeded data.

DB & business rules (summary)
- Main entities: Category, Product, Cart, CartItem, Order, OrderLine, OrderHistory, Identity tables.
- Checkout: validates cart, checks stock, decrements stock and creates order in a transaction. No RowVersion currently (concurrency risk).
- Dev DB: SQLite (mafia_store_dev.db). For high-concurrency production, consider PostgreSQL/SQL Server.

Reference (archived)
- _archive/gaps_producao_readiness.md
- _archive/inventario_stock_operacao.md
- _archive/logistica_envio_devolucao.md
- _archive/kpis_dashboard_negocio.md
- _archive/matriz_testes_e2e.md
- _archive/qa_checklist_funcional.md
- _archive/schema_sql_er.md
- _archive/DB_HowItWorks.md
- _archive/SQL_and_BusinessRules.md

Immediate priorities (P0)
1. Integrate a payment gateway (or use a staged mock).
2. Capture address and add simple freight + tracking.
3. Implement critical automated tests in CI (E2E for purchase).
4. Add health checks and basic observability/logging.
5. Define and enforce secret management and rotation.
6. Mitigate stock concurrency (RowVersion or atomic DB strategy).

Keep this file brief and link to archived details.
