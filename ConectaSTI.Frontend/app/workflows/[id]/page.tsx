'use client'

import { useState, useEffect, useCallback, useRef, use } from 'react'
import { useRouter } from 'next/navigation'
import type { Node, Edge } from '@xyflow/react'
import { createFluxo, executarFluxo, getFluxo, updateFluxo, type FluxoExecutionResult } from '@/lib/api/fluxos'
import { getNo, createNo, updateNo, deleteNo } from '@/lib/api/nos'
import { useIntegracoes } from '@/hooks/useIntegracoes'
import { useEndpoints } from '@/hooks/useEndpoints'
import { useFuncoes } from '@/hooks/useFuncoes'
import type {
  Fluxo, WorkflowNodeData, TipoErro, BackoffType, CanvasState, Operacao
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

function operacaoPayload(
  data: WorkflowNodeData,
  ordem: number,
  noId: number,
  fluxoId: number,
): Partial<Operacao> {
  const maximoRepeticao = data.maximoRepeticao ?? 0

  return {
    ...(data.operacaoId ? { id: data.operacaoId } : {}),
    ordem,
    noId,
    fluxoId,
    repetir: data.repetir ?? maximoRepeticao > 0,
    erro: data.erro,
    maximoRepeticao,
    backoffType: data.backoffType,
    backoffDelay: data.backoffDelay,
    backoffMultiplier: data.backoffMultiplier,
    timeout: data.timeout,
  }
}

export default function WorkflowBuilderPage({ params }: { params: Promise<{ id: string }> }) {
  const { id: idStr } = use(params)
  const isDraft = idStr === 'new'
  const id = isDraft ? 0 : Number(idStr)
  const router = useRouter()

  const { integracoes } = useIntegracoes()
  const { endpoints } = useEndpoints()
  const { funcoes } = useFuncoes()

  const [fluxo, setFluxo] = useState<Fluxo | null>(null)
  const [nome, setNome] = useState('')
  const [saving, setSaving] = useState(false)
  const [saved, setSaved] = useState(false)
  const [executing, setExecuting] = useState(false)
  const [executionResult, setExecutionResult] = useState<FluxoExecutionResult | null>(null)
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
      if (isDraft) {
        const draftName = sessionStorage.getItem('workflow-draft-name') ?? 'Novo workflow'
        const draftFluxo: Fluxo = { id: 0, nome: draftName, operacoes: [] }

        setFluxo(draftFluxo)
        setNome(draftName)
        setInitialNodes([])
        setInitialEdges([])
        nodesRef.current = []
        edgesRef.current = []
        initialNoIdsRef.current = new Set()
        return
      }

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
          repetir: op.repetir,
          maximoRepeticao: op.maximoRepeticao,
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
  }, [id, isDraft])

  const handleSave = useCallback(async () => {
    if (!fluxo) return
    setSaving(true)
    try {
      const currentNodes = nodesRef.current
      const currentEdges = edgesRef.current
      if (currentNodes.length === 0) {
        alert('Adicione ao menos um bloco antes de salvar o workflow no backend.')
        return
      }

      // Sorted left→right for ordem
      const sorted = [...currentNodes].sort((a, b) => a.position.x - b.position.x)

      // Build id-mapping for temp nodes
      const idMap = new Map<string, number>()
      const operacoes: Partial<Operacao>[] = []

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
          // New node — create No, then include its operation inside Fluxo.
          const no = await createNo(noPayload)
          idMap.set(node.id, no.id)
          operacoes.push(operacaoPayload(d, ordem, no.id, id))
          // Update node data with real ids
          nodesRef.current = nodesRef.current.map(n =>
            n.id === node.id
              ? { ...n, id: String(no.id), data: { ...n.data, noId: no.id } }
              : n
          )
        } else {
          // Existing node
          await updateNo(d.noId, noPayload)
          operacoes.push(operacaoPayload(d, ordem, d.noId, id))
          idMap.set(node.id, d.noId)
        }
      }

      const savedFluxo = isDraft
        ? await createFluxo({ nome, operacoes })
        : await updateFluxo(id, { nome, operacoes })
      const savedId = savedFluxo.id

      // Delete removed nodes
      const currentNoIds = new Set(currentNodes.map(n => n.data.noId as number).filter(n => n > 0))
      for (const noId of initialNoIdsRef.current) {
        if (!currentNoIds.has(noId)) {
          await deleteNo(noId).catch(() => {})
        }
      }

      // Save canvas state
      const positions: CanvasState['positions'] = {}
      for (const node of nodesRef.current) {
        const realId = idMap.get(node.id) ?? (node.data.noId as number)
        if (realId > 0) positions[String(realId)] = node.position
      }
      localStorage.setItem(CANVAS_KEY(savedId), JSON.stringify({ positions, edges: currentEdges }))

      // Update initial ids
      initialNoIdsRef.current = new Set(nodesRef.current.map(n => n.data.noId as number).filter(n => n > 0))
      setFluxo(savedFluxo)

      if (isDraft) {
        sessionStorage.removeItem('workflow-draft-name')
        router.replace(`/workflows/${savedId}`)
      }

      setSaved(true)
      setTimeout(() => setSaved(false), 2500)
    } catch (e) {
      alert(e instanceof Error ? e.message : 'Erro ao salvar')
    } finally {
      setSaving(false)
    }
  }, [fluxo, id, isDraft, nome, router])

  useEffect(() => {
    function onKey(e: KeyboardEvent) {
      if ((e.ctrlKey || e.metaKey) && e.key === 's') { e.preventDefault(); handleSave() }
    }
    window.addEventListener('keydown', onKey)
    return () => window.removeEventListener('keydown', onKey)
  }, [handleSave])

  async function handleExecute() {
    if (!fluxo || isDraft) {
      alert('Salve o workflow antes de executar.')
      return
    }

    setExecuting(true)
    setExecutionResult(null)
    try {
      setExecutionResult(await executarFluxo(fluxo.id))
    } catch (e) {
      alert(e instanceof Error ? e.message : 'Erro ao executar workflow')
    } finally {
      setExecuting(false)
    }
  }

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
            onClick={handleExecute}
            disabled={executing || isDraft}
          >
            {executing ? <CircleNotchIcon size={12} className="animate-spin" /> : <PlayIcon size={12} />}
            {executing ? 'Executando...' : 'Executar'}
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
        {executionResult && (
          <div className="absolute right-4 top-14 z-20 max-w-md border border-border bg-white/95 p-3 text-xs shadow-sm">
            <p className="font-medium">
              Execução {executionResult.sucesso === false ? 'finalizada com erro' : 'finalizada'}
            </p>
            {executionResult.retorno?.map((item, index) => (
              <p
                key={`${item.mensagem}-${index}`}
                className={item.erro ? 'mt-1 text-destructive' : 'mt-1 text-muted-foreground'}
              >
                {item.mensagem}
              </p>
            ))}
            {executionResult.respostaBody && (
              <pre className="mt-2 max-h-40 overflow-auto bg-muted p-2 font-mono text-[11px]">
                {executionResult.respostaBody}
              </pre>
            )}
          </div>
        )}

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
  const [maximoRepeticao, setMaximoRepeticao] = useState(d.maximoRepeticao ?? 0)
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
        repetir: maximoRepeticao > 0,
        maximoRepeticao,
        timeout,
        backoffType,
        backoffDelay,
        backoffMultiplier,
        chaveValor: (d.tipo === 3 || d.tipo === 4) ? chaveValor : d.chaveValor,
      },
    }
    onUpdate(updated)
  }

  const tipoLabel = {
    1: 'Requisição',
    2: 'Função JS',
    3: 'Salvar Storage',
    4: 'Pegar Storage',
    5: 'Fluxo',
  }[d.tipo] ?? '—'

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
              <label className="text-[10px] text-muted-foreground">Máx. repetições</label>
              <Input
                type="number"
                value={maximoRepeticao}
                onChange={e => setMaximoRepeticao(Number(e.target.value))}
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
