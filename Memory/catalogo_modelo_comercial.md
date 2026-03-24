# Catalogo e Modelo Comercial

## Objetivo
Definir como o catalogo deve evoluir para suportar estrategia comercial sustentavel.

## Estado atual
- Produtos com:
  - preco unico,
  - stock global por produto,
  - tamanhos em JSON,
  - categoria e destaque.
- Gestao via dashboard admin.

## Diretrizes de modelo comercial

### Estrutura de sortido
- Core permanente:
  - itens evergreen de conversao previsivel.
- Drops sazonais:
  - lotes limitados para gerar escassez.
- Capsulas tematicas:
  - colecoes com narrativa de marca.

### Regras de preco
- Faixa premium coerente com posicionamento.
- Evitar desconto continuo para nao deteriorar valor de marca.
- Descontos apenas por campanha com janela definida.

### Margem e mix
- Acompanhar margem por categoria e por SKU.
- Priorizar categorias com melhor equilibrio margem x conversao.

## Requisitos de dados para evolucao
- Custo por SKU (ainda nao modelado).
- Margem estimada por produto (ainda nao modelado).
- Politica de preco por campanha (ainda nao modelado).

## Checklist de governanca de catalogo
- [ ] Nome e descricao consistentes em tom editorial.
- [ ] Imagem principal de alta qualidade.
- [ ] Categoria correta.
- [ ] Stock coerente com plano comercial.
- [ ] Produto apto para SEO (slug, metadata futura).

## Relacoes
- [[inventario_stock_operacao]]
- [[plano_marketing_conteudo]]
- [[kpis_dashboard_negocio]]
