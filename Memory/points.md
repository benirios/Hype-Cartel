# Points / quick notes

## 2026-03-24
- UI refresh completed:
  - homepage featured products curated to formal set (4 items),
  - neutral white/gray accent palette replacing warm gold/yellow tones,
  - subtle radius polish on images and key controls.

- Admin dashboard added:
  - KPI summary, sales/status/top-product visual blocks, quick management links, recent orders table.
  - kept existing Admin / Reports / Orders flows and navigation.

- Validation snapshot:
  - `dotnet build ./MafiaStore.csproj` passed.
  - `dotnet test ./Hype-Cartel.sln` currently fails because `Tests/IntegrationTests/IntegrationTests.csproj` is referenced in solution but missing on disk.

- Follow-up suggestion:
  - restore the missing integration test project or remove stale `.sln` entry to recover full solution test execution.
