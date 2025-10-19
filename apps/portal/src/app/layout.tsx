import './globals.css'
import type { ReactNode } from 'react'
import { getServerSession } from 'next-auth/next'
import { authOptions } from '@/lib/auth-options'
import Link from 'next/link'

export const metadata = {
  title: 'Conveyancing Portal',
  description: 'Multi-tenant portal for conveyancing matters',
}

export default async function RootLayout({ children }: { children: ReactNode }) {
  const session = await getServerSession(authOptions)
  return (
    <html lang="en">
      <body>
        <header className="w-full border-b border-gray-200 p-3 flex items-center justify-between">
          <Link href="/" className="font-semibold">Conveyancing Portal</Link>
          <nav className="text-sm">
            {session ? (
              <form action="/api/auth/signout" method="post">
                <button className="underline" type="submit">Sign out</button>
              </form>
            ) : (
              <a className="underline" href="/api/auth/signin">Sign in</a>
            )}
          </nav>
        </header>
        {children}
      </body>
    </html>
  )
}
