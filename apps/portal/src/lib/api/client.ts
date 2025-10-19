export type ApiClientOptions = {
  baseUrl?: string
}

export async function apiFetch<T>(path: string, tenant: string, init?: RequestInit, options?: ApiClientOptions): Promise<T> {
  const base = options?.baseUrl || process.env.NEXT_PUBLIC_API_BASE || 'http://localhost:5228/api'
  const url = `${base}${path}`
  const headers = new Headers(init?.headers)
  headers.set('Content-Type', 'application/json')
  headers.set('X-Tenant', tenant)
  const resp = await fetch(url, { ...init, headers, cache: 'no-store' })
  if (!resp.ok) {
    const text = await resp.text().catch(() => '')
    throw new Error(`API ${resp.status} ${resp.statusText}: ${text}`)
  }
  if (resp.status === 204) return undefined as unknown as T
  return resp.json() as Promise<T>
}

export type Conveyance = {
  id: string
  buyerName: string
  sellerName: string
  propertyAddress: string
  createdUtc: string
}

