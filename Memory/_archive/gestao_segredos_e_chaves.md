# Gestao de Segredos e Chaves

## Objetivo
Evitar exposicao de credenciais e padronizar rotacao de segredos.

## Estado atual
- `appsettings.json` e `appsettings.Development.json` possuem connection string SQLite local.
- Nao ha chave de pagamento integrada ainda.
- Nao ha processo formal de rotacao documentado.

## Politica alvo

### Armazenamento
- Desenvolvimento:
  - usar `dotnet user-secrets` para segredos nao versionados.
- Producao:
  - usar secret manager do provedor (ex.: Azure Key Vault, AWS Secrets Manager).

### Nao permitido
- Credenciais reais em:
  - codigo fonte,
  - arquivos versionados,
  - docs publicas.

### Rotacao
- API keys de pagamento: rotacao trimestral ou imediata em incidente.
- Segredos de ambiente: rotacao semestral.
- Credenciais admin bootstrap: reset no primeiro deploy.

## Inventario minimo de segredos
- Chaves de pagamento (publishable e secret).
- String de conexao de producao.
- Credenciais SMTP/servicos externos.
- Chaves de observabilidade (quando aplicavel).

## Processo em caso de vazamento
1. Revogar segredo comprometido.
2. Rotacionar e redistribuir novo segredo.
3. Auditar logs para uso indevido.
4. Registrar incidente em [[runbook_incidentes]].

## Checklist rapido
- [ ] Nenhum segredo real em markdown de Memory.
- [ ] Nenhum segredo real em repo git.
- [ ] Ambiente local com user-secrets quando necessario.
- [ ] Plano de rotacao definido por owner.

## Relacoes
- [[seguranca_owasp_checklist]]
- [[checkout_pagamentos_estrategia]]
- [[runbook_incidentes]]
