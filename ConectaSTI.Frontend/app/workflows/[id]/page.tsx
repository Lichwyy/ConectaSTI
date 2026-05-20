'use client'

import { useState, useEffect, useCallback, useRef, use } from 'react'
import { useRouter } from 'next/navigation'
import type { Node, Edge } from '@xyflow/react'
import { getFluxo, updateFluxo } from '@/lib/api/fluxos'
import { getNo, createNo, updateNo, deleteNo } from '@/lib/api/nos'
import { createOperacao, updateOperacao, deleteOperacao } from '@/lib/api/operacoes'
import { useIntegracoes } from '@/hooks/useIntegracoes'
import { useEndpoints } from '@/hooks/useEndpoints'
import { useFuncoes } from '@/hooks/useFuncoes'
import type {
  Fluxo, WorkflowNodeData, TipoErro, BackoffType, CanvasState
} from '@/lib/types'
import { WorkflowCanvas } from '@/components/workflow/WorkflowCanvas'
import { NodePalette } from '@/components/workflow/NodePalette'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import {
  ArrowLeftIcon,
  FloppyDiskIcon,
  CheckCircleIcon,
  CircleNotchIcon,
  XIcon,
  PlayIcon,
} from '@phosphor-icons/react'
import { motion, AnimatePresence } from 'motion/react'

const CANVAS_KEY = (id: number) => `canvas-state-${id}`

