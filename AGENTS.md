# AGENTS.md — Working Agreements for This Repo

This file is for humans and AI coding agents. It defines how we work in this repository. Root-level AGENTS.md applies to the whole repo. If a deeper directory adds its own AGENTS.md, that one takes precedence for files in its subtree.

Key reminders for agents: always read AGENTS.md files; follow test-first and vertical-slice conventions; keep PRs small and behavior-focused.

## 1) Test-First, Behavior-Driven, API-Level

- Always write a failing test first (red → green → refactor).
- Tests are behavior-driven and black-box at the API boundary:
  - Use xUnit + Shouldly
  - Use WebApplicationFactory and in-memory SQLite
  - Exercise HTTP request/response only; avoid mocking internals
  - Assert on status codes, response shapes, and observable behaviors
- Multi-tenant tests must include tenant context:
  - Prefer unique tenant slugs per test (e.g., t{Guid})
  - Provide `X-Tenant: {slug}` header, or use `/t/{slug}` path
- Name tests by behavior/scenario, not implementation details.

## 2) Vertical Slice Architecture (Feature-first)

- Structure:
  - `Features/<Area>/<UseCase>/...` — endpoint mappers + request/response records
  - `Domain/` — entities/enums/value objects
  - `Data/` — DbContext, mappings
  - `Infrastructure/` — adapters (tenancy, external providers, storage)
  - `Agents/` — agent DI + implementations
- Endpoints pattern: static extension methods on `IEndpointRouteBuilder` that `MapGroup()` and register routes.
- DTOs and validations live within the slice; keep cross-slice coupling minimal.
- Persistence via EF Core; keep usage simple inside slices; avoid leaking DbContext across unrelated slices.
- Tenancy: all tenant-owned entities include `TenantId`; EF global query filters enforce `TenantId == CurrentTenant` on reads; all writes must set `TenantId` from the request context.

## 3) Human-in-the-Loop by Design (Agents)

- Agents NEVER send outputs directly to clients.
- Agents produce `DraftDocument`s which must be reviewed/approved by solicitors.
- Use Preview Outbox for edits/approvals/sending.
- Include audit metadata for all drafts (provider, model, latency, prompt hash, tokens when available).
- Provider default: Anthropic if configured; others are pluggable.

## 4) Technical Chunk + Small PRs

- New work must be agreed as a “Technical Chunk” before coding. Capture:
  - Problem statement and scope
  - Endpoints (paths, verbs), request/response schemas
  - Data changes (entities/fields, migrations)
  - Tests to add (behavior scenarios)
  - Risks/assumptions and rollout plan
- Keep PRs small and focused (ideally < 400 LOC diff excluding generated code).
- Each PR must:
  - Include tests (failing first, then passing)
  - Pass CI (build + tests)
  - Update docs where relevant (README, TECHNICAL-REQUIREMENTS.md, etc.)
  - Include a clear, scannable description following the Technical Chunk

## 5) Database & Migrations

- Dev: SQLite with `EnsureCreated()` is acceptable while iterating.
- Prod: Postgres with EF Core migrations and `Migrate()` on startup. Do not mix `EnsureCreated()` with migrations on the same DB.
- See `docs/DB-MIGRATIONS.md`.

## 6) CI/CD

- All tests must pass in CI. Add/adjust tests as part of your change.
- If adding tooling (lint/format), include config and run in CI.

## 7) Definition of Done (Checklist)

- [ ] Technical Chunk agreed
- [ ] Failing test added at API boundary
- [ ] Implementation added under correct vertical slice
- [ ] Tenancy handled (TenantId on writes; tests include tenant context)
- [ ] Tests pass locally and in CI
- [ ] Docs updated (if needed)
- [ ] Small, focused PR with clear description

## 8) Quick Reference

- Multi-tenancy: resolve via subdomain or `X-Tenant`/`/t/{slug}`; enforce via global EF filters
- Tests: xUnit + Shouldly + WebApplicationFactory + in-memory SQLite
- Agents: drafts-only; no direct chat endpoints; audit metadata required
- Architecture: vertical slices, minimal APIs, EF Core

## 9) About this File

This is the file I (the coding agent) always read for repo-specific working agreements. Place additional AGENTS.md files in subfolders to override or augment guidance for those areas.

