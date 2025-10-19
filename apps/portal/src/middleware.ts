import type { NextRequest } from 'next/server'
import { NextResponse } from 'next/server'

export function middleware(req: NextRequest) {
  // For now, we don’t rewrite – we just allow /t/{tenant} routing.
  // Subdomain parsing can be added later to set a cookie or header for client-side fetches.
  return NextResponse.next()
}

export const config = {
  matcher: ['/((?!_next/static|_next/image|favicon.ico).*)'],
}

