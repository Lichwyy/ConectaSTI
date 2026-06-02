'use client'

import { useState, useEffect, useCallback } from 'react'
import type { EndPoint } from '@/lib/types'
import * as svc from '@/lib/api/endpoints'

export function useEndpoints(integracaoId?: number) {
  const [endpoints, setEndpoints] = useState<EndPoint[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const load = useCallback(async () => {
    try {
      setLoading(true)
      setError(null)
      setEndpoints(await svc.getEndPoints(integracaoId))
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Erro ao carregar endpoints')
    } finally {
      setLoading(false)
    }
  }, [integracaoId])

  useEffect(() => { load() }, [load])

  const createEndpoint = useCallback(async (data: Omit<EndPoint, 'id' | 'criadoEm'>) => {
    const ep = await svc.createEndPoint(data)
    setEndpoints(prev => [...prev, ep])
    return ep
  }, [])

  const updateEndpoint = useCallback(async (id: number, data: Omit<EndPoint, 'id' | 'criadoEm'>) => {
    const ep = await svc.updateEndPoint(id, data)
    setEndpoints(prev => prev.map(e => e.id === id ? ep : e))
    return ep
  }, [])

  const deleteEndpoint = useCallback(async (id: number) => {
    await svc.deleteEndPoint(id)
    setEndpoints(prev => prev.filter(e => e.id !== id))
  }, [])

  return { endpoints, loading, error, createEndpoint, updateEndpoint, deleteEndpoint, reload: load }
}
