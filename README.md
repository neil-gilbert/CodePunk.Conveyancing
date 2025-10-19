# Conveyancing Portal

AI-powered shared portal for buyers, sellers and solicitors — automating most of the conveyancing process with human-in-the-loop approval.

## Overview
- Shared workspace for all parties
- AI Agents for search, title, and enquiries
- Human sign-off for all legal outputs
- Integration with ID&V, searches, SDLT, and HMLR

See `/docs` for full backlog, system design, and pitch materials.

## Backend
- .NET 9 minimal API in `src/CodePunk.Conveyancing.Api`
- Vertical-slice features under `Features/*`
- Basic endpoints: `GET /health`, `POST /api/conveyances`, `GET /api/conveyances/{id}`
- Swagger UI at `/swagger`
- Agents generate drafts only; no direct client chat

## Multi‑Tenant Portal (Overview)
- Operated as a SaaS for multiple solicitor firms and their clients.
- Tenant resolution via subdomain `{tenant}.portal.example.com` (preferred) or path `/t/{tenant}` in dev.
- Data isolation per tenant; drafts and outbox are tenant-scoped and require solicitor approval before sending.

### Run locally
- `dotnet run --project src/CodePunk.Conveyancing.Api`
  - Explore APIs at `http://localhost:5228/swagger` (port varies)

### Human-in-the-loop drafts
- AI output is stored as a draft, never sent directly to clients.
- Endpoints:
  - `POST /api/conveyances/{id}/drafts/title-enquiries` → generate draft
  - `GET /api/conveyances/{id}/drafts` → list drafts for case
  - `GET /api/drafts/{draftId}` → get draft
  - `POST /api/drafts/{draftId}/approve` → mark approved (reviewed by solicitor)
  - `POST /api/drafts/{draftId}/reject` → mark rejected with reason

### Persistence
- SQLite via EF Core (file: `conveyancing.db` in API working directory)
- Code-first with `EnsureCreated()` for dev. Migrations can be added later.

### Agents setup (Microsoft.Extensions.AI)
Packages installed:
- `Microsoft.Extensions.AI`
- `Microsoft.Extensions.AI.OpenAI`
- `Microsoft.Extensions.AI.AzureAIInference`

Current default uses a placeholder client that returns a message if not configured. To enable a provider:
- In `src/CodePunk.Conveyancing.Api/Agents/AgentSetup.cs:1`, replace the `NullChatClient` registration with your chosen provider wiring.
- Provide these env vars and restart the app:

OpenAI
- `OPENAI_API_KEY`
- `OPENAI_MODEL` (optional, default `gpt-4o-mini`)

Azure AI Inference
- `AZURE_AI_INFERENCE_ENDPOINT` (e.g. https://<your-endpoint>.models.ai.azure.com)
- `AZURE_AI_INFERENCE_API_KEY`
- `AZURE_AI_INFERENCE_MODEL` (e.g. `gpt-4o-mini`)

Anthropic
- `ANTHROPIC_API_KEY`
- `ANTHROPIC_MODEL` (optional, default `claude-3-5-sonnet-latest`)

## Frontend (Planned)
- Next.js (App Router) + Tailwind CSS, role-based navigation for firm users and clients.
- Tenant-aware theming/branding and OIDC-based login (central IdP, optional per-tenant SSO).

### Configuration
- App settings file: `src/CodePunk.Conveyancing.Api/appsettings.json`
- Environment variables override config values.
- Relevant keys:
  - `ConnectionStrings:Default` (SQLite path)
  - `AI:Provider` (`Anthropic`, `OpenAI`, or `AzureAIInference`)
  - Provider-specific keys as above

### Draft audit metadata
- Each draft stores metadata fields like:
  - `provider`, `model`, `agent`
  - `prompt_hash` (SHA-256 of system+user prompts)
  - `latency_ms`
  - `input_tokens`, `output_tokens` (when provider returns them)

Draft metadata includes `provider`, `model`, and `agent` for audit. Generate with `POST /api/conveyances/{id}/drafts/title-enquiries`.

### Next steps
- Swap `NullChatClient` for real provider wiring in `AgentSetup.cs`
- Add persistence and auth per product requirements
