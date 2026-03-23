# Steps to implement full final-project requirements

Generated: 2026-03-23
Purpose: Break down tasks required to meet PDF requirements for the final project (EF Core, Identity, Orders, transactional checkout, reports, backoffice, frontoffice). Each step includes a concise Copilot CLI prompt to execute or implement that step.

Guidelines for using the prompts
- Run prompts one-by-one.
- Inspect changes and run the app/tests in-between steps.
- Keep commits small and descriptive; include Co-authored-by trailer when committing.

---

1) Prepare repository & backups
- Goal: ensure clean state and create branch for the work.
- Outcome: new branch created, backup of current JSON data.
- Prompt to Copilot CLI:
  "git checkout -b feat/efcore-identity && mkdir -p tmp/backup && cp -r Catalog_Assets context Memory/Files tmp/backup/ && git status --porcelain" 

2) Add EF Core, Identity and SQLite packages
- Goal: add required NuGet packages for EF Core (SqlServer + Tools) and ASP.NET Identity; include SQLite for local POC.
- Outcome: csproj updated with packages.
- Prompt:
  "dotnet add package Microsoft.EntityFrameworkCore --version 8.* && dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 8.* && dotnet add package Microsoft.EntityFrameworkCore.Tools --version 8.* && dotnet add package Microsoft.EntityFrameworkCore.Sqlite --version 8.* && dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore --version 8.* && git add -A && git commit -m 'chore: add EF Core, Identity and SQLite packages\n\nCo-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>'"

3) Create EF Domain Models and DbContext
- Goal: add entities (Category, Product, User (Identity), Order, OrderLine) and ApplicationDbContext with DbSets and OnModelCreating mappings.
- Outcome: Models and Data/ApplicationDbContext.cs created.
- Prompt:
  "Create Data/ApplicationDbContext.cs and Models/Category.cs, Models/Product.cs, Models/Order.cs, Models/OrderLine.cs. Configure keys, relationships and decimal precision; add DbSets to ApplicationDbContext and register it in Program.cs using builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(Configuration.GetConnectionString('DefaultConnection')));"

4) Configure connection strings and migrations
- Goal: add connection strings for SQLite (dev) and SQL Server (production) in appsettings.json and register EF tools.
- Outcome: appsettings updated and initial migration prepared.
- Prompt:
  "Add DefaultConnection for SQLite to appsettings.Development.json: 'Data Source=mafia_store_dev.db'. Then run 'dotnet ef migrations add InitialCreate -p MafiaStore.csproj -s MafiaStore.csproj' and 'dotnet ef database update' to create DB."

5) Integrate ASP.NET Core Identity
- Goal: scaffold Identity using EF store; configure Identity options and seed admin/customer roles and accounts.
- Outcome: Identity wired, login/register use Identity stores.
- Prompt:
  "Modify ApplicationDbContext to inherit IdentityDbContext<IdentityUser>. In Program.cs, add builder.Services.AddDefaultIdentity<IdentityUser>(options => { options.Password.RequireNonAlphanumeric = false; }).AddRoles<IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>(); Create a data seeder to ensure roles Admin/Customer and test accounts exist, then run migrations and seed."

6) Migrate existing JSON data to DB (products, users)
- Goal: write a one-off migration script/service that reads Catalog_Assets/products.json and context/users.json and inserts into the new database, mapping fields.
- Outcome: relational rows populated with product and user data.
- Prompt:
  "Add a console migration tool (Migrations/DataMigrator.cs) or a temporary endpoint that reads Catalog_Assets/products.json and context/users.json, maps to Product and IdentityUser (set password via UserManager.CreateAsync using original passwords if known or set new temporary ones), then insert into ApplicationDbContext and save changes; run it once and remove."

7) Implement EF-backed services (IProductCatalogService, IUserStore)
- Goal: create new implementations ProductCatalogEfStore and UserEfStore that replace JSON stores while keeping interfaces.
- Outcome: Services use EF context and are registered in DI as singletons or scoped appropriately.
- Prompt:
  "Implement ProductCatalogEfStore : IProductCatalogService and UserEfStore : IUserStore using ApplicationDbContext; update Program.cs to register EF-based implementations (AddScoped for DbContext-backed services). Ensure existing controllers consume interfaces with minimal changes."

