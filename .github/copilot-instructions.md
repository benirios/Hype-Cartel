# Copilot instructions for MafiaStore (Hype Cartel)

Purpose: provide focused, repo-specific guidance for future Copilot CLI / Copilot sessions to act correctly and efficiently in this repository.

1) Build, test, and lint commands
- Build: dotnet build ./MafiaStore.csproj
- Run: dotnet run --project ./MafiaStore.csproj
- Run on a specific URL (if address in use): dotnet run --project ./MafiaStore.csproj --no-build --urls http://127.0.0.1:5301
- EF tooling (local tool may be installed under .dotnet-tools): dotnet ef migrations add <Name> --project ./MafiaStore.csproj && dotnet ef database update --project ./MafiaStore.csproj
- Tests: dotnet test (no test project present by default). To run a single test: dotnet test --filter "FullyQualifiedName=Namespace.ClassName.TestMethod" or dotnet test --filter "TestName~PartialName"
- Formatting (if available): dotnet format

2) High-level architecture (what Copilot should assume)
- ASP.NET Core MVC app (.NET 10) with Razor views.
- Layers:
  - Controllers/ : HTTP endpoints (Portuguese controller names: Produtos, Carrinho, Account, Admin, Home)
  - Views/ : Razor UI organized by controller
  - Services/ : Application services and interfaces (IProductCatalogService, ICartStore, IUserStore, IOrderService)
  - Data/ : ApplicationDbContext, EF models, migrations and seeders
  - Memory/ : project "vault" for documents and extracted PDFs (context and pdfs.md)
- Authentication: ASP.NET Core Identity is integrated; roles (Admin, Customer) are used for authorization.
- Persistence: project supports EF Core with SQLite (DefaultConnection) and contains migration+legacy JSON migration code (LegacyJsonDataMigrator). Existing product/user JSON import is performed at startup if needed.
- Startup behavior: Program.cs applies migrations (db.Database.Migrate()) and seeds Identity and legacy JSON data on startup — Copilot should be careful when making changes that affect migration/seed code.
- Static assets: Assets/ is exposed via UseStaticFiles mapped to the /Assets URL. Images referenced in Catalog_Assets should be accessible via /Assets.
- Cart model: CartStore and related interfaces exist; carts were historically JSON/in-memory. Where changes to cart behavior are made, preserve or migrate old data using LegacyJsonDataMigrator.

3) Key conventions and patterns (repo-specific)
- Interfaces-first: business logic is accessed via interfaces in Services/ (IProductCatalogService, IUserStore, ICartStore). Maintain those contracts when replacing implementations.
- DI lifetimes:
  - EF-backed services are registered AddScoped (ApplicationDbContext and stores).
  - Non-DB singletons historically existed; prefer Scoped for DbContext-backed implementations.
- Data migration flow:
  - Program.cs currently runs db.Database.Migrate() and IdentitySeedData.SeedAsync(scopedServices) followed by LegacyJsonDataMigrator.MigrateAsync(scopedServices, env). When editing these flows, preserve ordering and idempotency.
- Naming: controllers and views use Portuguese names (Produtos, Carrinho). Keep route and view naming consistent to avoid breaking URLs.
- Anti-forgery: All state-changing POST endpoints use ValidateAntiForgeryToken. Copilot should ensure views/forms include @Html.AntiForgeryToken() when adding forms.
- Password handling: legacy UserStore used salt+SHA256. The repository now integrates Identity. Be careful not to leak legacy password hashes; use the provided seeding/migrator utilities.
- Static asset mapping: Program.cs maps Assets folder to /Assets. Do not assume wwwroot is the only static folder.

4) Files to check before making changes
- Program.cs — startup, DI registration, migrations, seeding, static mapping
- Data/ApplicationDbContext.cs and Files under Data/ — EF models and migrations
- Services/* — implementors of IProductCatalogService, IUserStore, ICartStore
- Filters/CartCountActionFilter.cs and Cart-related resolvers — changes to cart/count must preserve filter behavior
- Memory/Context.md and Memory/pdfs.md — contain project context and extracted requirements. Update these when adding features required by the final project brief.

5) Safe-edit rules for Copilot sessions
- Preserve interface contracts. If changing an interface, update all implementations and register appropriate services in Program.cs.
- Avoid changing Program.cs migration/seed ordering without explicit user approval.
- When adding EF migrations, prefer creating them locally and running db update; do not commit temporary migration artifacts unless asked.
- When modifying authentication or password handling, ensure Identity integration tests (or manual verification steps) are performed and avoid writing cleartext credentials to source.

6) Common commands Copilot may run during automation
- git checkout -b feat/your-change
- dotnet build ./MafiaStore.csproj
- dotnet ef migrations add MyMigration --project ./MafiaStore.csproj
- dotnet ef database update --project ./MafiaStore.csproj
- dotnet run --project ./MafiaStore.csproj --urls http://127.0.0.1:5301
- dotnet test --filter "TestName=MyTest"

7) Where to add documentation or seed data
- Memory/Context.md — high-level notes and decisions (keeps assistant memory)
- Memory/pdfs.md — authoritative extracted requirements from course PDFs
- Data/SeedData.cs or Data/IdentitySeedData.cs — seeds for admin/customer credentials

8) Other assistant configs discovered
- No CLAUDE.md, .cursorrules, AGENTS.md, .windsurfrules, CONVENTIONS.md or similar files detected. If you add external assistant rules, include a short summary here.

-----
Would you like me to configure an MCP server for browser testing (Playwright) or end-to-end testing for this web project? (yes / no)

Summary: created .github/copilot-instructions.md containing build/test commands, architecture overview, and repository-specific conventions to help future Copilot sessions. Tell me if you want changes or additional coverage (e.g., sample migration checklist, seed credentials, or E2E test setup).
