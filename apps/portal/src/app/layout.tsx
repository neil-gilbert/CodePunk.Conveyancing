import './globals.css'
import type { ReactNode } from 'react'

export const metadata = {
  title: 'Conveyancing Portal',
  description: 'Multi-tenant portal for conveyancing matters',
}

export default function RootLayout({ children }: { children: ReactNode }) {
  return (
    <html lang="en">
      <body>{children}</body>
    </html>
  )
}

