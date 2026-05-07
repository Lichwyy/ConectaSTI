'use client'

import { useState, useMemo } from 'react'
import { motion, AnimatePresence } from 'motion/react'
import { useIntegracoes } from '@/hooks/useIntegracoes'
import { useEndpoints } from '@/hooks/useEndpoints'
import type { Integracao, EndPoint } from '@/lib/types'
import { verboToHttpMethod } from '@/lib/types'
import { ApiCard } from '@/components/ApiCard'
import { EndpointCard } from '@/components/EndpointCard'
import { ApiRegistrationForm } from '@/components/ApiRegistrationForm'
import { EndpointRegistrationForm } from '@/components/EndpointRegistrationForm'
import { MethodBadge } from '@/components/MethodBadge'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import {
  PlusIcon,
  MagnifyingGlassIcon,
  LinkSimpleIcon,
  LockKeyIcon,
  PencilSimpleIcon,
  TrashIcon,
  PlugsIcon,
} from '@phosphor-icons/react'

type PanelMode = 'empty' | 'new-api' | 'edit-api' | 'detail' | 'new-endpoint'

export default function ApisPage() {
  const { integracoes, loading, createIntegracao, updateIntegracao, deleteIntegracao } = useIntegracoes()
  const { endpoints, createEndpoint, deleteEndpoint } = useEndpoints()

  const [search, setSearch] = useState('')
  const [selectedId, setSelectedId] = useState<number | null>(null)
  const [panelMode, setPanelMode] = useState<PanelMode>('empty')

  const filtered = useMemo(
    () => integracoes.filter(a =>
      a.nome.toLowerCase().includes(search.toLowerCase()) ||
      a.url.toLowerCase().includes(search.toLowerCase())
    ),
    [integracoes, search]
  )

  const selected = integracoes.find(a => a.id === selectedId) ?? null
  const selectedEndpoints = endpoints.filter(e => e.integracaoId === selectedId)

  function selectApi(api: Integracao) {
    setSelectedId(api.id)
    setPanelMode('detail')
  }

  async function handleSaveApi(data: Omit<Integracao, 'id' | 'criadoEm'>) {
    if (panelMode === 'edit-api' && selectedId) {
      await updateIntegracao(selectedId, data)
      setPanelMode('detail')
    } else {
      const api = await createIntegracao(data)
      setSelectedId(api.id)
      setPanelMode('detail')
    }
  }

  async function handleSaveEndpoint(data: Omit<EndPoint, 'id' | 'criadoEm'>) {
    await createEndpoint(data)
    setPanelMode('detail')
  }

  async function handleDeleteApi(id: number) {
    if (!confirm('Remover esta API? Os endpoints vinculados também serão removidos.')) return
    await deleteIntegracao(id)
    setSelectedId(null)
    setPanelMode('empty')
  }

  return (
    <div className="flex h-screen w-full overflow-hidden">
      {/* Left panel */}
      <div className="flex flex-col w-72 shrink-0 border-r border-border bg-white/60 backdrop-blur-sm">
        <div className="p-4 border-b border-border">
          <div className="flex items-center justify-between mb-3">
            <h1 className="text-sm font-semibold">APIs</h1>
            <Button
              size="sm"
              variant="outline"
              className="h-7 gap-1 text-xs rounded-none"
              onClick={() => { setSelectedId(null); setPanelMode('new-api') }}
            >
              <PlusIcon size={12} />
              Nova API
            </Button>
          </div>
          <div className="relative">
            <MagnifyingGlassIcon size={13} className="absolute left-2.5 top-1/2 -translate-y-1/2 text-muted-foreground" />
            <Input
              value={search}
              onChange={e => setSearch(e.target.value)}
              placeholder="Buscar APIs..."
              className="pl-7 h-7 text-xs rounded-none"
            />
          </div>
        </div>

        <div className="flex-1 overflow-y-auto">
          {loading ? (
            <div className="p-4 space-y-2">
              {[1, 2, 3].map(i => (
                <div key={i} className="h-20 bg-muted/40 animate-pulse" />
              ))}
            </div>
          ) : filtered.length === 0 ? (
            <div className="p-6 text-center text-xs text-muted-foreground">
              {search ? 'Nenhuma API encontrada.' : 'Nenhuma API cadastrada.'}
            </div>
          ) : (
            <motion.div
              className="p-2 space-y-1.5"
              initial="hidden"
              animate="visible"
              variants={{ visible: { transition: { staggerChildren: 0.05 } } }}
            >
              {filtered.map(api => (
                <motion.div
                  key={api.id}
                  variants={{ hidden: { opacity: 0, y: 8 }, visible: { opacity: 1, y: 0 } }}
                >
                  <ApiCard
                    api={api}
                    endpoints={endpoints.filter(e => e.integracaoId === api.id)}
                    selected={selectedId === api.id}
                    onClick={() => selectApi(api)}
                  />
                </motion.div>
              ))}
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
                animate={{ y: [0, -6, 0] }}
                transition={{ repeat: Infinity, duration: 3, ease: 'easeInOut' }}
                className="text-muted-foreground/40"
              >
                <PlugsIcon size={56} weight="thin" />
              </motion.div>
              <div>
                <p className="text-sm font-medium text-muted-foreground">Nenhuma API selecionada</p>
                <p className="text-xs text-muted-foreground/60 mt-1">
                  Selecione uma API na lista ou cadastre uma nova.
                </p>
              </div>
              <Button
                size="sm"
                variant="outline"
                className="gap-1.5 rounded-none text-xs mt-2"
                onClick={() => setPanelMode('new-api')}
              >
                <PlusIcon size={12} />
                Cadastrar primeira API
              </Button>
            </motion.div>
          )}

          {(panelMode === 'new-api' || panelMode === 'edit-api') && (
            <motion.div key="api-form" initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }}>
              <ApiRegistrationForm
                initial={panelMode === 'edit-api' ? selected ?? undefined : undefined}
                onSave={handleSaveApi}
                onCancel={() => setPanelMode(selected ? 'detail' : 'empty')}
              />
            </motion.div>
          )}

          {panelMode === 'new-endpoint' && selected && (
            <motion.div key="ep-form" initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }}>
              <EndpointRegistrationForm
                apis={integracoes}
                prefilledApiId={selected.id}
                onSave={handleSaveEndpoint}
                onCancel={() => setPanelMode('detail')}
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
                <div>
                  <h2 className="font-semibold text-base">{selected.nome}</h2>
                  <div className="flex items-center gap-1.5 text-xs text-muted-foreground mt-1">
                    <LinkSimpleIcon size={12} />
                    <span className="font-mono">{selected.url}</span>
                  </div>
                </div>
                <div className="flex gap-2">
                  <Button
                    size="sm"
                    variant="outline"
                    className="gap-1.5 rounded-none text-xs h-7"
                    onClick={() => setPanelMode('edit-api')}
                  >
                    <PencilSimpleIcon size={12} />
                    Editar
                  </Button>
                  <Button
                    size="sm"
                    variant="outline"
                    className="gap-1.5 rounded-none text-xs h-7 text-destructive hover:text-destructive"
                    onClick={() => handleDeleteApi(selected.id)}
                  >
                    <TrashIcon size={12} />
                  </Button>
                </div>
              </div>

              <div className="grid grid-cols-2 gap-4 mb-6">
                <InfoBlock label="URL Base">
                  <span className="font-mono text-xs">{selected.url}</span>
                </InfoBlock>
                <InfoBlock label="Autenticação">
                  {selected.token ? (
                    <span className="inline-flex items-center gap-1 text-xs text-amber-700">
                      <LockKeyIcon size={12} />
                      Token configurado
                    </span>
                  ) : (
                    <span className="text-xs text-muted-foreground">Sem token</span>
                  )}
                </InfoBlock>
                <InfoBlock label="Descrição" className="col-span-2">
                  <p className="text-xs text-muted-foreground">{selected.descricao ?? '—'}</p>
                </InfoBlock>
                <InfoBlock label="Criado em">
                  <span className="text-xs text-muted-foreground">
                    {selected.criadoEm
                      ? new Date(selected.criadoEm).toLocaleDateString('pt-BR', { day: '2-digit', month: 'short', year: 'numeric' })
                      : '—'}
                  </span>
                </InfoBlock>
              </div>

              <div className="border-t pt-5">
                <div className="flex items-center justify-between mb-3">
                  <h3 className="text-xs font-semibold uppercase tracking-wider text-muted-foreground">
                    Endpoints ({selectedEndpoints.length})
                  </h3>
                  <Button
                    size="sm"
                    variant="outline"
                    className="h-7 gap-1 text-xs rounded-none"
                    onClick={() => setPanelMode('new-endpoint')}
                  >
                    <PlusIcon size={12} />
                    Adicionar Endpoint
                  </Button>
                </div>

                {selectedEndpoints.length === 0 ? (
                  <div className="border border-dashed border-border p-6 text-center">
                    <p className="text-xs text-muted-foreground">Nenhum endpoint cadastrado.</p>
                    <button
                      onClick={() => setPanelMode('new-endpoint')}
                      className="text-xs text-primary underline underline-offset-2 mt-1"
                    >
                      Adicionar o primeiro
                    </button>
                  </div>
                ) : (
                  <div className="space-y-1.5">
                    {selectedEndpoints.map(ep => (
                      <div key={ep.id} className="flex items-center gap-2">
                        <div className="flex-1">
                          <EndpointCard
                            endpoint={ep}
                            onDelete={() => deleteEndpoint(ep.id)}
                            compact={false}
                          />
                        </div>
                      </div>
                    ))}
                  </div>
                )}
              </div>
            </motion.div>
          )}
        </AnimatePresence>
      </div>
    </div>
  )
}

function InfoBlock({ label, children, className }: { label: string; children: React.ReactNode; className?: string }) {
  return (
    <div className={`flex flex-col gap-1 ${className ?? ''}`}>
      <span className="text-[10px] uppercase tracking-wider text-muted-foreground font-medium">{label}</span>
      {children}
    </div>
  )
}
