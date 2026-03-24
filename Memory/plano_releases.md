# Plano de Releases

## Objetivo
Definir um fluxo simples, repetivel e seguro para publicar novas versoes.

## Cadencia sugerida
- Release regular: semanal ou quinzenal.
- Hotfix: sob demanda (com post-mortem obrigatorio).

## Tipos de release
- Patch: bugfix sem alteracao de contrato.
- Minor: nova funcionalidade compativel.
- Major: mudanca quebrando comportamento/contrato.

## Fluxo proposto
1. Congelamento de escopo.
2. Checklist QA funcional completo.
3. Build e testes automatizados.
4. Deploy em staging.
5. Smoke test de fluxos criticos.
6. Deploy em producao com monitorizacao reforcada.
7. Verificacao pos-release.

## Gate de qualidade minimo
- Sem falha em build.
- Sem falha em testes criticos.
- Sem bug bloqueador aberto para fluxo de compra.

## Plano de rollback
- Reverter para versao anterior estavel.
- Executar validacoes minimas de saude.
- Registrar incidente e acao corretiva.

## Checklist pos-release
- [ ] Home, catalogo, carrinho e checkout responsivos.
- [ ] Dashboard admin acessivel.
- [ ] KPIs principais dentro do esperado.
- [ ] Logs sem erro critico anormal.

## Relacoes
- [[qa_checklist_funcional]]
- [[runbook_deploy]]
- [[runbook_incidentes]]
