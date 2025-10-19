import Link from 'next/link'
import type { ReactNode } from 'react'

export default function TenantLayout(
  { children, params }: { children: ReactNode, params: { tenant: string } }
) {
  const tenant = params.tenant
  return (
    <div className="app-container">
      <aside className="sidebar p-4">
        <div className="mb-6">
          <div className="text-xs uppercase text-gray-500">Tenant</div>
          <div className="font-semibold">{tenant}</div>
        </div>
        <nav className="space-y-2">
          <Link className="block hover:underline" href={`/t/${tenant}`}>Overview</Link>
          <Link className="block hover:underline" href={`/t/${tenant}/matters`}>Matters</Link>
          <Link className="block hover:underline" href={`/t/${tenant}/drafts`}>Drafts</Link>
          <Link className="block hover:underline" href={`/t/${tenant}/outbox`}>Outbox</Link>
          <Link className="block hover:underline" href={`/t/${tenant}/contacts`}>Contacts</Link>
          <Link className="block hover:underline" href={`/t/${tenant}/documents`}>Documents</Link>
        </nav>
      </aside>
      <main className="content">
        {children}
      </main>
    </div>
  )
}

