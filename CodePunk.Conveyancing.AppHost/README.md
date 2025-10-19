# AppHost (Aspire) — Local Orchestration

This AppHost uses .NET Aspire to run the API and Portal together.

## Prerequisites

- .NET 9 SDK (installed)
- .NET Aspire workload for .NET 9 (required for DCP + Dashboard)

Verify your Aspire workload version:

- `dotnet workload list` → look for `aspire` with a 9.0.x manifest (not 8.x)

If it shows 8.x, update/install for .NET 9:

- `dotnet workload update`
- `dotnet workload install aspire`
- `dotnet workload list` (should now show 9.0.x)

If the DCP/Dashboard error persists, ensure PATH points to the .NET 9 SDK and workloads.

## Run

- `dotnet run --project CodePunk.Conveyancing.AppHost`
- API at http://localhost:5228 (default ASP.NET port)
- Portal at http://localhost:3000

The AppHost wires `NEXT_PUBLIC_API_BASE` for the portal automatically.

## Fallback (no Aspire)

If Aspire tooling isn’t available, run services separately:

- API: `dotnet run --project src/CodePunk.Conveyancing.Api`
- Portal:
  - `cd apps/portal && npm install`
  - `export NEXT_PUBLIC_API_BASE=http://localhost:5228/api`
  - `npm run dev`
  - Open `http://localhost:3000/t/{tenant}`

## Notes

- Earlier endpoint conflict fixed; AppHost relies on the default ASP.NET `http` endpoint.
- NuGet warnings about rc versions are benign for local dev.
