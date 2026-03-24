# Points / quick notes

## 2026-03-24
- UI refresh completed:
  - homepage featured products curated to formal set (4 items),
  - neutral white/gray accent palette replacing warm gold/yellow tones,
  - subtle radius polish on images and key controls.

- Admin dashboard added:
  - KPI summary, sales/status/top-product visual blocks, quick management links, recent orders table.
  - kept existing Admin / Reports / Orders flows and navigation.

- Validation snapshot:
  - `dotnet build ./MafiaStore.csproj` passed.
  - `dotnet test ./Hype-Cartel.sln` currently fails because `Tests/IntegrationTests/IntegrationTests.csproj` is referenced in solution but missing on disk.

- Follow-up suggestion:
  - restore the missing integration test project or remove stale `.sln` entry to recover full solution test execution.

- PLAN mode (dashboard consolidation) decisions confirmed:
  - phase 1 will prioritize unified admin dashboard first; SQL Server migration deferred to a later phase.
  - old admin pages should be replaced in this phase (not just redirected).
  - user management scope: list + search + role switch (Admin/Customer) + activate/deactivate.
  - stock model for this phase: global stock per product (not by size variant).

- Unified dashboard implementation completed:
  - `Admin/Dashboard` now centralizes products (price + stock), categories, orders, users, and reports.
  - legacy `Admin/Produtos`, `Reports/*`, `OrdersAdmin/*` pages were replaced by dashboard-first flow (controllers now redirect).
  - user management added in dashboard: search, role update (Admin/Customer), activate/deactivate.
  - product stock is now editable in admin and persisted via EF/SQLite.

- Validation snapshot after implementation:
  - `dotnet build ./MafiaStore.csproj` ✅
  - `dotnet test ./MafiaStore.csproj` ✅
  - `dotnet test ./Hype-Cartel.sln` ✅ (stale missing test project reference removed from solution).

- Hotfix dashboard (SQLite SUM decimal):
  - bug reported: `Admin/Dashboard` crashing with `NotSupportedException` due to `Sum` over `decimal` in SQLite provider.
  - fix applied in `AdminController.Dashboard`:
    - revenue aggregations now sum as `double?` in SQL and convert back to rounded `decimal` for UI.
    - monthly revenue query now keeps "last 12 months" semantics (take newest first, then reorder ascending for display).
  - post-fix validation:
    - `dotnet build ./MafiaStore.csproj` ✅
    - `dotnet test ./MafiaStore.csproj` ✅
    - `dotnet test ./Hype-Cartel.sln` ✅
    - smoke test: admin login + `GET /Admin/Dashboard` returns `200` and no `SQLite Sum decimal` exception in response.

## 2026-03-24 (Obsidian expansion only - no app code changes)
- Pedido atual focado em documentacao completa no `Memory/` como reserva de contexto e planejamento.
- Estrutura expandida com docs por dominio:
  - estrategia/produto,
  - arquitetura/dados,
  - seguranca/compliance,
  - operacao ecommerce,
  - qualidade/release,
  - growth/SEO/analytics.
- Novos docs de analise transversal adicionados:
  - `gaps_producao_readiness.md`
  - `matriz_riscos_dependencias.md`
- `memory_index.md` atualizado com navegacao completa por dominio.
- Referencias externas usadas para orientar planos:
  - Google Ecommerce SEO docs,
  - Google SEO Starter Guide,
  - Stripe Checkout docs,
  - OWASP Top 10.
- Restricao respeitada: nenhuma alteracao em codigo da aplicacao nesta etapa.
