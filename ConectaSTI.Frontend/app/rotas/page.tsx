'use client'

import { useMemo, useState } from 'react'
import { AnimatePresence, motion } from 'motion/react'
import {
  ArrowClockwiseIcon,
  ArrowSquareOutIcon,
  CircleNotchIcon,
  KeyIcon,
  MagnifyingGlassIcon,
  PathIcon,
  PencilSimpleIcon,
  PlusIcon,
  ShieldCheckIcon,
  TimerIcon,
  TrashIcon,
} from '@phosphor-icons/react'
import { useRotas } from '@/hooks/useRotas'
import { useFluxosVersionados } from '@/hooks/useFluxosVersionados'
import type { Rota, VerboHttp } from '@/lib/types'
import { verboToHttpMethod } from '@/lib/types'
import type { RotaPayload } from '@/lib/api/rotas'
import { MethodBadge } from '@/components/MethodBadge'
import { RotaRegistrationForm } from '@/components/RotaRegistrationForm'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Badge } from '@/components/ui/badge'

function formatDate(value?: string | null) {
  if (!value) return '--'
  return new Date(value).toLocaleString('pt-BR')
}

function buildRouteUrl(caminho: string) {
  const normalizedPath = caminho.startsWith('/') ? caminho : `/${caminho}`

  if (typeof window === 'undefined') return normalizedPath

  const { protocol, hostname } = window.location
  if (hostname.startsWith('conectasti.')) {
    return `${protocol}//conectasti-rotas.${hostname.split('.').slice(1).join('.')}${normalizedPath}`
  }

  return `http://localhost:15184${normalizedPath}`
}

function prettyJson(value?: string | null) {
  if (!value) return null

  try {
    return JSON.stringify(JSON.parse(value), null, 2)
  } catch {
    return value
  }
}

const metodoOptions: Array<{ value: 'all' | VerboHttp; label: string }> = [
  { value: 'all', label: 'Todos' },
  { value: 1, label: 'GET' },
  { value: 2, label: 'POST' },
  { value: 3, label: 'PUT' },
  { value: 4, label: 'DELETE' },
  { value: 5, label: 'PATCH' },
]

type PanelMode = 'empty' | 'new' | 'edit' | 'detail'

