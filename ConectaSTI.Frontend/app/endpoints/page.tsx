'use client'

import { useState, useMemo } from 'react'
import { motion, AnimatePresence } from 'motion/react'
import { useIntegracoes } from '@/hooks/useIntegracoes'
import { useEndpoints } from '@/hooks/useEndpoints'
import type { EndPoint } from '@/lib/types'
import { verboToHttpMethod } from '@/lib/types'
import { EndpointCard } from '@/components/EndpointCard'
import { EndpointRegistrationForm } from '@/components/EndpointRegistrationForm'
import { MethodBadge } from '@/components/MethodBadge'
import { Button } from '@/components/ui/button'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import {
  PlusIcon,
  PlugIcon,
  PencilSimpleIcon,
  TrashIcon,
  ArrowSquareOutIcon,
} from '@phosphor-icons/react'
import { useRouter } from 'next/navigation'

type PanelMode = 'empty' | 'new' | 'edit' | 'detail'

export default function EndpointsPage() {
  const { integracoes } = useIntegracoes()
  const { endpoints, createEndpoint, updateEndpoint, deleteEndpoint } = useEndpoints()
  const router = useRouter()

  const [filterApiId, setFilterApiId] = useState<string>('all')
  const [selectedId, setSelectedId] = useState<number | null>(null)
  const [panelMode, setPanelMode] = useState<PanelMode>('empty')

  const filtered = useMemo(
    () => filterApiId === 'all' ? endpoints : endpoints.filter(e => e.integracaoId === Number(filterApiId)),
    [endpoints, filterApiId]
  )

  const grouped = useMemo(() => {
    const map = new Map<number, EndPoint[]>()
    for (const ep of filtered) {
      const list = map.get(ep.integracaoId) ?? []
      map.set(ep.integracaoId, [...list, ep])
    }
    return map
  }, [filtered])

  const selected = endpoints.find(e => e.id === selectedId) ?? null
  const selectedApi = selected ? integracoes.find(a => a.id === selected.integracaoId) : null

  async function handleSave(data: Omit<EndPoint, 'id' | 'criadoEm'>) {
    if (panelMode === 'edit' && selectedId) {
      const ep = await updateEndpoint(selectedId, data)
      setSelectedId(ep.id)
      setPanelMode('detail')
    } else {
      const ep = await createEndpoint(data)
      setSelectedId(ep.id)
      setPanelMode('detail')
    }
  }

  async function handleDelete(id: number) {
    if (!confirm('Remover este endpoint?')) return
    await deleteEndpoint(id)
    setSelectedId(null)
    setPanelMode('empty')
  }

  return (
    <div className="flex h-screen w-full overflow-hidden">
      {/* Left panel */}
      <div className="flex flex-col w-72 shrink-0 border-r border-border bg-white/60 backdrop-blur-sm">
        <div className="p-4 border-b border-border">
          <div className="flex items-center justify-between mb-3">
            <h1 className="text-sm font-semibold">Endpoints</h1>
            <Button
              size="sm"
              variant="outline"
              className="h-7 gap-1 text-xs rounded-none"
              onClick={() => { setSelectedId(null); setPanelMode('new') }}
            >
              <PlusIcon size={12} />
              Novo
            </Button>
          </div>
          <Select value={filterApiId} onValueChange={setFilterApiId}>
            <SelectTrigger className="rounded-none text-xs h-8">
              <SelectValue placeholder="Filtrar por API..." />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all" className="text-xs">Todas as APIs</SelectItem>
              {integracoes.map(api => (
                <SelectItem key={api.id} value={String(api.id)} className="text-xs font-mono">{api.nome}</SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>

        <div className="flex-1 overflow-y-auto">
          {filtered.length === 0 ? (
            <div className="p-6 text-center text-xs text-muted-foreground">
              Nenhum endpoint encontrado.
            </div>
          ) : (
            <motion.div
              className="p-2 space-y-3"
              initial="hidden"
              animate="visible"
              variants={{ visible: { transition: { staggerChildren: 0.04 } } }}
            >
              {Array.from(grouped.entries()).map(([apiId, epList]) => {
                const api = integracoes.find(a => a.id === apiId)
                return (
                  <div key={apiId}>
                    <div className="flex items-center justify-between px-2 py-1.5">
                      <span className="text-[10px] uppercase tracking-wider text-muted-foreground font-medium">
                        {api?.nome ?? 'API desconhecida'}
                      </span>
                      <span className="text-[10px] text-muted-foreground">{epList.length}</span>
                    </div>
                    <div className="space-y-1">
                      {epList.map(ep => (
                        <motion.div
                          key={ep.id}
                          variants={{ hidden: { opacity: 0, y: 6 }, visible: { opacity: 1, y: 0 } }}
                        >
                          <EndpointCard
                            endpoint={ep}
                            selected={selectedId === ep.id}
                            onClick={() => { setSelectedId(ep.id); setPanelMode('detail') }}
                            compact
                          />
                        </motion.div>
                      ))}
                    </div>
                  </div>
                )
              })}
            </motion.div>
          )}
        </div>
      </div>

      {/* Right panel */}
      <div className="flex-1 overflow-y-auto">
        <AnimatePresence mode="wait">
          {panelMode === 'empty' && (
            <motion.div
              key="empty"
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
              exit={{ opacity: 0 }}
              className="flex h-full flex-col items-center justify-center gap-4 text-center p-8"
            >
              <motion.div
                animate={{ rotate: [0, 8, -8, 0] }}
                transition={{ repeat: Infinity, duration: 4, ease: 'easeInOut' }}
                className="text-muted-foreground/40"
              >
                <PlugIcon size={56} weight="thin" />
              </motion.div>
              <div>
                <p className="text-sm font-medium text-muted-foreground">Nenhum endpoint selecionado</p>
                <p className="text-xs text-muted-foreground/60 mt-1">
                  Selecione um endpoint ou cadastre um novo.
                </p>
              </div>
              <Button
                size="sm"
                variant="outline"
                className="gap-1.5 rounded-none text-xs mt-2"
                onClick={() => setPanelMode('new')}
              >
                <PlusIcon size={12} />
                Cadastrar endpoint
              </Button>
            </motion.div>
          )}

          {(panelMode === 'new' || panelMode === 'edit') && (
            <motion.div key="form" initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }}>
              <EndpointRegistrationForm
                apis={integracoes}
                initial={panelMode === 'edit' ? selected ?? undefined : undefined}
                onSave={handleSave}
                onCancel={() => setPanelMode(selected ? 'detail' : 'empty')}
              />
            </motion.div>
          )}

          {panelMode === 'detail' && selected && (
            <motion.div
              key={`detail-${selected.id}`}
              initial={{ opacity: 0, x: 12 }}
              animate={{ opacity: 1, x: 0 }}
              exit={{ opacity: 0 }}
              transition={{ duration: 0.2 }}
              className="p-6 max-w-2xl"
            >
              <div className="flex items-start justify-between mb-6 pb-4 border-b">
                <div className="flex items-center gap-3">
                  <MethodBadge method={verboToHttpMethod(selected.verbo)} />
                  <div>
                    <h2 className="font-semibold text-base font-mono">{selected.recurso}</h2>
                    {selectedApi && (
                      <p className="text-xs text-muted-foreground mt-0.5">{selectedApi.nome}</p>
                    )}
                  </div>
                </div>
                <div className="flex gap-2">
                  <Button
                    size="sm"
                    variant="outline"
                    className="gap-1.5 rounded-none text-xs h-7"
                    onClick={() => setPanelMode('edit')}
                  >
                    <PencilSimpleIcon size={12} />
                    Editar
                  </Button>
                  <Button
                    size="sm"
                    variant="outline"
                    className="gap-1.5 rounded-none text-xs h-7 text-destructive hover:text-destructive"
                    onClick={() => handleDelete(selected.id)}
                  >
                    <TrashIcon size={12} />
                  </Button>
                </div>
              </div>

              <div className="space-y-5">
                <div className="grid grid-cols-2 gap-4">
                  <InfoBlock label="Método">
                    <MethodBadge method={verboToHttpMethod(selected.verbo)} />
                  </InfoBlock>
                  <InfoBlock label="API">
                    <div className="flex items-center gap-1.5">
                      <span className="text-xs">{selectedApi?.nome ?? '—'}</span>
                      {selectedApi && (
                        <button
                          onClick={() => router.push('/apis')}
                          className="text-muted-foreground hover:text-foreground transition-colors"
                          title="Ver API"
                        >
                          <ArrowSquareOutIcon size={12} />
                        </button>
                      )}
                    </div>
                  </InfoBlock>
                </div>

                <InfoBlock label="URL Completa">
                  <span className="font-mono text-xs text-muted-foreground">
                    {selectedApi?.url ?? ''}<span className="text-foreground">{selected.recurso}</span>
                  </span>
                </InfoBlock>

                <InfoBlock label="Descrição">
                  <p className="text-xs text-muted-foreground leading-relaxed">{selected.descricao ?? '—'}</p>
                </InfoBlock>
              </div>
            </motion.div>
          )}
        </AnimatePresence>
      </div>
    </div>
  )
}

function InfoBlock({ label, children }: { label: string; children: React.ReactNode }) {
  return (
    <div className="flex flex-col gap-1.5">
      <span className="text-[10px] uppercase tracking-wider text-muted-foreground font-medium">{label}</span>
      {children}
    </div>
  )
}
