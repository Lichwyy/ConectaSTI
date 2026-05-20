'use client'

import { useState, useEffect, useCallback } from 'react'
import type { Integracao } from '@/lib/types'
import * as svc from '@/lib/api/integracoes'

export function useIntegracoes() {
  const [integracoes, setIntegracoes] = useState<Integracao[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const load = useCallback(async () => {
    try {
      setLoading(true)
      setError(null)
      setIntegracoes(await svc.getIntegracoes())
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Erro ao carregar integrações')
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => { load() }, [load])

  const createIntegracao = useCallback(async (data: Omit<Integracao, 'id' | 'criadoEm'>) => {
    const item = await svc.createIntegracao(data)
    setIntegracoes(prev => [...prev, item])
    return item
  }, [])

  const updateIntegracao = useCallback(async (id: number, data: Omit<Integracao, 'id' | 'criadoEm'>) => {
    const item = await svc.updateIntegracao(id, data)
    setIntegracoes(prev => prev.map(a => a.id === id ? item : a))
    return item
  }, [])

  const deleteIntegracao = useCallback(async (id: number) => {
    await svc.deleteIntegracao(id)
    setIntegracoes(prev => prev.filter(a => a.id !== id))
  }, [])

  return { integracoes, loading, error, createIntegracao, updateIntegracao, deleteIntegracao, reload: load }
}
