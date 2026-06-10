'use client'

import { useCallback, useEffect, useState } from 'react'
import type { FluxoVersionado } from '@/lib/types'
import { getFluxosVersionados } from '@/lib/api/fluxos-versionados'

export function useFluxosVersionados() {
  const [fluxosVersionados, setFluxosVersionados] = useState<FluxoVersionado[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const load = useCallback(async () => {
    try {
      setLoading(true)
      setError(null)
      setFluxosVersionados(await getFluxosVersionados())
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Erro ao carregar versões de workflow')
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    void load()
  }, [load])

  return { fluxosVersionados, loading, error, reload: load }
}
