'use client'

import { memo } from 'react'
import { Handle, Position, type NodeProps } from '@xyflow/react'
import type { WorkflowNodeData } from '@/lib/types'
import { cn } from '@/lib/utils'
import { CodeIcon } from '@phosphor-icons/react'

export const FuncaoNode = memo(function FuncaoNode({ data, selected }: NodeProps) {
  const d = data as WorkflowNodeData

  return (
    <div
      className={cn(
        'bg-white border border-border border-l-4 border-l-violet-500 min-w-[220px] max-w-[280px] shadow-sm transition-shadow',
        selected && 'shadow-md ring-1 ring-violet-500/30'
      )}
    >
      <Handle
        type="target"
        position={Position.Left}
        className="!w-3 !h-3 !bg-white !border-2 !border-muted-foreground/40 hover:!border-violet-500 transition-colors"
      />

      <div className="flex items-center gap-2 px-3 py-2 border-b border-border bg-violet-50/50">
        <CodeIcon size={12} className="text-violet-500 shrink-0" />
        <span className="text-[11px] text-violet-600 font-medium">Função JS</span>
      </div>

      <div className="px-3 py-2.5">
        <div className="font-mono text-xs font-medium truncate">{d.funcaoNome ?? d.label}</div>
      </div>

      <Handle
        type="source"
        position={Position.Right}
        className="!w-3 !h-3 !bg-white !border-2 !border-muted-foreground/40 hover:!border-violet-500 transition-colors"
      />
    </div>
  )
})
