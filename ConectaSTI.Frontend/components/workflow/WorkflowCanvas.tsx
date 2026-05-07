'use client'

import { useCallback } from 'react'
import {
  ReactFlow,
  ReactFlowProvider,
  Background,
  Controls,
  MiniMap,
  BackgroundVariant,
  addEdge,
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

interface WorkflowCanvasProps {
  initialNodes: Node<WorkflowNodeData>[]
  initialEdges: Edge[]
  onNodesChange?: (nodes: Node<WorkflowNodeData>[]) => void
  onEdgesChange?: (edges: Edge[]) => void
  onNodeSelect?: (node: Node<WorkflowNodeData> | null) => void
  getTempId: () => number
}

function FlowContent({
  initialNodes,
  initialEdges,
  onNodesChange: notifyNodes,
  onEdgesChange: notifyEdges,
  onNodeSelect,
  getTempId,
}: WorkflowCanvasProps) {
  const { screenToFlowPosition } = useReactFlow()
  const [nodes, setNodes, onNodesChange] = useNodesState<Node<WorkflowNodeData>>(initialNodes)
  const [edges, setEdges, onEdgesChange] = useEdgesState(initialEdges)

  const onConnect = useCallback(
    (params: Connection) => {
      setEdges(eds => {
        const next = addEdge({ ...params, animated: true }, eds)
        notifyEdges?.(next)
        return next
      })
    },
    [setEdges, notifyEdges]
  )

  const handleNodesChange: OnNodesChange<Node<WorkflowNodeData>> = useCallback(
    (changes) => {
      onNodesChange(changes)
      setNodes(nds => {
        notifyNodes?.(nds as Node<WorkflowNodeData>[])
        return nds
      })
    },
    [onNodesChange, setNodes, notifyNodes]
  )

  const handleEdgesChange: OnEdgesChange = useCallback(
    (changes) => {
      onEdgesChange(changes)
      setEdges(eds => {
        notifyEdges?.(eds)
        return eds
      })
    },
    [onEdgesChange, setEdges, notifyEdges]
  )

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
        edges={edges}
        onNodesChange={handleNodesChange}
        onEdgesChange={handleEdgesChange}
        onConnect={onConnect}
        onDrop={onDrop}
        onDragOver={onDragOver}
        nodeTypes={nodeTypes}
        onNodeClick={(_, node) => onNodeSelect?.(node as Node<WorkflowNodeData>)}
        onPaneClick={() => onNodeSelect?.(null)}
        fitView
        fitViewOptions={{ padding: 0.4 }}
        deleteKeyCode="Delete"
        className="bg-transparent"
      >
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
