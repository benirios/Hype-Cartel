# Autenticacao e Autorizacao - Politicas

## Objetivo
Definir politicas de acesso para clientes e administradores com foco em seguranca e rastreabilidade.

## Estado atual observado
- Auth via ASP.NET Identity (`Program.cs`, `AccountController.cs`).
- Roles base:
  - Admin
  - Customer
- Cookies:
  - nome custom `HypeCartel.Auth`,
  - HttpOnly,
  - expiracao 12h com sliding expiration.

## Politicas de autenticacao (alvo)

### Login
- Bloquear brute force:
  - max 5 tentativas falhas / 10 minutos por conta e por IP.
- Exigir lockout temporario automatico apos limite.
- Registrar evento de login com:
  - user id,
  - timestamp UTC,
  - sucesso/falha,
  - origem (IP/UserAgent reduzido).

### Senha e recuperacao
- Politica de senha forte (ja parcialmente coberta pelo Identity).
- Reset de password via token de uso unico e expiracao curta.
- Invalidar sessoes em reset de password.

### Sessao
- Renovacao por sliding expiration controlada.
- Logout explicito em todas as sessoes (quando funcionalidade for adicionada).

## Politicas de autorizacao (alvo)

### Cliente
- Pode ver/editar apenas dados proprios.
- Pode consultar apenas encomendas proprias.

### Admin
- Pode gerir catalogo, categorias, encomendas e users na dashboard.
- Nao pode remover o ultimo admin ativo.
- Nao pode desativar a propria conta (regra ja aplicada no codigo).

### Evolucao recomendada
- Introduzir papeis:
  - AdminOperacao (catalogo/pedidos),
  - AdminGestao (users/roles),
  - SuperAdmin (controle total).

## Matriz de permissoes alvo (resumo)
- Produto CRUD: AdminOperacao, AdminGestao, SuperAdmin.
- Categoria CRUD: AdminOperacao, SuperAdmin.
- Pedido status: AdminOperacao, SuperAdmin.
- User role/active: AdminGestao, SuperAdmin.
- Configuracoes sensiveis: SuperAdmin.

## Eventos que devem ser auditados
- Login falho repetido.
- Promocao/rebaixamento de role.
- Ativar/desativar conta.
- Alteracao de estado de pedido.

## Relacoes
- [[seguranca_owasp_checklist]]
- [[runbook_incidentes]]
- [[matriz_requisitos_rastreabilidade]]