export default function RotasPage() {
  const { rotas, loading, error, createRota, updateRota, deleteRota, reload } = useRotas()
  const { fluxosVersionados, loading: loadingVersions } = useFluxosVersionados()
  const [selectedId, setSelectedId] = useState<number | null>(null)
  const [query, setQuery] = useState('')
  const [metodo, setMetodo] = useState<'all' | VerboHttp>('all')
  const [panelMode, setPanelMode] = useState<PanelMode>('empty')
  const [formError, setFormError] = useState<string | null>(null)

  const filtered = useMemo(() => {
    const text = query.trim().toLowerCase()

    return rotas.filter(rota => {
      const matchMethod = metodo === 'all' || rota.metodo === metodo
      const matchText = !text
        || rota.nome.toLowerCase().includes(text)
        || rota.caminho.toLowerCase().includes(text)
        || (rota.descricao ?? '').toLowerCase().includes(text)
        || (rota.pipelineVersao ?? '').toLowerCase().includes(text)

      return matchMethod && matchText
    })
  }, [rotas, query, metodo])

  const selected = rotas.find(rota => rota.id === selectedId) ?? filtered[0] ?? null
  const selectedUrl = selected ? buildRouteUrl(selected.caminho) : ''

  async function handleSave(data: RotaPayload) {
    try {
      setFormError(null)

      const saved = panelMode === 'edit' && selectedId
        ? await updateRota(selectedId, data)
        : await createRota(data)

      setSelectedId(saved.id)
      setPanelMode('detail')
    } catch (e) {
      setFormError(e instanceof Error ? e.message : 'Erro ao salvar rota')
    }
  }

  async function handleDelete(rota: Rota) {
    if (!confirm(`Remover a rota "${rota.nome}"?`)) return

    await deleteRota(rota.id)
    setSelectedId(null)
    setPanelMode('empty')
  }

  function showDetail(rota: Rota) {
    setSelectedId(rota.id)
    setPanelMode('detail')
    setFormError(null)
  }

  function showNewForm() {
    setSelectedId(null)
    setPanelMode('new')
    setFormError(null)
  }

  return (
    <div className="flex h-screen w-full overflow-hidden">
      <aside className="flex w-[380px] shrink-0 flex-col border-r border-border bg-white/70">
        <div className="border-b border-border p-5">
          <div className="flex items-center justify-between gap-3">
            <div>
              <h1 className="text-xl font-semibold">Rotas</h1>
              <p className="mt-1 text-xs text-muted-foreground">Endpoints públicos ligados aos workflows versionados.</p>
            </div>
            <div className="flex gap-2">
              <Button
                size="sm"
                className="h-8 gap-1.5 rounded-none text-xs"
                onClick={showNewForm}
              >
                <PlusIcon size={13} />
                Nova
              </Button>
              <Button
                size="sm"
                variant="outline"
                className="h-8 rounded-none"
                onClick={reload}
                disabled={loading}
                title="Atualizar rotas"
              >
                {loading ? <CircleNotchIcon size={14} className="animate-spin" /> : <ArrowClockwiseIcon size={14} />}
              </Button>
            </div>
          </div>

          <div className="relative mt-4">
            <MagnifyingGlassIcon size={13} className="absolute left-2.5 top-2.5 text-muted-foreground" />
            <Input
              value={query}
              onChange={event => setQuery(event.target.value)}
              placeholder="Buscar por nome, caminho ou workflow"
              className="h-8 rounded-none pl-8 text-xs"
            />
          </div>

          <div className="mt-3 flex flex-wrap gap-1.5">
            {metodoOptions.map(option => {
              const active = metodo === option.value
              return (
                <button
                  key={option.label}
                  type="button"
                  onClick={() => setMetodo(option.value)}
                  className={`border px-2.5 py-1 text-[11px] transition-colors ${
                    active
                      ? 'border-primary bg-primary text-primary-foreground'
                      : 'border-border text-muted-foreground hover:bg-muted'
                  }`}
                >
                  {option.label}
                </button>
              )
            })}
          </div>
        </div>

        <div className="flex-1 overflow-y-auto">
          {loading ? (
            <div className="space-y-3 p-5">
              {[1, 2, 3].map(item => (
                <div key={item} className="h-28 animate-pulse bg-muted/50" />
              ))}
            </div>
          ) : error ? (
            <div className="p-5 text-xs text-destructive">{error}</div>
          ) : filtered.length === 0 ? (
            <div className="p-8 text-center text-xs text-muted-foreground">Nenhuma rota encontrada.</div>
          ) : (
            <motion.div
              className="p-2"
              initial="hidden"
              animate="visible"
              variants={{ visible: { transition: { staggerChildren: 0.04 } } }}
            >
              {filtered.map(rota => {
                const selectedRoute = selected?.id === rota.id
                const method = verboToHttpMethod(rota.metodo)

                return (
                  <motion.button
                    key={rota.id}
                    variants={{ hidden: { opacity: 0, y: 6 }, visible: { opacity: 1, y: 0 } }}
                    onClick={() => showDetail(rota)}
                    className={`mb-1.5 w-full border p-4 text-left transition-colors ${
                      selectedRoute ? 'border-primary bg-primary text-primary-foreground' : 'border-border bg-white hover:bg-muted/60'
                    }`}
                  >
                    <div className="flex items-start justify-between gap-3">
                      <div className="min-w-0">
                        <div className="flex items-center gap-2">
                          <MethodBadge method={method} className={selectedRoute ? 'border-white/40 bg-white/15 text-white' : ''} />
                          {rota.senhaAcesso && <KeyIcon size={13} />}
                          {rota.rateLimit && <TimerIcon size={13} />}
                        </div>
                        <p className="mt-2 truncate text-sm font-medium">{rota.nome}</p>
                        <p className={`mt-1 truncate font-mono text-[11px] ${selectedRoute ? 'text-primary-foreground/75' : 'text-muted-foreground'}`}>
                          /{rota.caminho.replace(/^\//, '')}
                        </p>
                      </div>
                      <PathIcon size={16} className={selectedRoute ? 'text-primary-foreground/80' : 'text-muted-foreground'} />
                    </div>
                  </motion.button>
                )
              })}
            </motion.div>
          )}
        </div>
      </aside>

      <main className="flex-1 overflow-y-auto">
        <AnimatePresence mode="wait">
          {(panelMode === 'new' || panelMode === 'edit') && (
            <motion.div key="form" initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }} className="min-h-full">
              {formError && (
                <div className="mx-6 mt-6 border border-destructive/30 bg-destructive/10 p-3 text-xs text-destructive">
                  {formError}
                </div>
              )}
              <RotaRegistrationForm
                initial={panelMode === 'edit' ? selected ?? undefined : undefined}
                fluxosVersionados={fluxosVersionados}
                loadingVersions={loadingVersions}
                onSave={handleSave}
                onCancel={() => {
                  setFormError(null)
                  setPanelMode(selected ? 'detail' : 'empty')
                }}
              />
            </motion.div>
          )}

          {panelMode !== 'new' && panelMode !== 'edit' && selected ? (
          <motion.div key={selected.id} initial={{ opacity: 0, y: 8 }} animate={{ opacity: 1, y: 0 }} exit={{ opacity: 0 }} className="mx-auto max-w-5xl p-8">
            <div className="flex flex-wrap items-start justify-between gap-4 border-b border-border pb-5">
              <div className="min-w-0">
                <div className="flex flex-wrap items-center gap-2">
                  <MethodBadge method={verboToHttpMethod(selected.metodo)} />
                  <h2 className="text-xl font-semibold">{selected.nome}</h2>
                </div>
                <p className="mt-2 break-all font-mono text-sm text-muted-foreground">/{selected.caminho.replace(/^\//, '')}</p>
              </div>

              <div className="flex flex-wrap gap-2">
                <Button
                  size="sm"
                  variant="outline"
                  className="h-8 gap-1.5 rounded-none text-xs"
                  onClick={() => setPanelMode('edit')}
                >
                  <PencilSimpleIcon size={13} />
                  Editar
                </Button>
                <Button
                  size="sm"
                  variant="outline"
                  className="h-8 gap-1.5 rounded-none text-xs text-destructive hover:text-destructive"
                  onClick={() => handleDelete(selected)}
                >
                  <TrashIcon size={13} />
                </Button>
                <Button
                  size="sm"
                  variant="outline"
                  className="h-8 gap-1.5 rounded-none text-xs"
                  onClick={() => window.open(selectedUrl, '_blank', 'noopener,noreferrer')}
                >
                  Abrir rota
                  <ArrowSquareOutIcon size={13} />
                </Button>
              </div>
            </div>

            <div className="mt-6 grid grid-cols-1 gap-4 lg:grid-cols-4">
              <Metric label="Pipeline" value={selected.pipelineVersao ?? `#${selected.pipelineVersaoId}`} />
              <Metric label="Atualização" value={selected.usarFluxoMaisAtual ? 'Fluxo mais atual' : 'Versão fixa'} />
              <Metric label="Rate limit" value={selected.rateLimit ? `${selected.rateLimitRequests ?? 0}/${selected.rateLimitInterval ?? 60}s` : 'Desativado'} />
              <Metric label="Criada em" value={formatDate(selected.criadoEm)} />
            </div>

            <div className="mt-6 space-y-5">
              <Info label="URL de execução">
                <div className="flex items-center gap-2">
                  <code className="break-all border border-border bg-muted/40 px-2 py-1 text-xs">{selectedUrl}</code>
                </div>
              </Info>

              <Info label="Descrição">
                <p className="text-sm leading-relaxed text-muted-foreground">{selected.descricao || '--'}</p>
              </Info>

              <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
                <Flag enabled={Boolean(selected.senhaAcesso)} icon={<KeyIcon size={14} />} title="Protegida por senha" text={selected.senhaAcesso ? 'Exige header X-Rota-Senha' : 'Sem senha de rota'} />
                <Flag enabled={Boolean(selected.idempotencia)} icon={<ShieldCheckIcon size={14} />} title="Idempotência" text={selected.idempotencia ? 'Ativada para esta rota' : 'Desativada'} />
              </div>

              {selected.jsonSchemaReq && (
                <CodeBlock label="Schema da requisição" value={prettyJson(selected.jsonSchemaReq) ?? selected.jsonSchemaReq} />
              )}
              {selected.jsonSchemaResp && (
                <CodeBlock label="Schema da resposta" value={prettyJson(selected.jsonSchemaResp) ?? selected.jsonSchemaResp} />
              )}
            </div>
          </motion.div>
        ) : (
          panelMode !== 'new' && panelMode !== 'edit' && (
          <motion.div key="empty" initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }} className="flex h-full flex-col items-center justify-center gap-3 p-8 text-center text-muted-foreground">
            <PathIcon size={56} weight="thin" className="text-muted-foreground/40" />
            <div>
              <p className="text-sm font-medium">Nenhuma rota selecionada</p>
              <p className="mt-1 text-xs">Selecione uma rota para visualizar os detalhes.</p>
            </div>
            <Button size="sm" className="mt-2 gap-1.5 rounded-none" onClick={showNewForm}>
              <PlusIcon size={13} />
              Cadastrar rota
            </Button>
          </motion.div>
          )
        )}
        </AnimatePresence>
      </main>
    </div>
  )
}

