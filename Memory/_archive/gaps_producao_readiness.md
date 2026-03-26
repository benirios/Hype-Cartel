# Gaps de Producao Readiness

## Objetivo
Consolidar os gaps mais relevantes para transformar o projeto em ecommerce pronto para operacao real.

## Leitura executiva
- Estado atual: base MVP forte em catalogo/admin e persistencia.
- Readiness de producao: parcial.
- Bloqueadores: pagamento real, observabilidade, seguranca hardening e testes automatizados.

## Score por dominio (estimativa pragmatica)
- Catalogo e navegacao: Medio/Alto.
- Checkout e pagamentos: Baixo/Medio.
- Admin e operacao interna: Medio.
- Seguranca e compliance: Baixo.
- Qualidade e testes: Baixo.
- SEO e growth: Baixo/Medio.

## Top 10 gaps prioritarios
1. Sem gateway de pagamento integrado.
2. Sem endereco/frete no checkout.
3. Sem testes automatizados versionados no repo atual.
4. Sem observabilidade estruturada e health checks.
5. Sem rate-limit e lockout robusto em login.
6. Sem plano formal de segredos e rotacao.
7. Sem trilha de auditoria admin completa.
8. Sem export de relatorios operacionais.
9. Sem SEO tecnico completo (sitemap/robots/canonical/schema).
10. Sem processo de release/runbook maduro executado continuamente.

## Gaps que ja tem base pronta para evoluir
- Checkout transacional e validacao de stock ja existem.
- Dashboard admin unificada ja existe.
- Identity e roles ja existem.
- Migrations e schema relacional ja existem.

## Recomendacao de ataque
- Fechar P0 primeiro (pagamento, seguranca, testes, observabilidade).
- Avancar para P1 operacional (frete, notificacoes, export, SEO tecnico).
- Escalar com P2 growth (CRO, CRM, conteudo e analytics avancado).

## Relacoes
- [[backlog_priorizado]]
- [[roadmap_produto]]
- [[matriz_riscos_dependencias]]
