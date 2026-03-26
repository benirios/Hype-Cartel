# Runbook de Incidentes

## Objetivo
Dar resposta rapida e consistente para falhas de producao.

## Severidade
- Sev1: checkout indisponivel ou perda de receita em curso.
- Sev2: area admin critica indisponivel.
- Sev3: falha parcial sem bloqueio total.

## Fluxo de resposta
1. Detectar e classificar severidade.
2. Abrir incidente com responsavel e hora UTC.
3. Mitigar impacto imediato (feature flag/rollback/hotfix).
4. Comunicar status periodico.
5. Encerrar com post-mortem.

## Playbooks iniciais

### IN-001 Dashboard quebra em runtime
- Sintoma: excecao em `/Admin/Dashboard`.
- Acoes:
  - coletar stack trace,
  - validar query/agregacao recente,
  - aplicar hotfix seguro,
  - validar com smoke admin.
- Exemplo real: limitacao SQLite `Sum(decimal)` corrigida com agregacao em double e conversao para decimal.

### IN-002 Checkout falhando
- Sintoma: pedidos nao sao criados.
- Acoes:
  - validar saude do banco,
  - verificar erros em `OrderService`,
  - checar stock inconsistente,
  - acionar rollback se necessario.

### IN-003 Login indisponivel
- Sintoma: usuarios nao autenticam.
- Acoes:
  - validar Identity tables e conexao,
  - verificar expiracao de segredo/cert,
  - aplicar fallback operacional.

## Dados minimos no post-mortem
- Causa raiz.
- Linha do tempo.
- Impacto (usuarios/pedidos/receita).
- Correcao aplicada.
- Acao preventiva.

## Relacoes
- [[runbook_deploy]]
- [[seguranca_owasp_checklist]]
- [[kpis_dashboard_negocio]]
