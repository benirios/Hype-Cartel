# Runbook de Deploy

## Objetivo
Padronizar deploy para reduzir risco operacional.

## Pre-deploy
- Confirmar branch/tag correto.
- Executar build:
  - `dotnet build ./MafiaStore.csproj`
- Executar testes disponiveis:
  - `dotnet test ./MafiaStore.csproj`
  - `dotnet test ./Hype-Cartel.sln` (quando aplicavel)
- Validar migracoes pendentes.

## Deploy (alto nivel)
1. Backup do banco de producao.
2. Publicar artefato da versao.
3. Aplicar migracoes.
4. Reiniciar aplicacao.
5. Rodar smoke checks.

## Smoke checks obrigatorios
- `GET /` responde 200.
- `GET /Produtos` responde 200.
- Login customer/admin funcional.
- Acesso admin dashboard funcional.
- Fluxo de carrinho basico funcional.

## Criterios de sucesso
- Aplicacao no ar sem erro critico.
- Sem regressao no fluxo de compra e admin.
- Sem aumento anormal de erro nos logs.

## Rollback rapido
1. Voltar artefato para versao anterior.
2. Restaurar backup se migracao quebrar compatibilidade.
3. Validar endpoints criticos.
4. Abrir incidente formal.

## Relacoes
- [[plano_releases]]
- [[runbook_incidentes]]
- [[qa_checklist_funcional]]
