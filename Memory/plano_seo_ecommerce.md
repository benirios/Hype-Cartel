# Plano SEO Ecommerce

## Objetivo
Melhorar descoberta organica e qualidade de indexacao para paginas de produto e categoria.

## Referencias externas
- Google ecommerce SEO docs:
  - https://developers.google.com/search/docs/specialty/ecommerce
- Google SEO Starter Guide:
  - https://developers.google.com/search/docs/fundamentals/seo-starter-guide

## Estado atual
- Meta description global no layout.
- URLs de produto por id (`/Produtos/Detalhes/{id}`).
- Sem sitemap.xml e sem robots.txt dedicado.
- Sem canonical e sem schema estruturado.

## Plano por etapas

### Etapa 1 (P1)
- sitemap.xml dinamico.
- robots.txt com bloqueio de rotas administrativas.
- URLs amigaveis por slug.

### Etapa 2 (P1/P2)
- canonical por pagina de produto/categoria.
- dados estruturados JSON-LD:
  - Product
  - BreadcrumbList
  - Organization
- breadcrumbs semanticamente corretos.

### Etapa 3 (P2)
- monitorizacao Search Console:
  - cobertura de indexacao,
  - CTR por pagina,
  - query opportunities.

## Boas praticas prioritarias
- Conteudo unico por PDP (descricao rica e util).
- Estrutura de links internos clara entre home -> categoria -> PDP.
- Evitar duplicacao de URL para mesmo conteudo.

## KPI SEO
- Impressao e clique organico por categoria/PDP.
- CTR organico medio.
- Paginas indexadas validas.
- Receita organica atribuida.

## Relacoes
- [[plano_marketing_conteudo]]
- [[kpis_dashboard_negocio]]
- [[roadmap_produto]]
