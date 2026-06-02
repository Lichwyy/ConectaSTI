'use client'

import { useState, useEffect, useCallback } from 'react'
import type { Funcao } from '@/lib/types'
import * as svc from '@/lib/api/funcoes'

export function useFuncoes() {
  const [funcoes, setFuncoes] = useState<Funcao[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const load = useCallback(async () => {
    try {
      setLoading(true)
      setError(null)
      setFuncoes(await svc.getFuncoes())
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Erro ao carregar funções')
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => { load() }, [load])

  const createFuncao = useCallback(async (data: Omit<Funcao, 'id' | 'criadoEm'>) => {
    const item = await svc.createFuncao(data)
    setFuncoes(prev => [...prev, item])
    return item
  }, [])

  const updateFuncao = useCallback(async (id: number, data: Omit<Funcao, 'id' | 'criadoEm'>) => {
    const item = await svc.updateFuncao(id, data)
    setFuncoes(prev => prev.map(f => f.id === id ? item : f))
    return item
  }, [])

  const deleteFuncao = useCallback(async (id: number) => {
    await svc.deleteFuncao(id)
    setFuncoes(prev => prev.filter(f => f.id !== id))
  }, [])

  return { funcoes, loading, error, createFuncao, updateFuncao, deleteFuncao, reload: load }
}
