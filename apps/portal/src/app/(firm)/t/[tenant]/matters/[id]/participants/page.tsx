import { apiFetch } from '@/lib/api/client'

async function getParticipants(tenant: string, id: string) {
  return apiFetch<any[]>(`/conveyances/${id}/contacts`, tenant)
}

export default async function ParticipantsPage({ params }: { params: { tenant: string, id: string } }) {
  const { tenant, id } = params
  const participants = await getParticipants(tenant, id).catch(() => [])

  return (
    <div>
      <h1 className="text-2xl font-bold">Participants</h1>
      <p className="text-gray-600 mt-2">Conveyance ID: {id}</p>
      <div className="mt-4 space-y-2">
        {participants.length === 0 && (
          <div className="text-sm text-gray-500">No participants yet.</div>
        )}
        {participants.map(p => (
          <div key={p.id} className="border rounded p-3">
            <div className="font-semibold">{p.contact?.name ?? 'Unknown'}</div>
            <div className="text-sm text-gray-600">Role: {p.role} {p.isClientOfTenant ? '(Client)' : ''} {p.isPrimary ? '(Primary)' : ''}</div>
          </div>
        ))}
      </div>
    </div>
  )}

