'use client'

import { useState, useEffect, useCallback } from 'react'
import type { Fluxo } from '@/lib/types'
import * as svc from '@/lib/api/fluxos'

export function useFluxos() {
  const [fluxos, setFluxos] = useState<Fluxo[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const load = useCallback(async () => {
    try {
      setLoading(true)
      setError(null)
      setFluxos(await svc.getFluxos())
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Erro ao carregar fluxos')
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => { load() }, [load])

  const createFluxo = useCallback(async (nome: string) => {
    const item = await svc.createFluxo(nome)
    setFluxos(prev => [...prev, item])
    return item
  }, [])

  const updateFluxo = useCallback(async (id: number, data: { nome: string }) => {
    const item = await svc.updateFluxo(id, data)
    setFluxos(prev => prev.map(f => f.id === id ? item : f))
    return item
  }, [])

  const deleteFluxo = useCallback(async (id: number) => {
    await svc.deleteFluxo(id)
    setFluxos(prev => prev.filter(f => f.id !== id))
  }, [])

  return { fluxos, loading, error, createFluxo, updateFluxo, deleteFluxo, reload: load }
}
