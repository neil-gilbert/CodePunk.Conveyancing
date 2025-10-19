# CodePunk Conveyancing — Frontend (Portal)

This is the multi-tenant Next.js portal for solicitors and their clients.

## Quick Start

- Ensure the backend API is running locally (default: http://localhost:5228)
- Install dependencies: `npm install` (or `pnpm i` / `yarn`)
- Run dev: `npm run dev`
- Navigate to:
  - `http://localhost:3000/` (home)
  - `http://localhost:3000/t/{tenant}` (tenant shell)

Set `NEXT_PUBLIC_API_BASE` to point at your API base URL if not default.

## Tenancy

- Dev routing uses `/t/{tenant}`. Subdomain routing can be added later.
- API requests include `X-Tenant: {tenant}` header via the fetch wrapper.

## Tech Stack

- Next.js (App Router) + React + TypeScript
- Tailwind CSS (per-tenant theming via CSS variables)
- TanStack Query (to be added when needed)
- next-auth (OIDC to be added)

## Testing

- Component tests: `npm run test`
- E2E tests (Playwright): `npm run test:e2e`

Note: Install dev dependencies before running tests.
