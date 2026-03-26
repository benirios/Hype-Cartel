# Architecture

High-level overview of system components, data model and important architecture notes.

Key components (short)
- Web app: ASP.NET Core MVC + Razor
- Data: EF Core (SQLite in dev; consider PostgreSQL/SQL Server for production)
- Identity: ASP.NET Core Identity
- Services: ProductCatalog, CartStore, OrderService

Archived detailed docs:
- [Component architecture](_archive/arquitetura_componentes.md)
- [DB schema / ER](_archive/schema_sql_er.md)
- [DB how-it-works](_archive/DB_HowItWorks.md)

Keep this file concise: one paragraph per component and links to diagrams in _archive.
