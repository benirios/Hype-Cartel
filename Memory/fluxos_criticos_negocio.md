# Fluxos Criticos de Negocio

## Objetivo
Mapear fluxos de alto impacto em receita e operacao com pontos de falha.

## FC-01 Compra (catalogo -> checkout)
1. Cliente navega em `Produtos/Index`.
2. Cliente abre `Produtos/Detalhes`.
3. Cliente adiciona ao carrinho (`Carrinho/Adicionar`).
4. Cliente ajusta quantidade (`Carrinho/AtualizarQuantidade`).
5. Cliente fecha pedido (`Carrinho/Checkout` -> `OrderService.CreateOrderAsync`).
6. Pedido criado com estado `Pending`.

### Controles atuais
- anti-forgery em POST,
- validacao de stock na criacao da encomenda,
- transacao DB para consistencia.

### Riscos atuais
- sem gateway de pagamento real,
- sem endereco/frete,
- sem notificacao transacional.

## FC-02 Operacao de pedido (admin)
1. Admin abre dashboard aba orders.
2. Seleciona encomenda.
3. Atualiza estado (`UpdateOrderStatus`).
4. Sistema grava `OrderHistory`.
5. Em cancelamento de Paid/Shipped, stock e reposto.

### Controles atuais
- validacao de transicao de estado,
- historico de mudanca.

### Riscos atuais
- sem integracao com transportadora,
- sem SLA/alerta para pedidos parados.

## FC-03 Gestao de catalogo e stock
1. Admin cria/edita/remove produto.
2. Dados persistem via `IProductCatalogService` (EF store atual).
3. Categoria e mantida sincronizada.

### Controles atuais
- validacao server-side de campos,
- bloqueio de delete de categoria com produtos.

### Riscos atuais
- sem trilha de auditoria admin detalhada,
- sem importacao massiva validada.

## FC-04 Gestao de utilizadores
1. Admin pesquisa user.
2. Troca role ou ativa/desativa.
3. Sistema previne remover/desativar ultimo admin ativo.

### Controles atuais
- lock de mutacao admin (`SemaphoreSlim`),
- checks de ultimo admin.

### Riscos atuais
- sem trilha de auditoria de seguranca,
- sem 2FA para contas admin.

## FC-05 Login e sessao
1. Utilizador autentica em `Account/Login`.
2. Identity cria cookie auth.
3. Acesso protegido por `[Authorize]`/`[Authorize(Roles="Admin")]`.

### Riscos atuais
- sem rate-limit em tentativas,
- sem fluxo de reset de password com email real.

## Priorizacao operacional
- FC-01 e FC-02 sao os fluxos de maior impacto e risco.
- FC-03 e FC-04 sustentam operacao interna e prevencao de erro humano.

## Relacoes
- [[backlog_priorizado]]
- [[qa_checklist_funcional]]
- [[runbook_incidentes]]
