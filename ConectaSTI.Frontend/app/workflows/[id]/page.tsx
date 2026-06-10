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
  Fluxo, WorkflowNodeData, TipoErro, BackoffType, CanvasState, Operacao, No, EntradaFluxo
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
  TrashIcon,
  BracketsCurlyIcon,
} from '@phosphor-icons/react'
import { motion, AnimatePresence } from 'motion/react'

const CANVAS_KEY = (id: number) => `canvas-state-${id}`
const ENTRADA_KEY = (id: number) => `workflow-entry-${id}`

function parseJsonRecord(text: string, label: string): Record<string, string> {
  const trimmed = text.trim()
  if (!trimmed) return {}

  const parsed = JSON.parse(trimmed) as unknown
  if (!parsed || typeof parsed !== 'object' || Array.isArray(parsed)) {
    throw new Error(`${label} deve ser um objeto JSON.`)
  }

  return Object.fromEntries(
    Object.entries(parsed as Record<string, unknown>).map(([key, value]) => [
      key,
      value == null ? '' : String(value),
    ])
  )
}

function parseEntradaBody(text: string): unknown {
  const trimmed = text.trim()
  if (!trimmed) return undefined

  try {
    return JSON.parse(trimmed)
  } catch {
    return text
  }
}

function buildEntradaFluxo(
  routeParamsText: string,
  queryParamsText: string,
  bodyText: string,
): EntradaFluxo | undefined {
  const routeParams = parseJsonRecord(routeParamsText, 'Route params')
  const queryParams = parseJsonRecord(queryParamsText, 'Query params')
  const body = parseEntradaBody(bodyText)

  const entrada: EntradaFluxo = {}
  if (Object.keys(routeParams).length > 0) entrada.routeParams = routeParams
  if (Object.keys(queryParams).length > 0) entrada.queryParams = queryParams
  if (body !== undefined) entrada.body = body

  return Object.keys(entrada).length > 0 ? entrada : undefined
}

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
    usarDadosAnterior: data.usarDadosAnterior ?? false,
    erro: data.erro,
    maximoRepeticao,
    backoffType: data.backoffType,
    backoffDelay: data.backoffDelay,
    backoffMultiplier: data.backoffMultiplier,
    timeout: data.timeout,
  }
}

