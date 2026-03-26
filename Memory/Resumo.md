# Resumo: Base de Dados, DER, MVC e Regras de Negócio

Este documento resume, a partir do código-fonte, toda a configuração e uso da base de dados no projeto Hype-Cartel (MafiaStore), descrevendo o modelo de dados (DER), as regras de negócio implementadas, o fluxo MVC e os arquivos relevantes.

---

## 1. Visão geral rápida
- Aplicação: ASP.NET Core MVC (TargetFramework net10.0).
- ORM: Entity Framework Core (EF Core).
- Banco: SQLite (arquivos: `mafia_store_dev.db`, `mafia_store.db` — o ambiente define qual usar).
- Identity: ASP.NET Core Identity (IdentityDbContext<IdentityUser>) usado para autenticação/autorizações.
- Migrations: EF migrations (pasta `Migrations/`) aplicadas automaticamente em startup via `db.Database.Migrate()`.

---

## 2. Arquivos de configuração e DB físico
- `appsettings.json` — DefaultConnection: `Data Source=mafia_store_dev.db`.
- `appsettings.Development.json` — DefaultConnection: `Data Source=mafia_store.db`.
- Arquivos DB no repositório: `mafia_store_dev.db`, `mafia_store_dev.db-shm`, `mafia_store_dev.db-wal`.
- Startup (Program.cs) registra `ApplicationDbContext` com `UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"))`, executa `Database.Migrate()` e roda os seeders (`IdentitySeedData`, `LegacyJsonDataMigrator`, `SeedData`).

---

## 3. Migrations (visão)
Migrations presentes (nomes):
- `20260323131447_InitialCreate` — esquema inicial (produtos/categorias etc.).
- `20260323131755_AddIdentityTables` — tabelas do Identity (AspNetUsers, AspNetRoles, AspNetUserRoles, AspNetClaims etc.).
- `20260323202901_AddCartTables` — tabelas de carrinho (Carts, CartItems).
- `20260323205602_AddOrderStateManagement` — Orders, OrderLines, OrderHistory.

O `ApplicationDbContextModelSnapshot` contém o snapshot atual do modelo.

---

## 4. ApplicationDbContext (mapa rápido)
Classe: `MafiaStore.Data.ApplicationDbContext : IdentityDbContext<IdentityUser>`
- DbSets expostos:
  - Categories, Products, Orders, OrderLines, OrderHistory, Carts, CartItems
- Configurações em `OnModelCreating`:
  - Chaves primárias explicitadas para entidades de domínio.
  - Tamanhos máximos (MaxLength) e obrigatoriedade para várias colunas (Name, Slugs, Sku, UserId, Status, etc.).
  - Índices únicos: Product.Slug, Product.Sku, Category.Slug, Cart.OwnerKey.
  - Índice composto único em CartItem: (CartId, ProductId, Size).
  - Colunas decimais com precisão: Price, Subtotal, Vat, Total -> decimal(18,2).
  - Conversão de enum OrderStatus para string (EnumToStringConverter<OrderStatus>), com HasMaxLength(40).
  - Relacionamentos com comportamento de delete:
    - Product -> Category: FK CategoryId, OnDelete Restrict (não permite apagar categoria com produtos).
    - Order -> OrderLine: FK OrderId, OnDelete Cascade.
    - Order -> OrderHistory: FK OrderId, OnDelete Cascade.
    - Cart -> CartItem: FK CartId, OnDelete Cascade.

Observação: Orders.UserId é apenas uma string (UserId) com MaxLength(450); não há FK configurado para AspNetUsers no model builder.

---

## 5. Esquema das tabelas (Resumo / DDL-like)
Obs: tipos e restrições postas conforme `OnModelCreating` + inferência do EF/SQLite.

- Categories
  - Id INTEGER PK
  - Name VARCHAR(120) NOT NULL
  - Slug VARCHAR(160) NOT NULL UNIQUE

