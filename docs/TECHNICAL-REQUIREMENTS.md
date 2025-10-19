# Conveyancing Portal — Technical Requirements (MVP)

Status: Draft for collaboration
Owner: Engineering
Scope: MVP (8–10 weeks) mapped from MVP-PLAN.md

---

## 1. Goals & Constraints

- Reduce instruction→exchange time by 20–30% via automation with strict human-in-the-loop control.
- Auto-draft ≥70% of routine outputs (enquiries, digests, RoT sections) but require solicitor approval for any client/external send.
- One shared timeline to reduce status-chasing by 50%.
- Non‑functional: UK/EU data residency, encryption in transit/at rest, immutable audit, P95 < 1.5s for primary views.

Out of scope (MVP): lender panel integrations, full SDLT/HMLR filing flows (lightweight prep only), chain-wide optimization.

---

## 2. Architecture Overview

- Approach: Modular monolith in .NET 9 with vertical-slice architecture.
  - API: ASP.NET Core minimal APIs; per-feature route groups under `/api`.
  - Agents: Microsoft.Extensions.AI abstraction; Anthropic (Claude) first-class provider; others pluggable later.
  - Persistence: EF Core; SQLite for dev, Postgres for prod.
  - Files: S3-compatible object storage in prod (local disk for dev) for documents and large blobs.
  - Background work: Hosted workers for parsing, RAG indexing, webhooks, and retries.
  - Observability: Structured logs + basic metrics; OpenTelemetry when available.
- Frontend: React/Next.js portal (role-based), consuming the API. Server-side auth via OIDC.

Rationale: Vertical slices prioritize speed and cohesion, limiting cross-coupling. Modular monolith avoids premature service boundaries while leaving seams for future extraction.

---

## 2a. Multi‑Tenancy (SaaS)

- Tenancy model: B2B multi-tenant SaaS. A Tenant represents a solicitor firm (and optionally their invited clients). Our company operates the shared platform.
- Isolation:
  - Data isolation with `TenantId` on all tenant-owned tables (Matters, Documents, Drafts, Outbox, Users, etc.).
  - Global query filter in EF Core to enforce `TenantId` automatically per request.
  - Optional Postgres Row-Level Security (RLS) in production for defense-in-depth.
- Tenant resolution:
  - Preferred: subdomain `https://{tenantSlug}.portal.example.com`.
  - Fallback (dev): path prefix `/t/{tenantSlug}`.
  - Custom domains (optional): CNAME mapping table with domain→Tenant lookup.
- Provisioning:
  - Tenant creation API (admin only) with fields: name, slug, region, brand (logo/colors), SSO config, billing plan.
  - Tenant admins invite fee-earners and clients (email-based invite links).
- Branding:
  - Per-tenant theme (logo, primary color) and email templates.
  - Feature flags/entitlements via plan tiers stored at tenant level.

Request flow (backend):
- TenantResolver middleware extracts tenant from host or path, validates it, and stores `TenantId` + `TenantSlug` into request scoped context.
- EF Core sets `HasQueryFilter(e => e.TenantId == CurrentTenantId)` on multi-tenant entities.
- All writes set `TenantId` from context; cross-tenant access is forbidden by design.

AuthN/AuthZ (multi-tenant):
- Central OIDC (e.g., Auth0 Organizations or Entra ID multi-tenant) with organization/tenant claims.
- Supports per-tenant SAML SSO (optional) mapped to the same Tenant.
- Roles include tenant-scoped roles: `tenant_admin`, `fee_earner`, `supervisor`, `compliance`, `client_buyer`, `client_seller`.

---

## 3. Vertical Slice Structure (Backend)

- Project: `CodePunk.Conveyancing.Api` (net9.0)
- Folders:
  - `Features/<Area>/<UseCase>`: endpoint mappers and request/response records
  - `Domain`: entities/enums/value objects (EF-friendly)
  - `Data`: `ConveyancingDbContext`, migrations
  - `Infrastructure`: adapters for storage, outbound providers, webhooks, email bridges
  - `Agents`: agent DI setup + agent implementations
- Conventions:
  - Routes grouped at slice level with `MapGroup`.
  - Input records/DTOs are slice-local.
  - Each slice owns validation, authorization, and errors.
  - Handlers interact with `DbContext` directly (MVP) or via small repositories if needed.

---

## 4. Domain Model (MVP)

Aligns to ERD in MVP-PLAN.md; MVP subset below.

