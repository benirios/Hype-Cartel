# MafiaStore (Hype Cartel)

MafiaStore is an inventory-backed online boutique demo and reference implementation built with .NET 10 (ASP.NET Core MVC + Razor Views). It demonstrates a full small-ecommerce flow: EF Core persistence, ASP.NET Core Identity (roles), transactional checkout, admin backoffice and operational reporting.

Highlights
- Technology: .NET 10, ASP.NET Core MVC, Razor Views, EF Core, ASP.NET Core Identity
- Persistence: SQLite for local development (./mafia_store_dev.db); production-ready EF Core configuration for SQL Server
- Domain: Products (stock-aware), Categories, Shopping Carts, Orders, OrderHistory
- Admin: product/category CRUD, order state management, Admin/Dashboard with KPIs and charts
- Reporting: Top products, monthly revenue, order state distribution (Admin dashboard uses Chart.js for visuals)
- Data migration: LegacyJsonDataMigrator to import legacy JSON catalogs and seed data
- Tests: Integration tests covering checkout flows, CRUD and reports

Quickstart — development

Prerequisites
- .NET 10 SDK
- Optional: dotnet-ef if you will run EF migrations locally (may be installed as a local dotnet tool under .dotnet-tools)

Run locally (from repo root)
1. dotnet build ./MafiaStore.csproj
2. dotnet run --project ./MafiaStore.csproj --urls http://127.0.0.1:5301
   - To run without rebuilding: dotnet run --project ./MafiaStore.csproj --no-build --urls http://127.0.0.1:5301

Database & seeding
- Dev DB file: ./mafia_store_dev.db (SQLite) — included for convenience
- Apply migrations: dotnet ef database update --project ./MafiaStore.csproj
- Create a migration: dotnet ef migrations add <Name> --project ./MafiaStore.csproj
- Legacy JSON seeding: see Data/ and the LegacyJsonDataMigrator for importing legacy product catalogs

Default development accounts (development only)
- Admin: admin@local / Admin@123
- Customer: cliente@local / Cliente@123

Running tests
- Run the full test suite: dotnet test ./Hype-Cartel.sln
- If tests fail due to historical references, consult Memory/full_history.md for context

Project layout (high-level)
- Controllers/, Models/, Views/, Services/, Data/, Migrations/, Memory/
- Notable files:
  - Models/ViewModels/ProdutoViewModel.cs — includes Stock; both EF and JSON product services read/write stock values
  - Controllers/AdminController.cs, Controllers/ReportsController.cs, Controllers/OrdersAdminController.cs — admin pages and dashboard redirects
  - Views/Admin/Dashboard.cshtml — admin KPIs and charts

Documentation & runbooks
The repository includes a Memory/ directory with architecture documents, runbooks and checklists that should be consulted before major changes or deployments. Useful entries:
- Memory/arquitetura_componentes.md — architecture overview
- Memory/runbook_deploy.md — deployment runbook and steps
- Memory/runbook_incidentes.md — incident response playbook
- Memory/qa_checklist_funcional.md — QA checklist and critical flows
- Memory/full_history.md — change history and important notes

Contributing
- Check Memory/backlog_priorizado.md for the prioritized backlog
- Document significant changes in Memory/full_history.md
- Run tests and ensure migrations apply before opening a pull request

Where to get help
- Start with Memory/Context.md and Memory/steps.md for Copilot CLI prompts and context
- Open an issue or contact the repository owner for questions not covered in the Memory/ docs

License
- Add license information here if applicable

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