- Products
  - Id INTEGER PK
  - Name VARCHAR(180) NOT NULL
  - Slug VARCHAR(180) NOT NULL UNIQUE
  - Sku VARCHAR(60) NOT NULL UNIQUE
  - Price DECIMAL(18,2)
  - Description VARCHAR(4000) NOT NULL
  - ImageUrl VARCHAR(1000) NOT NULL
  - AdditionalImagesJson TEXT
  - SizesJson TEXT
  - Stock INTEGER
  - Highlight BOOLEAN
  - CategoryId INTEGER FK -> Categories(Id) ON DELETE RESTRICT

- Carts
  - Id INTEGER PK
  - OwnerKey VARCHAR(180) NOT NULL UNIQUE
  - UpdatedAtUtc DATETIME NOT NULL

- CartItems
  - Id INTEGER PK
  - CartId INTEGER FK -> Carts(Id) ON DELETE CASCADE
  - ProductId INTEGER
  - Size VARCHAR(30) NOT NULL
  - Quantity INTEGER NOT NULL
  - UNIQUE(CartId, ProductId, Size)

- Orders
  - Id INTEGER PK
  - UserId VARCHAR(450) NOT NULL (referencia lógica a AspNetUsers.Id, sem FK)
  - CreatedAtUtc DATETIME NOT NULL
  - Status VARCHAR(40) NOT NULL (enum armazenado como string)
  - Subtotal DECIMAL(18,2)
  - Vat DECIMAL(18,2)
  - Total DECIMAL(18,2)

- OrderLines
  - Id INTEGER PK
  - OrderId INTEGER FK -> Orders(Id) ON DELETE CASCADE
  - ProductId INTEGER
  - ProductName VARCHAR(180) NOT NULL
  - Size VARCHAR(30)
  - Quantity INTEGER
  - UnitPrice DECIMAL(18,2)

- OrderHistory
  - Id INTEGER PK
  - OrderId INTEGER FK -> Orders(Id) ON DELETE CASCADE
  - FromStatus VARCHAR(40) NOT NULL
  - ToStatus VARCHAR(40) NOT NULL
  - ChangedBy VARCHAR(256) NOT NULL
  - ChangedAtUtc DATETIME NOT NULL
  - INDEX(OrderId)

- Identity (padrão IdentityDbContext)
  - AspNetUsers (Id nvarchar(450) PK, UserName, Email, PasswordHash, ...)
  - AspNetRoles, AspNetUserRoles, AspNetUserClaims, AspNetRoleClaims, AspNetUserLogins, AspNetUserTokens

---

## 6. DER (texto / relacionamentos principais)
- Category (1) --- (N) Product
- Product (N) --- (0..N) OrderLine  (OrderLine guarda ProductId, mas **não** há FK explícita para Products)
- Order (1) --- (N) OrderLine  (FK OrderId, cascade)
- Order (1) --- (N) OrderHistory (FK OrderId, cascade)
- Cart (1) --- (N) CartItem (FK CartId, cascade)
- CartItem tem UNIQUE(CartId, ProductId, Size) — garante uma linha por combinação produto/tamanho no carrinho
- Orders.UserId referencia logicamente AspNetUsers.Id (sem FK). Isto facilita manter histórico mesmo que o user seja removido.

---

## 7. Regras de negócio implementadas e onde estão (resumo por área)
Arquivos-chave: `Program.cs`, `Data/ApplicationDbContext.cs`, `Data/IdentitySeedData.cs`, `Data/LegacyJsonDataMigrator.cs`, `Data/SeedData.cs`, `Services/*`, `Filters/CartCountActionFilter.cs`, `Controllers/*`.

Abaixo, regras implementadas no código (referência de arquivo entre parênteses):

- Cadastro e política de senha (Program.cs + Identity configuration):
  - Senha exige: Uppercase, Lowercase, Digit; comprimento mínimo 6; Non-alphanumeric NÃO exigido.
  - Cookie de autenticação: nome "HypeCartel.Auth", sliding expiration 12h, SameSite=Lax.

- Seed de identidades/usuários (Data/IdentitySeedData.cs):
  - Garante papéis `Admin` e `Customer` e dois usuários iniciais (`admin`, `cliente`) com senhas definidas.