function orderNodesForExecution(
  nodes: Node<WorkflowNodeData>[],
  edges: Edge[],
): Node<WorkflowNodeData>[] {
  if (edges.length === 0) {
    return [...nodes].sort((a, b) => a.position.x - b.position.x)
  }

  const nodeById = new Map(nodes.map(node => [node.id, node]))
  const outgoing = new Map<string, string>()
  const incomingCount = new Map<string, number>()
  const connectedIds = new Set<string>()

  for (const edge of edges) {
    if (!nodeById.has(edge.source) || !nodeById.has(edge.target)) continue

    outgoing.set(edge.source, edge.target)
    incomingCount.set(edge.target, (incomingCount.get(edge.target) ?? 0) + 1)
    connectedIds.add(edge.source)
    connectedIds.add(edge.target)
  }

  const starts = [...connectedIds]
    .filter(id => (incomingCount.get(id) ?? 0) === 0)
    .map(id => nodeById.get(id))
    .filter((node): node is Node<WorkflowNodeData> => Boolean(node))
    .sort((a, b) => a.position.x - b.position.x)

  const ordered: Node<WorkflowNodeData>[] = []
  const visited = new Set<string>()

  for (const start of starts) {
    let current: Node<WorkflowNodeData> | undefined = start

    while (current && !visited.has(current.id)) {
      ordered.push(current)
      visited.add(current.id)
      current = nodeById.get(outgoing.get(current.id) ?? '')
    }
  }

  const disconnected = nodes
    .filter(node => !visited.has(node.id))
    .sort((a, b) => a.position.x - b.position.x)

  return [...ordered, ...disconnected]
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
  const [showExecutionInput, setShowExecutionInput] = useState(false)
  const [routeParamsText, setRouteParamsText] = useState('')
  const [queryParamsText, setQueryParamsText] = useState('')
  const [bodyText, setBodyText] = useState('')
  const [executionInputError, setExecutionInputError] = useState<string | null>(null)
  const [selectedNode, setSelectedNode] = useState<Node<WorkflowNodeData> | null>(null)
  const [canvasSyncKey, setCanvasSyncKey] = useState(0)

  const [initialNodes, setInitialNodes] = useState<Node<WorkflowNodeData>[]>([])
  const [initialEdges, setInitialEdges] = useState<Edge[]>([])
  const [opNoPairs, setOpNoPairs] = useState<{ op: Operacao; no: No }[] | null>(null)
  const [canvasState, setCanvasState] = useState<CanvasState>({ positions: {}, edges: [] })

  const nodesRef = useRef<Node<WorkflowNodeData>[]>([])
  const edgesRef = useRef<Edge[]>([])
  const initialNoIdsRef = useRef<Set<number>>(new Set())
  const tempIdCounter = useRef(-1)

  const getTempId = useCallback(() => tempIdCounter.current--, [])

  useEffect(() => {
    async function load() {
      const entradaRaw = typeof window !== 'undefined' ? localStorage.getItem(ENTRADA_KEY(id)) : null
      if (entradaRaw) {
        try {
          const entrada = JSON.parse(entradaRaw) as EntradaFluxo
          setRouteParamsText(entrada.routeParams ? JSON.stringify(entrada.routeParams, null, 2) : '')
          setQueryParamsText(entrada.queryParams ? JSON.stringify(entrada.queryParams, null, 2) : '')
          setBodyText(entrada.body != null ? JSON.stringify(entrada.body, null, 2) : '')
        } catch {
          localStorage.removeItem(ENTRADA_KEY(id))
        }
      } else {
        setRouteParamsText('')
        setQueryParamsText('')
        setBodyText('')
      }

      if (isDraft) {
        const draftName = sessionStorage.getItem('workflow-draft-name') ?? 'Novo workflow'
        const draftFluxo: Fluxo = { id: 0, nome: draftName, operacoes: [] }

        setFluxo(draftFluxo)
        setNome(draftName)
        setCanvasState({ positions: {}, edges: [] })
        setOpNoPairs([])
        setInitialEdges([])
        nodesRef.current = []
        edgesRef.current = []
        initialNoIdsRef.current = new Set()
        return
      }

      const f = await getFluxo(id).catch(() => null)
      if (!f) { router.push('/workflows'); return }

      const operacoes = f.operacoes ?? []
      const nos = await Promise.all(operacoes.map(op => getNo(op.noId).catch(() => null)))
      const pairs = operacoes
        .map((op, i) => ({ op, no: nos[i] }))
        .filter((p): p is { op: Operacao; no: No } => p.no != null)

      const canvasRaw = typeof window !== 'undefined' ? localStorage.getItem(CANVAS_KEY(id)) : null
      const canvas: CanvasState = canvasRaw ? JSON.parse(canvasRaw) : { positions: {}, edges: [] }

      initialNoIdsRef.current = new Set(pairs.map(p => p.no.id))
      edgesRef.current = canvas.edges
      setCanvasState(canvas)
      setInitialEdges(canvas.edges)
      setOpNoPairs(pairs)
      setNome(f.nome)
      setFluxo(f)
    }

    load()
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [id, isDraft])

  // Build React Flow nodes once raw data is fetched. Kept separate from the fetch so it
  // re-runs when integrações/endpoints/funções finish loading and the labels can resolve.
  useEffect(() => {
    if (!opNoPairs) return

    const ordered = [...opNoPairs].sort((a, b) => a.op.ordem - b.op.ordem)

    const builtNodes: Node<WorkflowNodeData>[] = ordered.map(({ op, no }, i) => {
      const pos = canvasState.positions[String(no.id)] ?? { x: 100 + i * 260, y: 200 }

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
        usarDadosAnterior: op.usarDadosAnterior,
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

      return { id: String(no.id), type: nodeType, position: pos, data }
    })

    setInitialNodes(builtNodes)
    nodesRef.current = builtNodes

    // DB-created flows have no saved canvas edges; derive a sequential chain from `ordem`
    // so the implied data flow (each operação feeds the next) is shown.
    if (canvasState.edges.length === 0 && ordered.length > 1) {
      const derived: Edge[] = []
      for (let i = 0; i < ordered.length - 1; i++) {
        const source = String(ordered[i].no.id)
        const target = String(ordered[i + 1].no.id)
        derived.push({ id: `e-${source}-${target}`, source, target, animated: true })
      }
      setInitialEdges(derived)
      edgesRef.current = derived
    }
  }, [opNoPairs, canvasState, endpoints, integracoes, funcoes])

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

      const sorted = orderNodesForExecution(currentNodes, currentEdges)
      const currentNoIds = new Set(currentNodes.map(n => n.data.noId as number).filter(n => n > 0))
      const removedNoIds = [...initialNoIdsRef.current].filter(noId => !currentNoIds.has(noId))

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

      for (const noId of removedNoIds) {
        await deleteNo(noId).catch(() => {})
      }

      const savedFluxo = isDraft
        ? await createFluxo({ nome, operacoes })
        : await updateFluxo(id, { nome, operacoes })
      const savedId = savedFluxo.id

      const remappedEdges = currentEdges.map(edge => {
        const source = String(idMap.get(edge.source) ?? edge.source)
        const target = String(idMap.get(edge.target) ?? edge.target)

        return {
          ...edge,
          id: `e-${source}-${target}`,
          source,
          target,
        }
      })

      // Save canvas state
      const positions: CanvasState['positions'] = {}
      for (const node of nodesRef.current) {
        const realId = idMap.get(node.id) ?? (node.data.noId as number)
        if (realId > 0) positions[String(realId)] = node.position
      }
      localStorage.setItem(CANVAS_KEY(savedId), JSON.stringify({ positions, edges: remappedEdges }))

      // Update initial ids
      initialNoIdsRef.current = new Set(nodesRef.current.map(n => n.data.noId as number).filter(n => n > 0))
      edgesRef.current = remappedEdges
      setInitialNodes(nodesRef.current)
      setInitialEdges(remappedEdges)
      setCanvasSyncKey(key => key + 1)
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
    setExecutionInputError(null)
    try {
      const entrada = buildEntradaFluxo(routeParamsText, queryParamsText, bodyText)

      if (typeof window !== 'undefined') {
        if (entrada) {
          localStorage.setItem(ENTRADA_KEY(fluxo.id), JSON.stringify(entrada))
        } else {
          localStorage.removeItem(ENTRADA_KEY(fluxo.id))
        }
      }

      setExecutionResult(await executarFluxo(fluxo.id, entrada))
    } catch (e) {
      const message = e instanceof Error ? e.message : 'Erro ao executar workflow'
      setExecutionInputError(message)
      alert(message)
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

  const handleDeleteNode = useCallback((nodeId: string) => {
    const nextNodes = nodesRef.current.filter(n => n.id !== nodeId)
    const nextEdges = edgesRef.current.filter(e => e.source !== nodeId && e.target !== nodeId)

    nodesRef.current = nextNodes
    edgesRef.current = nextEdges
    setInitialNodes(nextNodes)
    setInitialEdges(nextEdges)
    setSelectedNode(null)
    setCanvasSyncKey(key => key + 1)
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
            variant={showExecutionInput ? 'default' : 'outline'}
            className="gap-1.5 rounded-none h-7 text-xs"
            onClick={() => setShowExecutionInput(value => !value)}
            disabled={isDraft}
          >
            <BracketsCurlyIcon size={12} />
            Entradas
          </Button>

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
        <AnimatePresence>
          {showExecutionInput && !isDraft && (
            <motion.div
              initial={{ opacity: 0, y: -8 }}
              animate={{ opacity: 1, y: 0 }}
              exit={{ opacity: 0, y: -8 }}
              transition={{ duration: 0.15 }}
              className="absolute left-[292px] top-14 z-20 w-[360px] border border-border bg-white/95 p-4 text-xs shadow-sm"
            >
              <div className="flex items-start justify-between gap-3">
                <div>
                  <p className="font-medium">Entradas da execução</p>
                  <p className="mt-1 text-[11px] text-muted-foreground">
                    Use nos nós com {'{{routeParams.nome}}'}, {'{{queryParams.nome}}'} e {'{{body.campo}}'}.
                  </p>
                </div>
                <button
                  onClick={() => setShowExecutionInput(false)}
                  className="text-muted-foreground hover:text-foreground transition-colors"
                  title="Fechar"
                >
                  <XIcon size={12} />
                </button>
              </div>

              <div className="mt-4 space-y-3">
                <ExecutionInputField
                  label="Route params"
                  value={routeParamsText}
                  onChange={setRouteParamsText}
                  placeholder={'{\n  "uf": "SP",\n  "codigoSerie": "11"\n}'}
                  rows={4}
                />
                <ExecutionInputField
                  label="Query params"
                  value={queryParamsText}
                  onChange={setQueryParamsText}
                  placeholder={'{\n  "query": "eleicoes",\n  "rows": "5"\n}'}
                  rows={4}
                />
                <ExecutionInputField
                  label="Body"
                  value={bodyText}
                  onChange={setBodyText}
                  placeholder={'{\n  "query": "partidos",\n  "rows": 5\n}'}
                  rows={6}
                />

                {executionInputError && (
                  <p className="border border-destructive/20 bg-destructive/10 p-2 text-[11px] text-destructive">
                    {executionInputError}
                  </p>
                )}

                <Button
                  size="sm"
                  variant="outline"
                  className="h-7 w-full rounded-none text-xs"
                  onClick={() => {
                    try {
                      const entrada = buildEntradaFluxo(routeParamsText, queryParamsText, bodyText)
                      if (entrada) {
                        localStorage.setItem(ENTRADA_KEY(id), JSON.stringify(entrada))
                      } else {
                        localStorage.removeItem(ENTRADA_KEY(id))
                      }
                      setExecutionInputError(null)
                    } catch (e) {
                      setExecutionInputError(e instanceof Error ? e.message : 'Entrada inválida')
                    }
                  }}
                >
                  Salvar entradas
                </Button>
              </div>
            </motion.div>
          )}
        </AnimatePresence>

        {executionResult && (
          <div className="absolute right-4 top-14 z-20 max-w-md border border-border bg-white/95 p-3 text-xs shadow-sm">
            <div className="flex items-start justify-between gap-3">
              <p className="font-medium">
                Execução {executionResult.sucesso === false ? 'finalizada com erro' : 'finalizada'}
                {executionResult.status != null && (
                  <span className="ml-1 font-normal text-muted-foreground">
                    (HTTP {executionResult.status})
                  </span>
                )}
              </p>
              <button
                onClick={() => setExecutionResult(null)}
                className="text-muted-foreground hover:text-foreground transition-colors"
                title="Fechar"
              >
                <XIcon size={12} />
              </button>
            </div>
            {executionResult.retorno?.map((item, index) => (
              <p
                key={`${item.mensagem}-${index}`}
                className={item.erro ? 'mt-1 text-destructive' : 'mt-1 text-muted-foreground'}
              >
                {item.mensagem}
              </p>
            ))}
            {(() => {
              const body = executionResult.respostaBody
                ?? (executionResult.resposta != null
                  ? JSON.stringify(executionResult.resposta, null, 2)
                  : null)

              return body ? (
                <pre className="mt-2 max-h-60 overflow-auto bg-muted p-2 font-mono text-[11px] whitespace-pre-wrap break-all">
                  {body}
                </pre>
              ) : (
                <p className="mt-1 text-muted-foreground">Sem corpo de resposta.</p>
              )
            })()}
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
            syncKey={canvasSyncKey}
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
                  onDelete={() => handleDeleteNode(selectedNode.id)}
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

function ExecutionInputField({
  label,
  value,
  onChange,
  placeholder,
  rows,
}: {
  label: string
  value: string
  onChange: (value: string) => void
  placeholder: string
  rows: number
}) {
  return (
    <div className="flex flex-col gap-1">
      <label className="text-[10px] uppercase tracking-wider text-muted-foreground">{label}</label>
      <textarea
        value={value}
        onChange={e => onChange(e.target.value)}
        placeholder={placeholder}
        rows={rows}
        className="w-full border border-input bg-background px-2 py-2 text-[11px] font-mono resize-y outline-none focus:ring-1 focus:ring-ring"
      />
    </div>
  )
}

function NodeDetailPanel({
  node,
  onClose,
  onDelete,
  onUpdate,
}: {
  node: Node<WorkflowNodeData>
  onClose: () => void
  onDelete: () => void
  onUpdate: (node: Node<WorkflowNodeData>) => void
}) {
  const d = node.data
  const requestAllowsBody = d.tipo === 1 && (d.verbo == null || (d.verbo !== 1 && d.verbo !== 4))
  const [erro, setErro] = useState<TipoErro>(d.erro)
  const [maximoRepeticao, setMaximoRepeticao] = useState(d.maximoRepeticao ?? 0)
  const [timeout, setTimeout_] = useState(d.timeout ?? 30000)
  const [backoffType, setBackoffType] = useState<BackoffType>(d.backoffType)
  const [backoffDelay, setBackoffDelay] = useState(d.backoffDelay ?? 0)
  const [backoffMultiplier, setBackoffMultiplier] = useState(d.backoffMultiplier ?? 1)
  const [chaveValor, setChaveValor] = useState(d.chaveValor ?? '')
  const [body, setBody] = useState(d.body ?? '')
  const [headers, setHeaders] = useState(d.headers ?? '')
  const [usarDadosAnterior, setUsarDadosAnterior] = useState(Boolean(d.usarDadosAnterior))

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
        usarDadosAnterior: requestAllowsBody ? usarDadosAnterior : false,
        body: requestAllowsBody ? (body.trim() || null) : null,
        headers: d.tipo === 1 ? (headers.trim() || null) : d.headers,
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
            <div className="border-t pt-4 space-y-3">
              <p className="text-[10px] uppercase tracking-wider text-muted-foreground font-medium">Entrada da requisição</p>

              {requestAllowsBody ? (
                <>
                  <label className="flex items-start gap-2 border border-border bg-muted/20 p-2">
                    <input
                      type="checkbox"
                      checked={usarDadosAnterior}
                      onChange={e => setUsarDadosAnterior(e.target.checked)}
                      className="mt-0.5"
                    />
                    <span className="text-xs">
                      <span className="block font-medium">Usar resposta anterior como body inteiro</span>
                      <span className="block text-[11px] text-muted-foreground">
                        Quando marcado, o body abaixo é ignorado e o resultado do nó anterior vira o corpo da requisição.
                      </span>
                    </span>
                  </label>

                  <div className="flex flex-col gap-1">
                    <label className="text-[10px] uppercase tracking-wider text-muted-foreground">Body JSON</label>
                    <textarea
                      value={body}
                      onChange={e => setBody(e.target.value)}
                      disabled={usarDadosAnterior}
                      placeholder={'{\n  "clienteId": "{{id}}",\n  "email": "{{data.email}}"\n}'}
                      rows={8}
                      className="w-full border border-input bg-background px-2 py-2 text-[11px] font-mono resize-y outline-none focus:ring-1 focus:ring-ring disabled:opacity-50"
                    />
                  </div>
                </>
              ) : (
                <p className="text-[10px] text-muted-foreground">
                  Este método não envia body. Use placeholders no recurso do endpoint ou nos headers para consumir dados anteriores.
                </p>
              )}

              <div className="flex flex-col gap-1">
                <label className="text-[10px] uppercase tracking-wider text-muted-foreground">Headers JSON</label>
                <textarea
                  value={headers}
                  onChange={e => setHeaders(e.target.value)}
                  placeholder={'{\n  "x-request-id": "{{requestId}}"\n}'}
                  rows={5}
                  className="w-full border border-input bg-background px-2 py-2 text-[11px] font-mono resize-y outline-none focus:ring-1 focus:ring-ring"
                />
              </div>
              <p className="text-[10px] text-muted-foreground">
                Placeholders aceitos: {'{{routeParams.uf}}'}, {'{{queryParams.query}}'}, {'{{body.query}}'}, {'{{data.id}}'}.
              </p>
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

        <div className="grid grid-cols-2 gap-2">
          <Button size="sm" className="rounded-none h-7 text-xs" onClick={apply}>
            Aplicar
          </Button>
          <Button size="sm" variant="outline" className="rounded-none h-7 text-xs gap-1.5 text-destructive hover:text-destructive" onClick={onDelete}>
            <TrashIcon size={12} />
            Remover
          </Button>
        </div>
      </div>
    </>
  )
}