8) Make cart per-user and persistent
- Goal: change CartStore to store carts per authenticated user and persist carts to DB (Cart and CartItem tables) or by session cookie for anonymous users.
- Outcome: Cart now associated to user or session id and persisted.
- Prompt:
  "Create Models/Cart.cs and Models/CartItem.cs and add to ApplicationDbContext. Update CartStore to read/write carts by userId or by persistent cookie ID; ensure Add/Update/Remove operations update DB and CartCountActionFilter reads per-user cart count."

9) Implement Orders and transactional checkout
- Goal: implement Orders, OrderLines, enforce stock checks, and ensure checkout is transactional.
- Outcome: Checkout creates Order and OrderLines inside a DB transaction, decrementing stock and rolling back on failure.
- Prompt:
  "Add OrderService with CreateOrderAsync(userId, cart) that opens a DB transaction, validates stock for each product, decrements stock, inserts Order and OrderLine rows, clears user's cart, commits transaction; return order id or error. Add server-side validation to prevent checkout with insufficient stock."

10) Backoffice: CRUD for Products and Categories with role-based auth
- Goal: implement Admin controllers and views for managing products & categories with proper authorization and model validation.
- Outcome: Admin area fully functional and limited to Admin role.
- Prompt:
  "Update AdminController and views to use EF stores: inject ApplicationDbContext or ProductCatalogEfStore, implement Create/Edit/Delete for Products and Categories with server-side validations, protect controller with [Authorize(Roles=\"Admin\")]. Add unit tests for basic CRUD behaviors."

11) Reports: top 5 products, monthly revenue, orders distribution by state
- Goal: add report queries and views in Backoffice to show required aggregates.
- Outcome: 3 reports accessible to Admin.
- Prompt:
  "Create ReportsController in Admin area with actions TopProducts, MonthlyRevenue, OrderStateDistribution. Implement efficient EF LINQ queries: top 5 products by SUM(quantity), revenue grouped by year-month, and count of orders grouped by status. Add Razor views with simple charts/tables."

12) Add order states and order management
- Goal: implement order state transitions and backoffice UI to change states; enforce valid transitions.
- Outcome: Orders have state machine and only valid transitions allowed.
- Prompt:
  "Add Order.Status enum (Pending, Paid, Shipped, Cancelled, Completed). Implement Admin endpoints to change status with validation (e.g., can't ship a cancelled order). Persist status changes and log transitions in OrderHistory table."

13) Migrate authentication to secure cookies & update auth config
- Goal: keep cookie settings secure and ensure ReturnUrl checks and role claims intact. Consider using Identity default cookie settings.
- Outcome: secure authentication and correct login flow.
- Prompt:
  "Replace current cookie auth with Identity's cookie setup (SignInManager). Remove manual cookie creation in AccountController, use SignInManager.PasswordSignInAsync and UserManager for registration; ensure ReturnUrl validation and configure cookie options (SameSite, SecurePolicy, 12h sliding expiration)."

14) Tests, basic integration tests and manual QA
- Goal: add integration tests for checkout transactional flow, product CRUD and report queries; run manual QA checklist from rubric.
- Outcome: automated tests that cover critical flows.
- Prompt:
  "Add xUnit project Tests/IntegrationTests. Write integration tests for: successful checkout (stock decrement + order created), checkout failure when stock insufficient, admin product CRUD, and report queries returning expected aggregates. Run dotnet test and fix issues."

15) Documentation, seeding, and delivery
- Goal: update README/contex.md/pdfs.md, seed data, provide admin and customer credentials, and create final deliverables (ER diagram + tech doc).
- Outcome: Project ready for evaluation per rubric.
- Prompt:
  "Update contex.md and Memory/pdfs.md with migration notes and credentials; add Data/SeedData to insert sample products, users (admin: admin@local / Admin@123), and orders. Generate a short technical doc (2-4 pages) with ER diagram, architecture and list of implemented rules."

16) Cleanup and final commit
- Goal: remove temporary migration tooling, commit changes, tag release.
- Outcome: repo cleaned and release tagged.
- Prompt:
  "Remove one-off migration utilities, run 'dotnet format' if available, run 'dotnet build' and 'dotnet test', commit all changes with message 'feat: implement EF Core, Identity, orders and reports\n\nCo-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>' and create git tag 'v1-finalproject'."

---

If you want, run these steps sequentially and I will execute the Copilot CLI prompts for each step. Or I can execute step 1 now. Tell me which step to start with.