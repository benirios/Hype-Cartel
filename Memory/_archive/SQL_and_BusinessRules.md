# SQL, Regras de Negócio e Arquitetura MVC — Resumo do Projeto

Este documento descreve, com base no código, o esquema de dados (SQL/EF), as regras de negócio principais e como a aplicação está organizada na arquitetura MVC.

---

**1) Modelo de Dados (tabelas principais)**

- `AspNet*` (Identity): tabelas de Identity criadas por `IdentityDbContext<IdentityUser>` (usuários, roles, claims, logins etc.).

- `Categories` (Category)
  - Id (PK, int)
  - Name (string, MaxLength 120, required)
  - Slug (string, MaxLength 160, required, UNIQUE index)
  - Relação: 1 Category -> N Products

- `Products` (Product)
  - Id (PK, int)
  - Name (string, MaxLength 180, required)
  - Slug (string, MaxLength 180, required, UNIQUE index)
  - Sku (string, MaxLength 60, required, UNIQUE index)
  - Description (string, MaxLength 4000, required)
  - ImageUrl (string, MaxLength 1000, required)
  - AdditionalImagesJson (TEXT) — JSON array de strings
  - SizesJson (TEXT) — JSON array de strings
  - Price (decimal(18,2))
  - Stock (int)
  - Highlight (bool)
  - CategoryId (FK -> Categories.Id) ON DELETE RESTRICT

- `Orders` (Order)
  - Id (PK, int)
  - UserId (string, MaxLength 450, required) — Identity user id (ou guest owner key)
  - CreatedAtUtc (DateTime, required)
  - Status (string) — enum `OrderStatus` armazenado como string (converter EnumToString)
  - Subtotal, Vat, Total (decimal(18,2))

- `OrderLines` (OrderLine)
  - Id (PK, int)
  - OrderId (FK -> Orders.Id) ON DELETE CASCADE
  - ProductId (int)
  - ProductName (string, MaxLength 180, required)
  - Size (string, MaxLength 30, nullable)
  - Quantity (int)
  - UnitPrice (decimal(18,2))

- `OrderHistory` (OrderHistory)
  - Id (PK, int)
  - OrderId (FK -> Orders.Id) ON DELETE CASCADE
  - FromStatus, ToStatus (enum -> string, MaxLength 40)
  - ChangedBy (string, MaxLength 256)
  - ChangedAtUtc (DateTime)
  - Index em `OrderId`

- `Carts` (Cart)
  - Id (PK, int)
  - OwnerKey (string, MaxLength 180, required, UNIQUE index)
  - UpdatedAtUtc (DateTime, required)

- `CartItems` (CartItem)
  - Id (PK, int)
  - CartId (FK -> Carts.Id) ON DELETE CASCADE
  - ProductId (int)
  - Size (string, MaxLength 30, required)
  - Quantity (int, required)
  - UNIQUE index composito `(CartId, ProductId, Size)`

---

**2) Migrações**

- O repositório contém migrações que mostram a evolução do schema (ex.: `InitialCreate`, `AddIdentityTables`, `AddCartTables`, `AddOrderStateManagement`).
- `ApplicationDbContext` configura o modelo (índices, FK, tipos, conversores de enum, precisão decimal).
- Observação: `AdditionalImagesJson` e `SizesJson` são `TEXT` — adequado para SQLite/EF, mas significa que dados relacionais de tamanhos/imagens não estão normalizados.

---

**3) Regras de Negócio implementadas no código**

- Catálogo / Produtos
  - Validações ao criar/atualizar: Id>0, Nome obrigatório, Preço >= 0, Stock >= 0, Categoria e Imagem e Descrição obrigatórias.
  - `Slug` é gerado/normalizado por `Slugify()`; `Slug` único.
  - `Sku` gerado com padrão `SKU-<id>` (fallback com GUID se necessário).
  - `ResolveCategory()` cria categoria automaticamente quando não existe.
  - `Sizes` e `AdditionalImages` são armazenados como JSON (listas) e normalizados.

- Carrinho (Cart)
  - Cada carrinho é identificado por `OwnerKey` (usuário logado ou chave cliente). `OwnerKey` é obrigatório e único.
  - `AddItem`: se item (produto + size) já existe, incrementa `Quantity`; caso contrário adiciona novo `CartItem`.
  - `NormalizeSize()` padroniza tamanhos (upper-case) e usa tamanho default do produto se não especificado.
  - `UpdateQuantity`: se quantidade <= 0 remove o item.
  - `GetOrCreateCart()` cria o registro `Cart` se inexistente.

- Checkout / Pedidos (OrderService)
  - `CreateOrderAsync(userId, ownerKey)` executa as seguintes validações e passos dentro de transação:
    - Verifica que o carrinho exista e não esteja vazio.
    - Carrega produtos referenciados e valida:
      - Produto existe.
      - Quantidade do item > 0.
      - `product.Stock >= quantidade` (suficiência de stock).
    - Atualiza `product.Stock -= quantidade` (em memória, depois SaveChanges dentro da mesma transação).
    - Calcula `subtotal = sum(price * qty)`; `vat = round(subtotal * 0.23, 2, AwayFromZero)`; `total = subtotal + vat`.
    - Cria `Order` com `Status = Pending`, adiciona `OrderLines`, persiste, remove itens do carrinho e commita transaction.
    - Retorna `CheckoutResult.Ok(order.Id)` ou `CheckoutResult.Fail(message)`.
  - Observações importantes:
    - A decrementação de stock acontece dentro da transação, mas não há um mecanismo de locking/optimistic concurrency explícito (RowVersion). Pode haver condições de corrida sob alta concorrência.
    - O VAT está fixo em 23% no código.