- Matter (Conveyance): id, buyer/seller parties, property, status, key dates, createdAt
- Property: id, titleNo, uprn, address, tenure, geo
- Party: id, role [buyer|seller|fee_earner|supervisor|compliance], name, emails[], phones[], kyc/aml
- Document: id, matterId, partyId?, type, edition, filename, mime, size, hash, extractedJson, approvalState, createdAt
- Search: id, matterId, kind, provider, requestRef, status, eta, responseBlob, parsedFindingsJson, severity
- Enquiry: id, matterId, draftText, approvedText, citations[], sentAt, replyId?, status
- Message (Preview Outbox): id, matterId, channel[email|portal], draftText, approvedText, to[], from, status
- Filing (light): id, matterId, type [SDLT|AP1], payloadJson, status, externalRef
- AuditLog: id, actorId, actorType[human|agent], action, entity, entityId, beforeJson, afterJson, ts
- DraftDocument (implemented): id, conveyanceId, type[TitleEnquiries|…], status[Draft|Approved|Rejected], contentMarkdown, metadata, created/approved/rejected stamps

Tenancy additions:
- Tenant: id, slug, name, region, brand_json, sso_config_json, billing_plan, created_at
- User (platform): id, tenant_id, auth_provider_id, email, roles[], name, created_at
- All tenant-owned entities include `tenant_id` for isolation.

Notes:
- DraftDocument is used for AI-generated outputs pending approval (Enquiries, RoT sections, Search digest summaries).
- For MVP we store extracted JSON and blob ref on Document; vector store/RAG indexes per matter can be introduced later.

---

## 5. Data & Persistence

- Dev: SQLite `conveyancing.db` via EF Core, EnsureCreated (MVP).
- Prod: Postgres 15+, EF Core migrations, flyway/DbUp optional for seed/control.
- Encryption at rest: at storage layer (Postgres TDE or volume encryption) + field-level encryption for highly sensitive data (future).
- Object storage: S3-compatible (e.g., Azure Blob S3 front or MinIO) with server-side encryption, signed URLs, bucket per environment.
- Indices: entity indexes on foreign keys (matterId), createdAt; partial indexes on DraftDocument(status).

---

## 6. Integrations (MVP)

- ID&V Provider
  - REST API for liveness + MRZ; webhook for completion.
  - Store outcome: pass/fail, reasons, risk flags; require override+justification on fail.
- Open Banking (optional in MVP): consent + statements ingestion; otherwise PDF parse.
- Search Providers
  - Ordering API; provider selection by locale; ETAs; results via webhook or polling.
  - Store original PDFs and provider JSON; parse to findings JSON with citations (page/para).
- E-sign (light)
  - Envelope creation for Contract/TR1; status updates; signed doc fetch.
- Email Bridge (post-MVP): send/receive with approval gate.

Integration guarantees (NFR):
- Webhooks idempotent (event key), signed (HMAC), and retried (exponential backoff).
- All external calls logged with correlation IDs; PII redacted in logs.

---

## 7. AI/Agents

- Provider Abstraction: Microsoft.Extensions.AI for common interfaces; Anthropic Claude support via direct HTTP for now.
- Human-in-the-loop: Agents only create `DraftDocument`s; never send external communications.
- Prompting: system prompts include role/scope; user prompts pass structured context; require citations and assumptions.
- Audit metadata captured on every draft:
  - `provider`, `model`, `agent`, `prompt_hash` (SHA-256 of system+user), `latency_ms`, `input_tokens`, `output_tokens` (if available).
- Safety: matter-scoped context, RAG to be introduced with per-matter namespaces; confidence thresholds; citation checks.

Agents (MVP):
- TitleEnquiryAgent (implemented): generates enquiries draft from title/search context; currently uses address; extend to full corpus.
- SearchDigestAgent (planned): summarizes search results with traffic-light severity; citations to PDFs.
- ReportOnTitleAgent (planned): drafts RoT with sections and actions/indemnities.

Multi-tenant considerations:
- Agent context strictly scoped to a single tenant and matter; no cross-tenant retrieval.
- Prompt/audit metadata must not leak tenant identifiers in logs.

Configuration:
- Anthropic: `ANTHROPIC_API_KEY`, optional `ANTHROPIC_MODEL` (default `claude-3-5-sonnet-latest`).
- OpenAI/Azure AI Inference pluggable via Microsoft.Extensions.AI providers (optional for MVP if Anthropic is default).

---

## 8. API Surface (MVP-first)

Base: `/api`

- Health
  - GET `/health` → `{ status: "ok" }`

- Conveyances
  - POST `/conveyances` → create matter; returns entity
  - GET `/conveyances/{id}` → fetch

- Drafts (Human-in-the-loop)
  - POST `/conveyances/{id}/drafts/title-enquiries` → generate draft from agent
  - GET `/conveyances/{id}/drafts` → list drafts for a matter
  - GET `/drafts/{draftId}` → fetch draft
  - POST `/drafts/{draftId}/approve` → mark approved (actor provided)
  - POST `/drafts/{draftId}/reject` → mark rejected (actor + reason)

Planned endpoints (next slices):
- Documents: upload/store; parse; extracted JSON retrieval
- Searches: order; provider webhook endpoints; parsed digest retrieval
- Outbox: list/edit/approve/send communications; diff view
- RoT: generate sections as drafts; compile to PDF on approval
- Timeline: consolidated events for stakeholders; SLA heatmap data