export default function WorkflowBuilderPage({ params }: { params: Promise<{ id: string }> }) {
  const { id: idStr } = use(params)
  const id = Number(idStr)
  const router = useRouter()

  const { integracoes } = useIntegracoes()
  const { endpoints } = useEndpoints()
  const { funcoes } = useFuncoes()

  const [fluxo, setFluxo] = useState<Fluxo | null>(null)
  const [nome, setNome] = useState('')
  const [saving, setSaving] = useState(false)
  const [saved, setSaved] = useState(false)
  const [selectedNode, setSelectedNode] = useState<Node<WorkflowNodeData> | null>(null)

  const [initialNodes, setInitialNodes] = useState<Node<WorkflowNodeData>[]>([])
  const [initialEdges, setInitialEdges] = useState<Edge[]>([])

  const nodesRef = useRef<Node<WorkflowNodeData>[]>([])
  const edgesRef = useRef<Edge[]>([])
  const initialNoIdsRef = useRef<Set<number>>(new Set())
  const tempIdCounter = useRef(-1)

  const getTempId = useCallback(() => tempIdCounter.current--, [])

  useEffect(() => {
    async function load() {
      const f = await getFluxo(id).catch(() => null)
      if (!f) { router.push('/workflows'); return }

      setFluxo(f)
      setNome(f.nome)

      const operacoes = f.operacoes ?? []
      const nos = await Promise.all(operacoes.map(op => getNo(op.noId).catch(() => null)))

      const canvasRaw = typeof window !== 'undefined' ? localStorage.getItem(CANVAS_KEY(id)) : null
      const canvas: CanvasState = canvasRaw ? JSON.parse(canvasRaw) : { positions: {}, edges: [] }

      const builtNodes: Node<WorkflowNodeData>[] = []
      const initialNoIds = new Set<number>()

      for (let i = 0; i < operacoes.length; i++) {
        const op = operacoes[i]
        const no = nos[i]
        if (!no) continue

        initialNoIds.add(no.id)

        const pos = canvas.positions[String(no.id)] ?? { x: 100 + i * 260, y: 200 }

        let nodeType = 'reqNode'
        const data: WorkflowNodeData = {
          noId: no.id,
          operacaoId: op.id,
          tipo: no.tipo,
          label: '',
          body: no.body,
          headers: no.headers,
          chaveValor: no.chaveValor ?? undefined,
          funcaoId: no.funcaoId ?? undefined,
          endpointId: no.endPointId ?? undefined,
          ordem: op.ordem,
          erro: op.erro,
          maxRetries: op.maxRetries,
          backoffType: op.backoffType,
          backoffDelay: op.backoffDelay,
          backoffMultiplier: op.backoffMultiplier,
          timeout: op.timeout,
        }

        if (no.tipo === 1) {
          nodeType = 'reqNode'
          const ep = endpoints.find(e => e.id === no.endPointId)
          const integ = ep ? integracoes.find(a => a.id === ep.integracaoId) : null
          data.integracaoId = ep?.integracaoId
          data.verbo = ep?.verbo
          data.integracaoNome = integ?.nome
          data.recurso = ep?.recurso
          data.label = ep?.descricao ?? ep?.recurso ?? `No ${no.id}`
        } else if (no.tipo === 2) {
          nodeType = 'funcaoNode'
          const fn = funcoes.find(f => f.id === no.funcaoId)
          data.funcaoNome = fn?.nome
          data.label = fn?.nome ?? `Função ${no.id}`
        } else if (no.tipo === 3) {
          nodeType = 'storageNode'
          data.label = 'Salvar Storage'
        } else if (no.tipo === 4) {
          nodeType = 'storageNode'
          data.label = 'Pegar Storage'
        }

        builtNodes.push({ id: String(no.id), type: nodeType, position: pos, data })
      }

      initialNoIdsRef.current = initialNoIds
      setInitialNodes(builtNodes)
      setInitialEdges(canvas.edges)
      nodesRef.current = builtNodes
      edgesRef.current = canvas.edges
    }

    load()
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [id])

  const handleSave = useCallback(async () => {
    if (!fluxo) return
    setSaving(true)
    try {
      await updateFluxo(id, { nome })

      const currentNodes = nodesRef.current
      const currentEdges = edgesRef.current

      // Sorted left→right for ordem
      const sorted = [...currentNodes].sort((a, b) => a.position.x - b.position.x)

      // Build id-mapping for temp nodes
      const idMap = new Map<string, number>()

      for (let i = 0; i < sorted.length; i++) {
        const node = sorted[i]
        const d = node.data
        const ordem = i + 1

        const noPayload = {
          tipo: d.tipo,
          body: d.body ?? null,
          headers: d.headers ?? null,
          funcaoId: d.funcaoId ?? null,
          endPointId: d.endpointId ?? null,
          chaveValor: d.chaveValor ?? null,
        }

        if (d.noId < 0) {
          // New node — create No, then Operacao
          const no = await createNo(noPayload)
          const op = await createOperacao({
            ordem,
            noId: no.id,
            fluxoId: id,
            erro: d.erro,
            backoffType: d.backoffType,
            maxRetries: d.maxRetries,
            backoffDelay: d.backoffDelay,
            backoffMultiplier: d.backoffMultiplier,
            timeout: d.timeout,
          })
          idMap.set(node.id, no.id)
          // Update node data with real ids
          nodesRef.current = nodesRef.current.map(n =>
            n.id === node.id
              ? { ...n, id: String(no.id), data: { ...n.data, noId: no.id, operacaoId: op.id } }
              : n
          )
        } else {
          // Existing node
          await updateNo(d.noId, noPayload)
          if (d.operacaoId) {
            await updateOperacao(d.operacaoId, {
              ordem,
              noId: d.noId,
              fluxoId: id,
              erro: d.erro,
              backoffType: d.backoffType,
              maxRetries: d.maxRetries,
              backoffDelay: d.backoffDelay,
              backoffMultiplier: d.backoffMultiplier,
              timeout: d.timeout,
            })
          }
          idMap.set(node.id, d.noId)
        }
      }

      // Delete removed nodes
      const currentNoIds = new Set(currentNodes.map(n => n.data.noId as number).filter(n => n > 0))
      for (const noId of initialNoIdsRef.current) {
        if (!currentNoIds.has(noId)) {
          const node = initialNodes.find(n => n.data.noId === noId)
          if (node?.data.operacaoId) await deleteOperacao(node.data.operacaoId).catch(() => {})
          await deleteNo(noId).catch(() => {})
        }
      }

      // Save canvas state
      const positions: CanvasState['positions'] = {}
      for (const node of nodesRef.current) {
        const realId = idMap.get(node.id) ?? (node.data.noId as number)
        if (realId > 0) positions[String(realId)] = node.position
      }
      localStorage.setItem(CANVAS_KEY(id), JSON.stringify({ positions, edges: currentEdges }))

      // Update initial ids
      initialNoIdsRef.current = new Set(nodesRef.current.map(n => n.data.noId as number).filter(n => n > 0))

      setSaved(true)
      setTimeout(() => setSaved(false), 2500)
    } catch (e) {
      alert(e instanceof Error ? e.message : 'Erro ao salvar')
    } finally {
      setSaving(false)
    }
  }, [fluxo, id, nome, initialNodes])

  useEffect(() => {
    function onKey(e: KeyboardEvent) {
      if ((e.ctrlKey || e.metaKey) && e.key === 's') { e.preventDefault(); handleSave() }
    }
    window.addEventListener('keydown', onKey)
    return () => window.removeEventListener('keydown', onKey)
  }, [handleSave])

  const handleNodesChange = useCallback((nodes: Node<WorkflowNodeData>[]) => {
    nodesRef.current = nodes
  }, [])

  const handleEdgesChange = useCallback((edges: Edge[]) => {
    edgesRef.current = edges
  }, [])

  if (!fluxo) {
    return (
      <div className="flex h-screen items-center justify-center">
        <CircleNotchIcon size={24} className="animate-spin text-muted-foreground" />
      </div>
    )
  }

  return (
    <div className="relative flex flex-col h-screen w-full overflow-hidden bg-[radial-gradient(circle,#73737330_1px,transparent_1px)] bg-[size:20px_20px]">
      {/* Toolbar */}
      <div className="flex items-center gap-3 px-4 py-2.5 border-b border-border bg-white/80 backdrop-blur-sm shrink-0 z-10">
        <button
          onClick={() => router.push('/workflows')}
          className="text-muted-foreground hover:text-foreground transition-colors"
          title="Voltar"
        >
          <ArrowLeftIcon size={16} />
        </button>

        <div className="h-5 w-px bg-border" />

        <Input
          value={nome}
          onChange={e => setNome(e.target.value)}
          className="h-7 text-sm font-medium border-0 border-b border-transparent hover:border-border focus-visible:border-border focus-visible:ring-0 rounded-none bg-transparent px-1 w-52 transition-colors"
          placeholder="Nome do workflow"
        />

        <div className="ml-auto flex items-center gap-2">
          <Button
            size="sm"
            variant="outline"
            className="gap-1.5 rounded-none h-7 text-xs"
            onClick={() => alert('Execução será implementada com o backend .NET.')}
          >
            <PlayIcon size={12} />
            Executar
          </Button>

          <Button
            size="sm"
            className="gap-1.5 rounded-none h-7 text-xs"
            onClick={handleSave}
            disabled={saving}
          >
            {saving ? (
              <CircleNotchIcon size={12} className="animate-spin" />
            ) : saved ? (
              <CheckCircleIcon size={12} />
            ) : (
              <FloppyDiskIcon size={12} />
            )}
            {saving ? 'Salvando...' : saved ? 'Salvo!' : 'Salvar'}
          </Button>
        </div>

        <div className="text-[10px] text-muted-foreground hidden sm:block">Ctrl+S</div>
      </div>

      {/* Canvas area */}
      <div className="flex flex-1 overflow-hidden">
        <NodePalette
          integracoes={integracoes}
          endpoints={endpoints}
          funcoes={funcoes}
        />

        <div className="flex-1 relative">
          <WorkflowCanvas
            initialNodes={initialNodes}
            initialEdges={initialEdges}
            onNodesChange={handleNodesChange}
            onEdgesChange={handleEdgesChange}
            onNodeSelect={setSelectedNode}
            getTempId={getTempId}
          />
        </div>

        {/* Node detail panel */}
        <AnimatePresence>
          {selectedNode && (
            <motion.div
              initial={{ width: 0, opacity: 0 }}
              animate={{ width: 300, opacity: 1 }}
              exit={{ width: 0, opacity: 0 }}
              transition={{ duration: 0.2 }}
              className="border-l border-border bg-white/80 backdrop-blur-sm overflow-hidden shrink-0"
            >
              <div className="w-[300px] h-full overflow-y-auto">
                <NodeDetailPanel
                  node={selectedNode}
                  onClose={() => setSelectedNode(null)}
                  onUpdate={(updated) => {
                    nodesRef.current = nodesRef.current.map(n =>
                      n.id === updated.id ? updated : n
                    )
                    setSelectedNode(updated)
                  }}
                />
              </div>
            </motion.div>
          )}
        </AnimatePresence>
      </div>
    </div>
  )
}

function NodeDetailPanel({
  node,
  onClose,
  onUpdate,
}: {
  node: Node<WorkflowNodeData>
  onClose: () => void
  onUpdate: (node: Node<WorkflowNodeData>) => void
}) {
  const d = node.data
  const [erro, setErro] = useState<TipoErro>(d.erro)
  const [maxRetries, setMaxRetries] = useState(d.maxRetries ?? 0)
  const [timeout, setTimeout_] = useState(d.timeout ?? 30000)
  const [backoffType, setBackoffType] = useState<BackoffType>(d.backoffType)
  const [backoffDelay, setBackoffDelay] = useState(d.backoffDelay ?? 0)
  const [backoffMultiplier, setBackoffMultiplier] = useState(d.backoffMultiplier ?? 1)
  const [chaveValor, setChaveValor] = useState(d.chaveValor ?? '')

  function apply() {
    const updated: Node<WorkflowNodeData> = {
      ...node,
      data: {
        ...d,
        erro,
        maxRetries,
        timeout,
        backoffType,
        backoffDelay,
        backoffMultiplier,
        chaveValor: (d.tipo === 3 || d.tipo === 4) ? chaveValor : d.chaveValor,
      },
    }
    onUpdate(updated)
  }

  const tipoLabel = { 1: 'Requisição', 2: 'Função JS', 3: 'Salvar Storage', 4: 'Pegar Storage' }[d.tipo] ?? '—'

  return (
    <>
      <div className="p-4 border-b border-border flex items-center justify-between">
        <p className="text-xs font-semibold">Detalhes do bloco</p>
        <button onClick={onClose} className="text-muted-foreground hover:text-foreground transition-colors">
          <XIcon size={14} />
        </button>
      </div>

      <div className="p-4 space-y-4">
        <div>
          <p className="text-[10px] uppercase tracking-wider text-muted-foreground mb-1">Tipo</p>
          <p className="text-xs font-medium">{tipoLabel}</p>
        </div>

        {d.tipo === 1 && (
          <>
            <div>
              <p className="text-[10px] uppercase tracking-wider text-muted-foreground mb-1">Endpoint</p>
              <p className="font-mono text-xs">{d.recurso ?? '—'}</p>
            </div>
            <div>
              <p className="text-[10px] uppercase tracking-wider text-muted-foreground mb-1">API</p>
              <p className="text-xs">{d.integracaoNome ?? '—'}</p>
            </div>
          </>
        )}

        {d.tipo === 2 && (
          <div>
            <p className="text-[10px] uppercase tracking-wider text-muted-foreground mb-1">Função</p>
            <p className="text-xs">{d.funcaoNome ?? '—'}</p>
          </div>
        )}

        {(d.tipo === 3 || d.tipo === 4) && (
          <div className="flex flex-col gap-1">
            <label className="text-[10px] uppercase tracking-wider text-muted-foreground">Chave</label>
            <Input
              value={chaveValor}
              onChange={e => setChaveValor(e.target.value)}
              placeholder="nome-da-chave"
              className="h-7 text-xs font-mono rounded-none"
            />
          </div>
        )}

        <div className="border-t pt-4 space-y-3">
          <p className="text-[10px] uppercase tracking-wider text-muted-foreground font-medium">Operação</p>

          <div className="flex flex-col gap-1">
            <label className="text-[10px] text-muted-foreground">Em caso de erro</label>
            <Select value={String(erro)} onValueChange={v => setErro(Number(v) as TipoErro)}>
              <SelectTrigger className="h-7 text-xs rounded-none">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="1" className="text-xs">Parar</SelectItem>
                <SelectItem value="2" className="text-xs">Continuar</SelectItem>
                <SelectItem value="3" className="text-xs">Repetir</SelectItem>
              </SelectContent>
            </Select>
          </div>

          <div className="grid grid-cols-2 gap-2">
            <div className="flex flex-col gap-1">
              <label className="text-[10px] text-muted-foreground">Timeout (ms)</label>
              <Input
                type="number"
                value={timeout}
                onChange={e => setTimeout_(Number(e.target.value))}
                className="h-7 text-xs rounded-none"
              />
            </div>
            <div className="flex flex-col gap-1">
              <label className="text-[10px] text-muted-foreground">Máx. tentativas</label>
              <Input
                type="number"
                value={maxRetries}
                onChange={e => setMaxRetries(Number(e.target.value))}
                className="h-7 text-xs rounded-none"
              />
            </div>
          </div>

          {erro === 3 && (
            <>
              <div className="flex flex-col gap-1">
                <label className="text-[10px] text-muted-foreground">Tipo de backoff</label>
                <Select value={String(backoffType)} onValueChange={v => setBackoffType(Number(v) as BackoffType)}>
                  <SelectTrigger className="h-7 text-xs rounded-none">
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="1" className="text-xs">Constante</SelectItem>
                    <SelectItem value="2" className="text-xs">Linear</SelectItem>
                    <SelectItem value="3" className="text-xs">Exponencial</SelectItem>
                  </SelectContent>
                </Select>
              </div>
              <div className="grid grid-cols-2 gap-2">
                <div className="flex flex-col gap-1">
                  <label className="text-[10px] text-muted-foreground">Delay (ms)</label>
                  <Input
                    type="number"
                    value={backoffDelay}
                    onChange={e => setBackoffDelay(Number(e.target.value))}
                    className="h-7 text-xs rounded-none"
                  />
                </div>
                <div className="flex flex-col gap-1">
                  <label className="text-[10px] text-muted-foreground">Multiplicador</label>
                  <Input
                    type="number"
                    value={backoffMultiplier}
                    onChange={e => setBackoffMultiplier(Number(e.target.value))}
                    step="0.1"
                    className="h-7 text-xs rounded-none"
                  />
                </div>
              </div>
            </>
          )}
        </div>

        {d.noId > 0 && (
          <div className="border-t pt-3">
            <p className="text-[10px] uppercase tracking-wider text-muted-foreground mb-1">ID do nó</p>
            <p className="font-mono text-[10px] text-muted-foreground">{d.noId}</p>
          </div>
        )}

        <Button size="sm" className="w-full rounded-none h-7 text-xs" onClick={apply}>
          Aplicar
        </Button>
      </div>
    </>
  )
}
