"use client"
import { useState } from 'react'
import { Dialog, DialogFooter, DialogHeader, DialogTitle } from '@/components/ui/dialog'
import { Input } from '@/components/ui/input'
import { Button } from '@/components/ui/button'
import { apiFetch } from '@/lib/api/client'
import { useRouter } from 'next/navigation'

export default function CreateMatter({ tenant }: { tenant: string }) {
  const [open, setOpen] = useState(false)
  const [buyerName, setBuyerName] = useState('')
  const [sellerName, setSellerName] = useState('')
  const [propertyAddress, setPropertyAddress] = useState('')
  const [busy, setBusy] = useState(false)
  const router = useRouter()

  async function onCreate() {
    setBusy(true)
    try {
      const created = await apiFetch<any>(`/conveyances`, tenant, {
        method: 'POST',
        body: JSON.stringify({ buyerName, sellerName, propertyAddress })
      })
      setOpen(false)
      setBuyerName(''); setSellerName(''); setPropertyAddress('')
      router.push(`/t/${tenant}/matters/${created.id}/participants`)
    } catch (e) {
      console.error(e)
      alert('Failed to create matter')
    } finally {
      setBusy(false)
    }
  }

  return (
    <div>
      <Button onClick={() => setOpen(true)}>Create Matter</Button>
      <Dialog open={open} onOpenChange={setOpen}>
        <DialogHeader>
          <DialogTitle>Create a new Matter</DialogTitle>
        </DialogHeader>
        <div className="space-y-3">
          <div>
            <label className="block text-sm mb-1">Buyer name</label>
            <Input value={buyerName} onChange={e => setBuyerName(e.target.value)} />
          </div>
          <div>
            <label className="block text-sm mb-1">Seller name</label>
            <Input value={sellerName} onChange={e => setSellerName(e.target.value)} />
          </div>
          <div>
            <label className="block text-sm mb-1">Property address</label>
            <Input value={propertyAddress} onChange={e => setPropertyAddress(e.target.value)} />
          </div>
        </div>
        <DialogFooter>
          <Button variant="outline" onClick={() => setOpen(false)} disabled={busy}>Cancel</Button>
          <Button onClick={onCreate} disabled={busy || !buyerName || !sellerName || !propertyAddress}>Create</Button>
        </DialogFooter>
      </Dialog>
    </div>
  )
}