Patterns:
- Use route groups per feature; request/response DTOs local to slices; return appropriate status codes (201/200/404/422/409).
- Validation: minimal server-side for MVP; extend with FluentValidation.
- Errors: ProblemDetails (RFC 7807) for standardized error payloads.

Multi-tenant patterns:
- All `/api` routes run under tenant context resolved from host/path.
- Admin-only `/api/admin/tenants` endpoints for provisioning (create/update, invite).
- Pagination and search are tenant-scoped by default.

---

## 9. Security, AuthZ, Compliance

- AuthN: OIDC (Auth0/Entra ID/Keycloak). For MVP, start with firm-level login; role claims for authorization.
- AuthZ: roles `buyer`, `seller`, `fee_earner`, `supervisor`, `compliance`. Enforce on endpoints using authorization policies.
- Data residency: host in UK/EU region; data processing limited to approved providers.
- Secrets: environment variables + platform secret store; never commit secrets.
- Encryption: TLS1.2+; at-rest via managed DB encryption; object storage SSE.
- Logging: PII/financial data redacted/masked; correlation IDs per request.
- Audit: immutable append-only audit log entries for all state changes and approvals.

Multi-tenant specifics:
- Organization-aware SSO (per-tenant) while maintaining a central IdP configuration.
- Session contains tenant claim; backend enforces per-request tenant context.
- Optional DB-level RLS by tenant in production environments.

---

## 10. Observability & Ops

- Logging: Serilog or built-in structured logging; request/response summaries with correlation id.
- Metrics: request rate, latency (P50/P95), error rate, agent latency/tokens; publish to Prometheus-compatible endpoint.
- Tracing: OpenTelemetry SDK hooks for HTTP client and DB (optional for MVP).
- CI/CD: build, test, publish container; EF migrations run on deploy; environment config via variables.
- Migrations: introduce `dotnet ef` migrations; replace `EnsureCreated()` with `Migrate()` outside dev.

---

## 11. Performance & Reliability Targets

- API P95 < 1.5s for primary views; background parsing offloaded to workers.
- Reliability: 99.5% uptime monthly in MVP; circuit breakers/timeouts for external calls (HTTP client policies).
- Webhooks: idempotent 
- Rate limiting: basic IP/user limits to prevent abuse.

---

## 12. Testing Strategy

- Unit tests: slice-level business logic and validators.
- Integration tests: endpoint + EF Core in-memory/SQLite.
- Contract tests: provider adapters (ID&V, searches) against sandbox fixtures.
- E2E smoke: core flows (create conveyance → generate enquiry draft → approve).
- Acceptance: align to samples in MVP-PLAN.md §6; codify as integration tests where possible.

---

## 13. Implementation Plan (MVP Phases)

Phase 0 — Foundations (You are here)
- API scaffold (done), vertical slices (done), drafts workflow (done), EF Core + SQLite (done), Swagger (done), Anthropic integration (done).

Phase 1 — Preview Outbox & Enquiries
- Outbox entities + endpoints (list/edit/approve/send)
- TitleEnquiryAgent: move to matter corpus context; add citations; diffing on edits
- Audit log entries on all transitions

Phase 2 — Searches
- Search ordering endpoints; provider adapters; webhook ingestion
- Parser to findings JSON with citations; Draft Search Digest generation

Phase 3 — Documents & RoT
- Document upload/store; OCR + extraction service
- Report on Title drafts per section; compile to PDF after approval

Phase 4 — Timeline & SLA
- Event sourcing light; aggregate per matter; SLA heatmap

Cross-cutting
- AuthN/AuthZ; Observability; Migrations for Postgres; Dockerfile + CI

Multi-tenant tasks (parallel track)
- Introduce Tenant entity and provisioning endpoints.
- Add `TenantId` to Matter/Conveyance, Drafts, Outbox, Documents, Searches.
- Implement TenantResolver middleware + EF global query filters.
- Frontend: subdomain/path-based tenant routing, branding/theme loading.

---

## 14. Open Questions & Assumptions

- Which ID&V and search providers for pilot? (sandbox credentials needed)
- Hosting environment preferences (Azure vs AWS) and data residency constraints.
- Email bridge requirement in MVP vs Phase 2?
- Required doc templates for Contract/TR1 and e-sign provider choice.
- Client-facing copy and tone templates for enquiries and RoT.

---

## 15. Appendix — Current API Implementation Snapshot

- Health: GET `/health`
- Conveyances: POST `/conveyances`, GET `/conveyances/{id}`
- Drafts: POST `/conveyances/{id}/drafts/title-enquiries`, GET `/conveyances/{id}/drafts`, GET `/drafts/{id}`, POST `/drafts/{id}/approve`, POST `/drafts/{id}/reject`
- Agents: Anthropic preferred via `ANTHROPIC_API_KEY`; audit metadata persisted per draft
