# Frontend Architecture (MVP)

Status: Proposal

## Goals
- Multi-tenant portal for firms and their clients with tenant-based branding.
- Role-based UX with Preview Outbox and approvals at the center.
- Fast perceived performance; offline-friendly where feasible.

## Stack
- Next.js (App Router) + React 18
- TypeScript, ESLint, Prettier
- Styling: Tailwind CSS + CSS variables for theming
- Data: REST (initial) via TanStack Query; evolve to tRPC/GraphQL if needed
- Auth: next-auth (OIDC provider) with tenant-aware session (org claim)
- Forms: React Hook Form + Zod

## Tenancy & Routing
- Subdomain per tenant: `{tenant}.portal.example.com` (preferred)
- Dev fallback: path segment `/t/{tenant}`
- Tenant resolution flow:
  - On request, read host/path → resolve tenant → fetch tenant config (theme, name)
  - Inject theme CSS variables and logo at layout level
- Custom domains (optional): lookup by host header

## Roles & Navigation
- Roles: `tenant_admin`, `fee_earner`, `supervisor`, `compliance`, `client_buyer`, `client_seller`
- Layouts:
  - Firm users: Matters, Drafts, Outbox, Documents, Timeline
  - Clients: Tasks/Checklist, Documents, Messages, Status

## Key Screens (MVP)
- Login + Tenant selection (if no subdomain)
- Matters list/detail (firm users)
- Drafts list/detail with audit metadata display
- Preview Outbox list/detail (edit/approve/send)
- Searches digest view (phase 2)
- RoT draft review (phase 3)

## API Integration
- Use `/api` endpoints; include tenant slug header `X-Tenant` when path-based
- Error handling via ProblemDetails; show validation and auth errors consistently
- Caching: query keys include tenant+matter ids; invalidate on approve/send

## Theming & Branding
- Theme tokens in CSS variables: primary/neutral palette, surface, border
- Per-tenant brand JSON fetched at layout render; persisted client-side for quick boot
- Logo & favicon from tenant config

## Security
- Session contains tenant/org claim; all API calls include access token
- CSRF handled by next-auth; secure cookies with SameSite=Lax/Strict
- Prevent tenant switching without explicit context reset/log-out

## Observability
- Client-side telemetry (basic): page views, API timings (aggregated), error boundaries
- Feature flags for gradual rollout (e.g., Drafts v2)

## Delivery
- Deployed as static assets on CDN + Node SSR where needed
- Env-configurable API base URL; supports multiple regions
- CI: typecheck, lint, build, smoke tests
