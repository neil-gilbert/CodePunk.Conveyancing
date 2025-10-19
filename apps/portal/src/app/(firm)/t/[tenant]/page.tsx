export default function TenantHome({ params }: { params: { tenant: string } }) {
  return (
    <div>
      <h1 className="text-2xl font-bold">Overview</h1>
      <p className="text-gray-600 mt-2">Welcome to {params.tenant}. Use the nav to access Matters, Drafts, Outbox, and Contacts.</p>
    </div>
  )
}

