# ADRs - Decisoes Tecnicas

## ADR-001 Persistencia principal em EF Core
- Status: Aceito
- Contexto: projeto nasceu com legado JSON e precisou evoluir para consistencia transacional.
- Decisao: usar `ApplicationDbContext` com EF Core e migracoes versionadas.
- Consequencias:
  - ganho de integridade e transacoes,
  - maior controle de schema,
  - necessidade de governanca de migrations.

## ADR-002 Identity para auth/roles
- Status: Aceito
- Contexto: modelo manual de auth nao suportava requisitos de seguranca e roles.
- Decisao: adotar ASP.NET Identity com roles Admin/Customer.
- Consequencias:
  - auth padronizada e robusta,
  - seeds necessarios para ambiente dev,
  - ainda faltam hardenings (2FA/rate-limit/reset real).

## ADR-003 Dashboard admin unificada
- Status: Aceito
- Contexto: areas admin/reports/orders fragmentadas.
- Decisao: consolidar em `Admin/Dashboard` com abas.
- Consequencias:
  - operacao centralizada,
  - menor friccao de navegacao,
  - controller mais extenso exigindo disciplina de manutencao.

## ADR-004 Checkout transacional sem gateway (fase atual)
- Status: Aceito temporario
- Contexto: necessidade de garantir stock e consistencia de pedido.
- Decisao: manter `OrderService` com transacao e estado Pending, sem pagamento real nesta etapa.
- Consequencias:
  - fluxo funcional para validacao academica/MVP,
  - bloqueio para producao real ate integrar pagamentos.

## ADR-005 SQLite no dev
- Status: Aceito
- Contexto: setup rapido local e baixo custo operacional.
- Decisao: usar SQLite em dev (`mafia_store_dev.db`) com possibilidade de SQL Server posterior.
- Consequencias:
  - onboarding facil,
  - atencao a limites do provider (ex.: sum decimal no SQLite), mitigado no dashboard.

## ADR-006 Sem suite de testes versionada no estado atual
- Status: Risco aceito temporario
- Contexto: repositorio atual sem pasta `Tests` no workspace.
- Decisao: documentar como gap critico P0.
- Consequencias:
  - risco de regressao alto,
  - prioridade imediata no backlog.

## ADRs propostas (a validar em implementacao futura)
- ADR-P01: Stripe Checkout como gateway inicial.
- ADR-P02: logging estruturado (Serilog) + health checks.
- ADR-P03: sitemap/canonical/schema e governanca SEO tecnica.

## Relacoes
- [[arquitetura_componentes]]
- [[backlog_priorizado]]
- [[gaps_producao_readiness]]
