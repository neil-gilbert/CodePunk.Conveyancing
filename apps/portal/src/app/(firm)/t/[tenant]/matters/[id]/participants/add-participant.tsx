"use client"
import { useState } from 'react'
import { Input } from '@/components/ui/input'
import { Button } from '@/components/ui/button'
import { apiFetch } from '@/lib/api/client'

type Role = 'Buyer' | 'Seller' | 'FeeEarner' | 'Lender' | 'Other'

export default function AddParticipantForm({ tenant, conveyanceId }: { tenant: string, conveyanceId: string }) {
  const [name, setName] = useState('')
  const [email, setEmail] = useState('')
  const [phone, setPhone] = useState('')
  const [role, setRole] = useState<Role>('Buyer')
  const [isClient, setIsClient] = useState(true)
  const [isPrimary, setIsPrimary] = useState(true)
  const [busy, setBusy] = useState(false)

  async function onSubmit(e: React.FormEvent) {
    e.preventDefault()
    setBusy(true)
    try {
      // 1) Create contact
      const contact = await apiFetch<any>(`/contacts`, tenant, {
        method: 'POST',
        body: JSON.stringify({ name, email, phone }),
      })
      // 2) Link to conveyance
      const roleMap: Record<Role, number> = { Buyer: 1, Seller: 2, FeeEarner: 3, Lender: 4, Other: 99 }
      await apiFetch(`/conveyances/${conveyanceId}/contacts`, tenant, {
        method: 'POST',
        body: JSON.stringify({ contactId: contact.id, role: roleMap[role], isClientOfTenant: isClient, isPrimary })
      })
      setName(''); setEmail(''); setPhone('')
      alert('Participant added')
      location.reload()
    } catch (e) {
      console.error(e)
      alert('Failed to add participant')
    } finally {
      setBusy(false)
    }
  }

  return (
    <form onSubmit={onSubmit} className="mt-4 border rounded p-4 space-y-3">
      <div className="text-sm font-semibold">Add Participant</div>
      <div>
        <label className="block text-sm mb-1">Name</label>
        <Input value={name} onChange={e => setName(e.target.value)} required />
      </div>
      <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
        <div>
          <label className="block text-sm mb-1">Email</label>
          <Input type="email" value={email} onChange={e => setEmail(e.target.value)} />
        </div>
        <div>
          <label className="block text-sm mb-1">Phone</label>
          <Input value={phone} onChange={e => setPhone(e.target.value)} />
        </div>
      </div>
      <div className="grid grid-cols-1 md:grid-cols-3 gap-3 items-end">
        <div>
          <label className="block text-sm mb-1">Role</label>
          <select className="h-9 w-full rounded-md border border-gray-300 bg-white px-3 text-sm" value={role} onChange={e => setRole(e.target.value as Role)}>
            {['Buyer','Seller','FeeEarner','Lender','Other'].map(r => <option key={r} value={r}>{r}</option>)}
          </select>
        </div>
        <label className="inline-flex items-center gap-2 text-sm">
          <input type="checkbox" checked={isClient} onChange={e => setIsClient(e.target.checked)} /> Client of firm
        </label>
        <label className="inline-flex items-center gap-2 text-sm">
          <input type="checkbox" checked={isPrimary} onChange={e => setIsPrimary(e.target.checked)} /> Primary
        </label>
      </div>
      <div className="flex justify-end">
        <Button type="submit" disabled={busy || !name}>Add</Button>
      </div>
    </form>
  )
}