- Migração de dados legados (Data/LegacyJsonDataMigrator.cs):
  - Se `Products` vazio, importa `Catalog_Assets/products.json` criando categorias automáticas (slug normalizado) e produtos usando o Id legado.
  - Se existir `context/users.json`, cria usuários com senha temporária `Temp@123` e atribui papéis.

- Regras de produto (Services/ProductCatalogEfStore.cs):
  - Validações na criação/atualização: Id>0, Nome obrigatório, Preço >=0, Stock >=0, Categoria obrigatória, Imagem e descrição obrigatórias.
  - Geração/normalização de Slug e Sku; Slug único; Sku tentado como `SKU-{id:000}` e, se em conflito, sufixo com GUID parcial.
  - Campos `SizesJson` e `AdditionalImagesJson` serializados como JSON em TEXT.
  - Cria categoria automaticamente se não existir (ResolveCategory).

- Regras de carrinho (Services/CartStore.cs + CartOwnerResolver.cs):
  - Chave do dono do carrinho (OwnerKey) é resolvida por `CartOwnerResolver`:
    - Se autenticado: `user:{userId}` (preferível) ou `user-name:{username}`.
    - Se anônimo: usa cookie `MafiaStore.CartOwner`; se não existe gera `anon:{GUID}` e persiste cookie.
  - `GetOrCreateCart` normaliza OwnerKey, cria carrinho se não existir.
  - `AddItem` normaliza tamanho (fallback para primeiro tamanho do produto ou "M") e incrementa `Quantity` se item já existe (mesmo CartId, ProductId, Size).
  - `UpdateQuantity`: se quantidade <= 0 remove o item; caso contrário atualiza.
  - `Clear` remove todos os itens do carrinho.
  - `GetCartCount` soma `Quantity` dos itens do carrinho.
  - Indíce único (CartId, ProductId, Size) evita duplicatas em nível de BD.

- Regras de encomenda/checkout (Services/OrderService.cs):
  - `CreateOrderAsync(userId, ownerKey)`:
    - Usa `normalizedUserId = userId ?? ownerKey` (se não autenticado, usa ownerKey como identificador do pedido).
    - Carrega carrinho e itens; valida carrinho não vazio.
    - Em transação verifica todos os produtos existem e possuem stock suficiente; valida quantidades >0.
    - Atualiza `product.Stock -= item.Quantity` (persiste no mesmo tx).
    - Calcula subtotal = sum(preço * quantidade), VAT = round(subtotal * 0.23, 2, AwayFromZero), total = subtotal + vat.
    - Cria `Order` (Status = Pending) + `OrderLine`s (grava ProductId, ProductName, UnitPrice, Size, Quantity).
    - Remove `CartItems` do carrinho e atualiza `cart.UpdatedAtUtc`.
    - Commit transação e retorna `CheckoutResult.Ok(order.Id)`; se qualquer violação, retorna `CheckoutResult.Fail(message)`.

- Controlo de UI (Filters/CartCountActionFilter.cs):
  - A cada execução de action, popula `ViewBag.CartCount` para exibir contagem no layout.

- User store (Services/UserEfStore.cs):
  - Usa UserManager do Identity para autenticar e gerenciar usuários.
  - `Authenticate` valida password, lê roles e devolve `UserAccount` sem expor hashes.
  - `CreateUser` valida entradas, verifica unicidade de username/email e cria usuário com role (default Customer).

---

## 8. Fluxos principais (simplificados)
- Inicialização: `dotnet run` -> `Program.cs` configura serviços + `db.Database.Migrate()` aplica migrations -> seeders rodam (identidade, migração de JSON legados, seed orders).

- Visualizar catálogos: `ProdutosController` usa `IProductCatalogService` (implementado por `ProductCatalogEfStore`) para listar/detalhar produtos.

- Adicionar ao carrinho: `CartOwnerResolver` determina ownerKey -> `CartStore.AddItem(ownerKey, produtoId, tamanho)` ajusta quantidade ou insere novo item.

- Checkout: `EncomendasController` chama `OrderService.CreateOrderAsync(userId, ownerKey)` -> validações + decremento de stock + criação de order e orderlines dentro de transação.

