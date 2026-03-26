# Resume Instructions — Quick start for next Copilot session

Purpose: concise, copy-paste commands and notes to quickly resume development or testing.

Prerequisites
- .NET 10 SDK installed and on PATH
- From repository root: /Users/beni/Dev/MafiaStore

Commands
1) Build the project
   dotnet build ./MafiaStore.csproj

2) Run the web app locally (development)
   dotnet run --project ./MafiaStore.csproj --urls http://127.0.0.1:5301

3) Inspect the dev DB (SQLite)
   sqlite3 mafia_store_dev.db ".tables"
   sqlite3 mafia_store_dev.db "SELECT COUNT(*) FROM Products;"

4) Run EF migrations (if creating new migration)
   dotnet ef migrations add <Name> --project ./MafiaStore.csproj
   dotnet ef database update --project ./MafiaStore.csproj

5) Seed / re-seed data (manual)
   - Seeds run automatically on startup via IdentitySeedData and SeedData.
   - To force a clean seed: stop the app, delete mafia_store_dev.db, then dotnet run.

6) Run tests
   dotnet test ./Hype-Cartel.sln

Useful notes
- Local admin credentials (development only): admin@local / Admin@123
- Customer credentials (development only): cliente@local / Cliente@123
- DB path: ./mafia_store_dev.db
- Migrations can be added and updated with dotnet ef commands above.

If you want, I can also add a small shell script ./scripts/resume_dev.sh that runs build, starts the server on 5301, and tails the logs.
