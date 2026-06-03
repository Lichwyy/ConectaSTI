'use client'

import { useCallback, useEffect, useMemo, useState } from 'react'
import {
  ReactFlow,
  ReactFlowProvider,
  Background,
  Controls,
  MiniMap,
  BackgroundVariant,
  addEdge,
  applyEdgeChanges,
  applyNodeChanges,
  useNodesState,
  useEdgesState,
  useReactFlow,
  type Connection,
  type Node,
  type Edge,
  type OnNodesChange,
  type OnEdgesChange,
} from '@xyflow/react'
import '@xyflow/react/dist/style.css'

import type { WorkflowNodeData, TipoNo } from '@/lib/types'
import { DEFAULT_OPERACAO } from '@/lib/types'
import { ApiNode } from './ApiNode'
import { FuncaoNode } from './FuncaoNode'
import { StorageNode } from './StorageNode'

const nodeTypes = {
  reqNode: ApiNode,
  funcaoNode: FuncaoNode,
  storageNode: StorageNode,
}

function createsCycle(source: string, target: string, edges: Edge[]) {
  if (source === target) return true

  const outgoing = new Map<string, string[]>()
  for (const edge of edges) {
    const targets = outgoing.get(edge.source) ?? []
    targets.push(edge.target)
    outgoing.set(edge.source, targets)
  }

  const stack = [target]
  const visited = new Set<string>()

  while (stack.length > 0) {
    const current = stack.pop()
    if (!current || visited.has(current)) continue
    if (current === source) return true

    visited.add(current)
    stack.push(...(outgoing.get(current) ?? []))
  }

  return false
}

interface WorkflowCanvasProps {
  initialNodes: Node<WorkflowNodeData>[]
  initialEdges: Edge[]
  onNodesChange?: (nodes: Node<WorkflowNodeData>[]) => void
  onEdgesChange?: (edges: Edge[]) => void
  onNodeSelect?: (node: Node<WorkflowNodeData> | null) => void
  getTempId: () => number
  syncKey?: number
}

