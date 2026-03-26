# Como a Base de Dados funciona — do código à criação (simples)

Este documento explica, de forma direta e simples, como o código cria e usa a base de dados do projeto, se ela é SQLite e como funcionam as migrações.

1) Resumo rápido
- A aplicação usa Entity Framework Core com o provider SQLite.
- A connection string em `appsettings.json` aponta para `Data Source=mafia_store.db` (arquivo local).
- Em `Program.cs` o `ApplicationDbContext` é registado com `UseSqlite(...)` e o aplicativo executa `db.Database.Migrate()` ao iniciar.

2) Do código ao ficheiro `.db` (passos simples)
- Definição do modelo: `Data/ApplicationDbContext.cs` declara `DbSet<>`s e configura tabelas/colunas/índices em `OnModelCreating`.
- Migrations: cada migration em `Migrations/` é uma classe C# com métodos `Up()` e `Down()` que descrevem alterações no esquema.
- Aplicação das migrações:
  - Automaticamente: `Program.cs` chama `db.Database.Migrate()` no arranque; se `mafia_store.db` não existir, EF cria o arquivo e aplica as migrations existentes.
  - Manualmente (CLI):

```bash
dotnet ef migrations add NomeDaMigration --project ./MafiaStore.csproj
dotnet ef database update --project ./MafiaStore.csproj
```

- Seeders: após aplicar migrations, o código executa `IdentitySeedData.SeedAsync(...)`, `LegacyJsonDataMigrator.MigrateAsync(...)` e `SeedData.SeedAsync(...)` para popular dados iniciais.

3) A base é SQLite — o que isto significa (explicação simples)
- É um ficheiro local único (`mafia_store.db`) que contém todas as tabelas e índices.
- Vantagens: simples de configurar, portátil, ótimo para desenvolvimento e POC.
- Limitações importantes:
  - Escrita concorrente limitada: apenas um writer por vez no ficheiro; em cargas com muitos writes simultâneos pode criar contenção.
  - Não tem features avançadas de servidor (ex.: procedures, complexos mecanismos de locking distribuído).
- No EF, tipos são mapeados para `TEXT`, `INTEGER` etc.; enums podem ser gravados como `TEXT` (como aqui) e campos JSON são guardados em `TEXT`.

4) Migrações (explicação simples)
- O que são: código que descreve mudanças no esquema (criar tabela, adicionar coluna, criar índice).
- Como são geradas: usando `dotnet ef migrations add <Nome>` — EF compara o modelo atual com o snapshot e gera a migration.
- Como se aplicam: com `dotnet ef database update` ou automaticamente com `db.Database.Migrate()` no arranque da aplicação.
- Estrutura: cada migration tem `Up()` (aplica) e `Down()` (reverte).
- O projeto já inclui migrations (ex.: InitialCreate, AddIdentityTables, AddCartTables, AddOrderStateManagement).

5) Como inspecionar e operar localmente (comandos úteis)
- Abrir DB com CLI ou GUI:

```bash
sqlite3 mafia_store.db
.tables
.schema Products
```

- Criar/atualizar migrations:

```bash
dotnet ef migrations add MinhaMudanca --project ./MafiaStore.csproj
dotnet ef database update --project ./MafiaStore.csproj
```

- Executar a aplicação (o arranque aplica migrations automaticamente):

```bash
dotnet run --project ./MafiaStore.csproj
```

6) Recomendações rápidas
- Para produção com alta concorrência, considere migrar para PostgreSQL ou SQL Server (client/server) — SQLite é file-based e tem limitações de escrita simultânea.
- Para evitar oversell em cenários concorrentes, adicione `RowVersion` (optimistic concurrency) ou atualizações atómicas no BD.
- Fazer backups é simples: copiar o ficheiro `mafia_store.db` ou usar `.dump` do `sqlite3`.

---

Se quiser, eu:
- gravo este ficheiro agora (já fiz);
- adiciono um bloco com exemplos das SQL geradas pelas migrations;
- crio um pequeno tutorial para migrar para PostgreSQL (scripts e passos).

Qual o próximo passo preferes?