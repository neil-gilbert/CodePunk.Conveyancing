"use client"
import * as React from 'react'
import { cn } from '@/lib/utils'

type DialogProps = {
  open: boolean
  onOpenChange: (open: boolean) => void
  children: React.ReactNode
}

export function Dialog({ open, onOpenChange, children }: DialogProps) {
  return (
    <div className={cn('fixed inset-0 z-50', open ? '' : 'hidden')}>
      <div className="absolute inset-0 bg-black/30" onClick={() => onOpenChange(false)} />
      <div className="relative mx-auto mt-20 w-full max-w-lg rounded-lg bg-white p-4 shadow-lg">
        {children}
      </div>
    </div>
  )
}

export function DialogHeader({ children }: { children: React.ReactNode }) {
  return <div className="mb-2 border-b pb-2">{children}</div>
}

export function DialogTitle({ children }: { children: React.ReactNode }) {
  return <h2 className="text-lg font-semibold">{children}</h2>
}

export function DialogFooter({ children }: { children: React.ReactNode }) {
  return <div className="mt-4 flex justify-end gap-2">{children}</div>
}