function FlowContent({
  initialNodes,
  initialEdges,
  onNodesChange: notifyNodes,
  onEdgesChange: notifyEdges,
  onNodeSelect,
  getTempId,
  syncKey = 0,
}: WorkflowCanvasProps) {
  const { screenToFlowPosition, fitView } = useReactFlow()
  const [nodes, setNodes] = useNodesState<Node<WorkflowNodeData>>(initialNodes)
  const [edges, setEdges] = useEdgesState(initialEdges)
  const [selectedEdgeId, setSelectedEdgeId] = useState<string | null>(null)

  const displayedEdges = useMemo(() => (
    edges.map(edge => ({
      ...edge,
      selected: edge.id === selectedEdgeId,
      style: edge.id === selectedEdgeId
        ? { ...(edge.style ?? {}), strokeWidth: 3, stroke: 'hsl(var(--primary))' }
        : edge.style,
    }))
  ), [edges, selectedEdgeId])

  // initialNodes/Edges arrive async (after the fluxo + nós are fetched). useNodesState
  // only reads them on first mount, so sync them in when they load. Guarded on length so
  // a freshly dragged draft canvas is never wiped by an empty initial set.
  useEffect(() => {
    if (initialNodes.length === 0 && syncKey === 0) return
    setNodes(initialNodes)
    if (initialNodes.length > 0) {
      requestAnimationFrame(() => fitView({ padding: 0.4 }))
    }
  }, [initialNodes, setNodes, fitView, syncKey])

  useEffect(() => {
    if (initialEdges.length === 0 && syncKey === 0) return
    setEdges(initialEdges)
  }, [initialEdges, setEdges, syncKey])

  const onConnect = useCallback(
    (params: Connection) => {
      if (!params.source || !params.target || params.source === params.target) {
        return
      }

      setEdges(eds => {
        const withoutPortConflicts = eds.filter(edge =>
          edge.source !== params.source && edge.target !== params.target
        )

        if (createsCycle(params.source!, params.target!, withoutPortConflicts)) {
          return eds
        }

        const next = addEdge({ ...params, animated: true }, withoutPortConflicts)
        notifyEdges?.(next)
        setSelectedEdgeId(null)
        return next
      })
    },
    [setEdges, notifyEdges]
  )

  const handleNodesChange: OnNodesChange<Node<WorkflowNodeData>> = useCallback(
    (changes) => {
      setNodes(nds => {
        const next = applyNodeChanges(changes, nds)
        notifyNodes?.(next as Node<WorkflowNodeData>[])
        return next
      })

      setEdges(eds => {
        const removedNodeIds = new Set(changes.filter(change => change.type === 'remove').map(change => change.id))
        if (removedNodeIds.size === 0) return eds

        const next = eds.filter(edge => !removedNodeIds.has(edge.source) && !removedNodeIds.has(edge.target))
        notifyEdges?.(next)
        return next
      })
    },
    [setNodes, setEdges, notifyNodes, notifyEdges]
  )

  const handleEdgesChange: OnEdgesChange = useCallback(
    (changes) => {
      setEdges(eds => {
        const next = applyEdgeChanges(changes, eds)
        notifyEdges?.(next)

        if (selectedEdgeId && !next.some(edge => edge.id === selectedEdgeId)) {
          setSelectedEdgeId(null)
        }

        return next
      })
    },
    [setEdges, notifyEdges, selectedEdgeId]
  )

  const disconnectSelectedEdge = useCallback(() => {
    if (!selectedEdgeId) return

    setEdges(eds => {
      const next = eds.filter(edge => edge.id !== selectedEdgeId)
      notifyEdges?.(next)
      return next
    })
    setSelectedEdgeId(null)
  }, [selectedEdgeId, setEdges, notifyEdges])

  const onDrop = useCallback(
    (event: React.DragEvent) => {
      event.preventDefault()
      const raw = event.dataTransfer.getData('application/reactflow')
      if (!raw) return

      const payload = JSON.parse(raw) as {
        nodeType: string
        tipo: TipoNo
        data: Partial<WorkflowNodeData>
        label: string
      }

      const position = screenToFlowPosition({ x: event.clientX, y: event.clientY })

      const newNode: Node<WorkflowNodeData> = {
        id: `tmp-${Math.abs(getTempId())}`,
        type: payload.nodeType,
        position,
        data: {
          noId: getTempId(),
          tipo: payload.tipo,
          label: payload.label,
          ...payload.data,
          ...DEFAULT_OPERACAO,
        },
      }

      setNodes(nds => {
        const next = [...nds, newNode]
        notifyNodes?.(next as Node<WorkflowNodeData>[])
        return next
      })
    },
    [screenToFlowPosition, setNodes, notifyNodes, getTempId]
  )

  const onDragOver = useCallback((event: React.DragEvent) => {
    event.preventDefault()
    event.dataTransfer.dropEffect = 'move'
  }, [])

  return (
    <div className="flex-1 h-full w-full">
      <ReactFlow
        nodes={nodes}
        edges={displayedEdges}
        onNodesChange={handleNodesChange}
        onEdgesChange={handleEdgesChange}
        onConnect={onConnect}
        onDrop={onDrop}
        onDragOver={onDragOver}
        nodeTypes={nodeTypes}
        onEdgeClick={(_, edge) => setSelectedEdgeId(edge.id)}
        onEdgeDoubleClick={(_, edge) => {
          setEdges(eds => {
            const next = eds.filter(item => item.id !== edge.id)
            notifyEdges?.(next)
            return next
          })
          setSelectedEdgeId(null)
        }}
        onNodeClick={(_, node) => onNodeSelect?.(node as Node<WorkflowNodeData>)}
        onPaneClick={() => {
          setSelectedEdgeId(null)
          onNodeSelect?.(null)
        }}
        fitView
        fitViewOptions={{ padding: 0.4 }}
        deleteKeyCode="Delete"
        className="bg-transparent"
      >
        {selectedEdgeId && (
          <div className="absolute right-3 top-3 z-10 border border-border bg-white px-2 py-1 shadow-sm">
            <button
              type="button"
              onClick={disconnectSelectedEdge}
              className="text-[11px] font-medium text-destructive hover:underline"
            >
              Desconectar ligação
            </button>
          </div>
        )}
        <Background variant={BackgroundVariant.Dots} gap={20} size={1} color="#c8c8c8" />
        <Controls className="!rounded-none !border-border !shadow-none [&>button]:!rounded-none [&>button]:!border-border" />
        <MiniMap
          className="!rounded-none !border !border-border !shadow-none"
          nodeColor="#e5e7eb"
          maskColor="rgba(255,255,255,0.7)"
        />
      </ReactFlow>
    </div>
  )
}

export function WorkflowCanvas(props: WorkflowCanvasProps) {
  return (
    <ReactFlowProvider>
      <FlowContent {...props} />
    </ReactFlowProvider>
  )
}
