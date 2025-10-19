import { apiFetch, type Conveyance } from '@/lib/api/client'

export default async function MattersPage({ params }: { params: { tenant: string } }) {
  const tenant = params.tenant
  // For MVP, there is no list endpoint; show an instruction and example with a single fetch if needed.
  // This page will grow once /api/conveyances?search=... is available.
  let sample: Conveyance | null = null
  try {
    // Example: try a non-existent id to demonstrate error handling gracefully
    await apiFetch(`/conveyances/00000000-0000-0000-0000-000000000000`, tenant)
  } catch {
    sample = null
  }

  return (
    <div>
      <h1 className="text-2xl font-bold mb-4">Matters</h1>
      <p className="text-gray-600">Search and manage matters for tenant <span className="font-semibold">{tenant}</span>.</p>
      {sample && (
        <div className="mt-4 border rounded p-4">
          <div className="font-semibold">Example Conveyance</div>
          <div className="text-sm text-gray-600">{sample.propertyAddress}</div>
        </div>
      )}
      <div className="mt-6 text-sm text-gray-500">
        This is a scaffold. We will hook this up to a real list endpoint with filters and pagination.
      </div>
    </div>
  )
}