- Ordem e Histórico
  - `OrderStatus` é um enum com valores: Pending, Paid, Shipped, Cancelled, Completed.
  - `OrderHistory` grava transições com `FromStatus`, `ToStatus`, `ChangedBy` e `ChangedAtUtc`.

- Autenticação e autorização
  - A aplicação usa ASP.NET Core Identity (IdentityDbContext) para gerir usuários.
  - `EncomendasController` exige `[Authorize]` para listar/detalhar pedidos do utilizador.
  - Endpoints de escrita (POST) usam `[ValidateAntiForgeryToken]` (proteção CSRF) nos controllers (ex.: `CarrinhoController`).

---

**4) Arquitetura MVC e responsabilidades**

- Camadas:
  - Controllers/: controladores MVC que tratam requests e devolvem Views ou JSON (`ProdutosController`, `CarrinhoController`, `EncomendasController`, `AdminController`, etc.).
  - Views/: Razor views organizadas por controller (UI).
  - Models/: modelos de domínio e view-models (produto, pedido, carrinho, etc.).
  - Data/: `ApplicationDbContext` (EF Core), migrations, seeders (`IdentitySeedData`, `SeedData`, `LegacyJsonDataMigrator`).
  - Services/: lógica de negócio reutilizável e persistência mais elevada (interfaces e implementações):
    - `IProductCatalogService` / `ProductCatalogEfStore` (catalog operations)
    - `ICartStore` / `CartStore` (carrinho CRUD)
    - `IOrderService` / `OrderService` (checkout e criação de pedidos)
    - `ICartOwnerResolver` (resolução da chave do dono do carrinho)
    - `IUserStore` (abstração sobre usuários)
  - Filters/: filtros cross-cutting (ex.: `CartCountActionFilter` para popular contador de items no layout).

- Fluxo típico (ex.: Checkout)
  1. Cliente adiciona itens via `CarrinhoController.Adicionar` (POST) -> `ICartStore.AddItem` persiste `CartItem`.
  2. Cliente faz `Carrinho/Checkout` (POST) -> `IOrderService.CreateOrderAsync`:
     - valida carrinho, produtos e stock
     - cria `Order` e `OrderLines`, atualiza stock
     - remove itens do carrinho
     - commit transacional
  3. Usuário autenticado vê pedidos em `EncomendasController` (filtrado por `UserId`).

---

**5) Pontos de atenção / riscos / melhorias sugeridas**

- Concorrência de stock: não existe `RowVersion`/optimistic concurrency token nos `Products`. Em cenários de alta concorrência, múltiplos checkouts podem permitir vender mais do que o stock disponível. Recomenda-se adicionar um campo `RowVersion` (byte[]) e usar verificações de concurrency, ou aplicar locking no nível de BD.

- Normalização dos tamanhos e imagens: armazenar como JSON é prático, mas impede consultas relacionais (ex.: procurar produtos por tamanho). Se necessário suportar filtros por tamanho no BD, considere uma tabela `ProductSize` normalizada.

- VAT fixo e regras fiscais: o cálculo está hardcoded (23%). Se for variável por produto, região ou cliente, deve ser externalizado para uma regra configurável.

- Slugify e unicidade: Slugify é simples; para internacionalização e caracteres especiais complexos pode produzir colisões. Há tratamento para Slug duplicado em criação/atualização, mas cuidado com limites de tamanho.

- Validações no lado cliente: a maior parte das validações importantes está no servidor (ProductCatalogEfStore, OrderService, CartStore) — bom.

- Logs e auditoria: `OrderHistory` existe, mas as transições automáticas (ex.: Paid -> Shipped) precisam ser acionadas por código administrativo (ver `OrdersAdminController`) — verificar se sempre gravam `OrderHistory` nas mudanças.

---

**6) Checklist para validação funcional (sugestão rápida)**

- [ ] Criar produto com dados válidos -> SKU/Slug únicos, categoria criada se inexistente.
- [ ] Adicionar item ao carrinho (com/sem tamanho) -> item aparece com tamanho normalizado.
- [ ] Atualizar quantidade para 0 -> item removido.
- [ ] Checkout com stock suficiente -> order criada, stock decrementado, carrinho esvaziado, order.status = Pending.
- [ ] Checkout com algum produto sem stock -> falha com mensagem adequada, transação revertida.
- [ ] Usuário autenticado só vê suas encomendas (testar `EncomendasController`).
- [ ] Testar formas de ataque concurrency (duplo checkout rápido) e ver comportamento do stock.

---

**7) Arquivos relevantes (para referência)**

- `Data/ApplicationDbContext.cs` — model builder, índices, conversores
- `Models/*` — domain models: `Product`, `Category`, `Cart`, `CartItem`, `Order`, `OrderLine`, `OrderHistory`, `OrderStatus`
- `Services/ProductCatalogEfStore.cs`, `Services/CartStore.cs`, `Services/OrderService.cs` — regras centrais
- `Controllers/ProdutosController.cs`, `Controllers/CarrinhoController.cs`, `Controllers/EncomendasController.cs` — pontos de entrada do fluxo
- `Migrations/` — migrações aplicadas

---

Se quiser, eu:
- adiciono um diagrama ER textual (ou mermaid) dentro deste ficheiro;
- incluirei exemplos de queries SQL geradas (ex.: criação de tabelas a partir das migrações);
- crio um ticket de melhoria para concorrência de stock e normalização de tamanhos.

Diz-me qual o próximo item preferes: gerar o diagrama ER aqui, ou criar um ticket/PR com melhorias (ex.: `RowVersion`)?
