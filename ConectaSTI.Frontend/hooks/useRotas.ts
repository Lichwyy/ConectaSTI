'use client'

import { useCallback, useEffect, useState } from 'react'
import type { Rota } from '@/lib/types'
import * as svc from '@/lib/api/rotas'

export function useRotas() {
  const [rotas, setRotas] = useState<Rota[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const load = useCallback(async () => {
    try {
      setLoading(true)
      setError(null)
      setRotas(await svc.getRotas())
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Erro ao carregar rotas')
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    void load()
  }, [load])

  const createRota = useCallback(async (data: svc.RotaPayload) => {
    const rota = await svc.createRota(data)
    setRotas(prev => [...prev, rota])
    return rota
  }, [])

  const updateRota = useCallback(async (id: number, data: svc.RotaPayload) => {
    const rota = await svc.updateRota(id, data)
    setRotas(prev => prev.map(item => item.id === id ? rota : item))
    return rota
  }, [])

  const deleteRota = useCallback(async (id: number) => {
    await svc.deleteRota(id)
    setRotas(prev => prev.filter(item => item.id !== id))
  }, [])

  return { rotas, loading, error, createRota, updateRota, deleteRota, reload: load }
}
