# Matriz de Testes E2E

## Objetivo
Definir cenarios end-to-end de alto risco para automacao progressiva.

## Convencao
- Prioridade:
  - Alta (bloqueador de release)
  - Media
  - Baixa

## Cenários

| ID | Cenario | Prioridade | Resultado esperado |
|---|---|---|---|
| E2E-001 | Navegar home -> catalogo -> PDP | Alta | paginas renderizam sem erro e com dados consistentes |
| E2E-002 | Adicionar item no carrinho pela PDP | Alta | item aparece no carrinho com tamanho correto |
| E2E-003 | Alterar quantidade no carrinho | Alta | subtotal e total atualizam corretamente |
| E2E-004 | Checkout com stock suficiente | Alta | pedido criado e carrinho limpo |
| E2E-005 | Checkout com stock insuficiente | Alta | pedido nao criado e erro exibido |
| E2E-006 | Login customer + visualizar encomendas | Alta | apenas pedidos proprios visiveis |
| E2E-007 | Login admin + editar preco/stock produto | Alta | alteracao persiste no banco |
| E2E-008 | Admin altera estado do pedido | Alta | historico de estado gravado |
| E2E-009 | Admin tenta remover ultimo admin | Alta | operacao bloqueada com feedback |
| E2E-010 | Dashboard reports carrega | Media | graficos/tabelas sem excecao runtime |
| E2E-011 | Fluxo logout e bloqueio de rota admin | Media | rota admin redireciona login |
| E2E-012 | Busca catalogo com termo invalido | Baixa | estado vazio amigavel sem erro |

## Estrategia de automacao
- Sprint 1:
  - automatizar E2E-002, 004, 005, 007, 008, 009.
- Sprint 2:
  - cobrir E2E restantes.

## Ambiente de teste sugerido
- DB SQLite efemera por run.
- Dados seed controlados para previsibilidade.
- Pipeline CI executando smoke + regressao critica.

## Relacoes
- [[qa_checklist_funcional]]
- [[plano_releases]]
- [[matriz_requisitos_rastreabilidade]]