function Metric({ label, value }: { label: string; value: string }) {
  return (
    <div className="border border-border bg-white p-3">
      <p className="text-[10px] uppercase tracking-wider text-muted-foreground">{label}</p>
      <p className="mt-1 text-xs font-medium">{value}</p>
    </div>
  )
}

function Info({ label, children }: { label: string; children: React.ReactNode }) {
  return (
    <div>
      <p className="text-[10px] uppercase tracking-wider text-muted-foreground">{label}</p>
      <div className="mt-1">{children}</div>
    </div>
  )
}

function Flag({ enabled, icon, title, text }: { enabled: boolean; icon: React.ReactNode; title: string; text: string }) {
  return (
    <div className={`border p-4 ${enabled ? 'border-primary/30 bg-primary/5' : 'border-border bg-white'}`}>
      <div className="flex items-center gap-2">
        {icon}
        <p className="text-sm font-medium">{title}</p>
        <Badge variant={enabled ? 'default' : 'secondary'} className="ml-auto rounded-none">
          {enabled ? 'Sim' : 'Não'}
        </Badge>
      </div>
      <p className="mt-2 text-xs text-muted-foreground">{text}</p>
    </div>
  )
}

function CodeBlock({ label, value }: { label: string; value: string }) {
  return (
    <div>
      <p className="text-[10px] uppercase tracking-wider text-muted-foreground">{label}</p>
      <pre className="mt-1 max-h-80 overflow-auto border border-border bg-muted/40 p-3 text-[11px] whitespace-pre-wrap break-words">
        {value}
      </pre>
    </div>
  )
}
