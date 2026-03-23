# MafiaStore (Hype Cartel)

A modern, inventory-backed online boutique demo built with ASP.NET Core 10 MVC and Razor Views. Designed as a final-project reference implementation: EF Core persistence, ASP.NET Core Identity authentication, transactional checkout, and admin reporting.

Key features
- Tech: .NET 10, ASP.NET Core MVC, EF Core (SQLite dev / SQL Server production-ready), Identity (roles), Razor Views
- Domain: Products, Categories, Carts, Orders, OrderHistory, Reports (Top 5 products, Monthly revenue, Order state distribution)
- Admin backoffice: product/category CRUD, order state management
- Data migration: legacy JSON importer to seed DB (LegacyJsonDataMigrator)
- Tests: Integration tests covering checkout, CRUD and reports

Quickstart (development)
1. Install .NET 10 SDK
2. From repo root:
   - dotnet build ./MafiaStore.csproj
   - dotnet run --project ./MafiaStore.csproj --urls http://127.0.0.1:5301
3. Dev DB: ./mafia_store_dev.db (SQLite)
4. Default dev accounts (development only):
   - Admin: admin@local / Admin@123
   - Customer: cliente@local / Cliente@123

Developer notes
- Migrations: dotnet ef migrations add <Name> --project ./MafiaStore.csproj
- Update DB: dotnet ef database update --project ./MafiaStore.csproj
- Tests: dotnet test ./Hype-Cartel.sln
- Memory vault: see Memory/Context.md, Memory/pdfs.md, Memory/steps.md for project context, requirements and step-by-step prompts for Copilot CLI.

Contribution & commits
- All significant changes are documented in Memory/full_history.md.

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