- Administração: `AdminController` / `OrdersAdminController` / `ReportsController` (existem controladores administrativos para gerir produtos/encomendas/relatórios).

---

## 9. Observações técnicas e sugestões (pontos importantes)
- `Orders.UserId` não é FK para `AspNetUsers`. Vantagem: historico permanece mesmo após remoção do user; desvantagem: sem integridade referencial automática.
- Campos JSON (`SizesJson`, `AdditionalImagesJson`) são TEXT serializados com System.Text.Json; isto facilita flexibilidade, mas torna consultas SQL complexas.
- Precisão decimal configurada (18,2) para valores monetários.
- Cálculo de VAT fixo: 23% com arredondamento `AwayFromZero` (implementado em OrderService).
- Concorrência: decremento de stock feito dentro de transação, mas não há token de concorrência explícito (poderá ocorrer corrida entre checkouts paralelos). Recomenda-se considerar um controle de concorrência/lock mais robusto se esperado alto volume concorrente.
- Índices importantes: Slug e Sku em Products; Slug em Category; OwnerKey único em Carts; índice composto em CartItems para garantir unicidade por (CartId, ProductId, Size).
- Para inspecionar DB localmente: usar DB Browser for SQLite ou `sqlite3 mafia_store_dev.db`.

---

## 10. Arquivos/chaves do projeto (onde procurar)
- `Program.cs` — configuração DI, Identity, cookies, chamada a Migrate() e seeders.
- `Data/ApplicationDbContext.cs` — DbSets e Fluent API (OnModelCreating).
- `Data/IdentitySeedData.cs` — seed de roles/usuários.
- `Data/LegacyJsonDataMigrator.cs` — importadores de `Catalog_Assets/products.json` e `context/users.json`.
- `Data/SeedData.cs` — seed de pedidos de exemplo.
- `Services/ProductCatalogEfStore.cs` — regras de negócio de produtos e CRUD (slug/sku/tamanhos/imagens).
- `Services/CartStore.cs` — lógica de carrinho (Get/Add/Update/Remove/Clear).
- `Services/OrderService.cs` — lógica de checkout (validações, stock, criação de encomenda).
- `Services/UserEfStore.cs` — integração com Identity para autenticação/geração de Users.
- `Filters/CartCountActionFilter.cs` — popula ViewBag.CartCount.
- `Controllers/` — rotas MVC: `HomeController.cs`, `ProdutosController.cs`, `CarrinhoController.cs`, `EncomendasController.cs`, `AccountController.cs`, `AdminController.cs`, `OrdersAdminController.cs`, `ReportsController.cs`.
- `Catalog_Assets/products.json` — possível fonte de importação para produtos antigos.

---

## 11. DER ASCII simplificado
(Category) --1:N--> (Product)
(Product) --0..N--> (OrderLine)
(Order) --1:N--> (OrderLine)
(Order) --1:N--> (OrderHistory)
(Cart) --1:N--> (CartItem)

Chaves e restrições principais:
- Product.Slug UNIQUE
- Product.Sku UNIQUE
- Category.Slug UNIQUE
- Cart.OwnerKey UNIQUE
- CartItem UNIQUE(CartId, ProductId, Size)
- OrderStatus armazenado como string

---

## 12. Conclusão
O banco é gerido por EF Core + migrations e armazena tanto o domínio (produtos, categorias, carrinho, encomendas e histórico) quanto as tabelas do ASP.NET Identity. As regras de negócio principais estão centralizadas em `Services/*` e respeitam validações e restrições de BD definidas em `ApplicationDbContext`. O fluxo de checkout é transacional e cuida do stock e histórico de encomendas. Há trade-offs conscientes (ex.: Orders.UserId como string) que oferecem flexibilidade de histórico em detrimento de integridade referencial automática.

---

Se quiser, posso:
- abrir/mostrar o conteúdo deste arquivo aqui no terminal;
- gerar um diagrama ER visual (export PNG) com base no modelo;
- aplicar pequenas melhorias (ex.: adicionar FK Orders.UserId -> AspNetUsers, adicionar token de concorrência em Product.Stock).

FIM.
