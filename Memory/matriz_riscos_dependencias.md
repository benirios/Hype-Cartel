# Matriz de Riscos e Dependencias

## Objetivo
Visibilizar riscos operacionais e tecnicos, com mitigacao e dependencias entre frentes.

## Riscos principais

| ID | Risco | Impacto | Probabilidade | Mitigacao | Owner sugerido |
|---|---|---|---|---|---|
| R-01 | Checkout sem pagamento real | Alto | Alto | Integrar gateway + webhook idempotente | Produto/Eng |
| R-02 | Regressao sem testes automatizados | Alto | Alto | Suite minima unit + integracao + smoke | Eng |
| R-03 | Falta de observabilidade | Alto | Medio | Logs estruturados + health + alertas | Eng/Ops |
| R-04 | Ataques de forca bruta em login | Alto | Medio | rate limit + lockout + auditoria | Seguranca/Eng |
| R-05 | Vazamento de segredo | Alto | Medio | user-secrets + secret manager + rotacao | Eng/Ops |
| R-06 | Erro operacional admin sem trilha | Medio | Medio | auditoria de acao admin | Eng |
| R-07 | Ruptura de stock em alta demanda | Medio | Medio | alertas de stock + processo de reposicao | Operacao |
| R-08 | Baixa indexacao organica | Medio | Alto | SEO tecnico + Search Console | Growth |

## Dependencias entre frentes
- Pagamento real depende de:
  - gestao de segredos,
  - seguranca de webhook,
  - observabilidade basica.
- Frete/endereco depende de:
  - consolidacao do checkout.
- CRO depende de:
  - analytics/eventos,
  - estabilidade do funil.
- SEO tecnico depende de:
  - arquitetura de URLs e templates.

## Criticidade para release
- Nao liberar producao sem mitigacao minima de R-01, R-02, R-03 e R-04.

## Relacoes
- [[gaps_producao_readiness]]
- [[backlog_priorizado]]
- [[plano_releases]]
