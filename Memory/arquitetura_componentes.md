# Arquitetura de Componentes - Estado Atual

## Visao geral
Arquitetura monolitica MVC com separacao por camadas:
- Presentation: Controllers + Views Razor.
- Application: Services (catalogo, carrinho, checkout, utilizadores).
- Data: EF Core DbContext + migrations.
- Identity: ASP.NET Identity com roles.

## Componentes principais

### 1) Web MVC
- `Controllers/HomeController.cs`
- `Controllers/ProdutosController.cs`
- `Controllers/CarrinhoController.cs`
- `Controllers/EncomendasController.cs`
- `Controllers/AccountController.cs`
- `Controllers/AdminController.cs`

Responsavel por rotas, validacao de input de entrada e composicao de view models.

### 2) Servicos de dominio
- `Services/ProductCatalogEfStore.cs`
- `Services/CartStore.cs`
- `Services/OrderService.cs`
- `Services/UserEfStore.cs`
- `Services/CartOwnerResolver.cs`

Responsavel por regras de negocio e persistencia atraves de interfaces.

### 3) Persistencia e identidade
- `Data/ApplicationDbContext.cs`
- `Data/IdentitySeedData.cs`
- `Data/LegacyJsonDataMigrator.cs`
- `Data/SeedData.cs`
- `Migrations/*.cs`

Responsavel por schema, estado do banco e bootstrap inicial.

### 4) Frontend Razor/CSS
- `Views/**`
- `wwwroot/css/site.css`
- `Views/Shared/_Layout.cshtml`

Responsavel pela experiencia visual e fluxo de navegacao.

## Fluxo tecnico de request (resumo)
1. Request chega ao controller.
2. Controller consulta servico (ou DbContext quando necessario).
3. Servico executa regra de negocio e persistencia.
4. Resultado e mapeado para ViewModel.
5. Razor renderiza resposta.

## Pontos fortes atuais
- Backoffice unificado em dashboard.
- Checkout com transacao e validacao de stock.
- Controle de acesso por role.
- Migrations e seeds automatizados no startup.

## Limites atuais
- Sem integracao de pagamento real.
- Sem observabilidade operacional madura.
- Sem suite de testes automatizada.
- Sem camada API separada para omnichannel.

## Direcao evolutiva recomendada
- Curto prazo: manter monolito e fechar gaps P0/P1.
- Medio prazo: extrair endpoints API para checkout/pedidos/admin reports.
- Longo prazo: separar BFF/API para web e possivel app.

## Relacoes
- [[schema_sql_er]]
- [[fluxos_criticos_negocio]]
- [[adrs_decisoes_tecnicas]]
