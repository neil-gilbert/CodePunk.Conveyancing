# Frontend MVP Plan & Checklist

Status: Draft for collaboration
Owner: Engineering + Product
Scope: Multi-tenant solicitor/client portal (MVP)

---

## Goals
- Matter-first workflow with Client 360 overview.
- Human-in-the-loop for all AI outputs (drafts/outbox approvals).
- Multi-tenant branding, routing, and data isolation.
- Clean, modern, accessible, responsive UI.

---

## Information Architecture (MVP)
- Firm Users
  - Matters: list + detail (Overview, Participants, Drafts, Outbox, Documents, Timeline)
  - Drafts Review: list/detail, edit, approve/reject, audit metadata, diff
  - Preview Outbox: list, compose/edit from draft, recipients, approve, send
  - Contacts: list + detail (Contact 360: Overview, Conveyances, Documents, Messages)
  - Documents: upload, status, PDF viewer, extracted data panel
  - Admin (Tenant): branding, users/roles, SSO (minimal placeholder)
- Client Users
  - Checklist/Uploads, Messages, Status (read-only approved outputs)

---

## Tech Stack
- Next.js (App Router) + React 18 + TypeScript
- Tailwind CSS + CSS variables (per-tenant theming; light/dark)
- TanStack Query (data fetching/caching)
- Auth: next-auth (OIDC; central IdP, optional tenant SSO later)
- Forms: React Hook Form + Zod
- Testing: RTL + Vitest (components), Playwright (E2E), MSW (API mocking)

---

## Tenancy & Auth
- Routing: subdomain `{tenant}.portal.example.com`; dev fallback `/t/{tenant}` via middleware.
- Tenant resolution: middleware parses host/path; API client sends `X-Tenant` header.
- Branding: load tenant brand JSON at layout; set CSS variables (theme) and logo/favicon.
- Auth: OIDC via next-auth; map org/tenant claim; enforce tenant match on session.
- Security: CSRF (next-auth), secure cookies, signed URLs for file access, no cross-tenant state.

---

## UX Patterns
- Navigation: left rail (firm), simplified client hub.
- Lists: server-driven filters/sort, skeletons, empty states.
- Drafts: side-by-side diff, audit metadata (provider/model/tokens/latency).
- Outbox: compose from draft, recipients, approval gate, send feedback.
- Documents: progressive upload, PDF viewer with sidebar + extracted data panel.
- Accessibility: WCAG AA, keyboard shortcuts for approve/send.

---

## Data & API Integration
- Client modules: `lib/api/{conveyances,drafts,outbox,contacts}.ts`.
- Query keys include tenant slug and entity ids.
- Errors: map ProblemDetails to UI toasts and field errors.

---

## Performance & Observability
- Performance budget: TTI < 2s on primary views; list updates < 100ms.
- Caching: SWR with invalidation on approve/send.
- Telemetry: page views, API timings, handled errors; feature flags hooks.

---

## Testing Strategy (AGENTS.md aligned)
- Test-first: write failing test (RTL/Playwright) before implementation.
- Component tests: RTL + MSW for API.
- E2E: Playwright flows (login → matter → draft → approve → outbox send).
- Contract checks: minimal JSON shape assertions for stability.

---

## Delivery Plan (Small PRs)
Each item: prepare Technical Chunk, add failing tests, implement, pass CI, update docs.

1) Scaffold app shell
- Next.js + Tailwind + TanStack Query + next-auth + MSW + Playwright
- Tenancy middleware (host/path) and API client with `X-Tenant` header
- Themed layout; basic nav; placeholder pages

2) Auth + Session
- OIDC login/logout; session shows tenant/org; guard routes

3) Matters (list + detail shell)
- List/search/paging; detail tabs scaffold; Tenant guard

4) Participants (ConveyanceContact)
- Matter Participants tab: list + add/link existing Contact with Role

5) Drafts (review/approve/reject)
- Drafts list/detail; audit metadata panel; edit + diff; approve/reject actions

6) Preview Outbox
- List/detail; create from draft; recipients; approve/send; status feedback

7) Contacts (Client 360)
- Contacts list/detail; linked conveyances; create/link flows

8) Documents
- Upload UI with progress; list; PDF viewer; extracted data panel

9) Client Portal (minimal)
- Checklist/Uploads, Messages, Status (approved-only)

10) Tenant Admin (placeholder)
- Branding form; users/roles stub; SSO placeholder

---

## Decisions to Confirm
- OIDC provider: Auth0 (Organizations) vs Entra ID.
- UI kit: Tailwind-only vs Tailwind + shadcn/ui.
- Hosting: Vercel/Netlify/Azure (wildcard subdomains, cookie domains).

---

## Work Checklist (Tick as we go)

- [ ] Decide OIDC provider (Auth0 vs Entra ID)
- [ ] Decide UI kit (Tailwind-only vs Tailwind+shadcn)
- [ ] Decide hosting (and domain setup for subdomains)
- [ ] Scaffold Next.js app (App Router, TS)
- [ ] Add Tailwind + base theme tokens
- [ ] Add tenancy middleware (host/path) and API client with `X-Tenant`
- [ ] Add next-auth wiring for OIDC (env-driven)
- [ ] Add MSW + Playwright + RTL test harness
- [ ] Layout + navigation shell (firm + client variants)
- [ ] Matters list (fetch + filters + skeleton)
- [ ] Matter detail tabs scaffold
- [ ] Participants tab (list + add/link Contact)
- [ ] Drafts list/detail (audit panel; edit + diff)
- [ ] Approve/Reject actions wired to API
- [ ] Outbox list/detail (create from draft; recipients)
- [ ] Approve/Send actions wired to API
- [ ] Contacts list/detail + linked conveyances
- [ ] Documents upload + viewer + extracted data panel
- [ ] Client portal minimal pages (checklist/uploads/messages)
- [ ] Tenant admin placeholder (branding/users)
- [ ] E2E happy-path: login → create conveyance → draft → approve → outbox send (mocked API)
- [ ] Accessibility audit (key screens) and perf pass
- [ ] Update docs and record open issues for Phase 2 (Search digest, RoT)

---

## Notes for PRs (AGENTS.md)
- Always create a PR; do not push directly to `main`.
- Run tests locally (`pnpm test`, `pnpm test:e2e`) before PR/push.
- Keep PRs small, behavior-focused; include Technical Chunk in description.
- Align feature code with vertical slices; tests at API/UI boundaries.

