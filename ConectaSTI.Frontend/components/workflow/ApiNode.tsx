'use client'

import { memo } from 'react'
import { Handle, Position, type NodeProps } from '@xyflow/react'
import type { WorkflowNodeData } from '@/lib/types'
import { verboToHttpMethod, METHOD_NODE_COLORS } from '@/lib/types'
import { MethodBadge } from '@/components/MethodBadge'
import { cn } from '@/lib/utils'
import { CircleIcon } from '@phosphor-icons/react'

export const ApiNode = memo(function ApiNode({ data, selected }: NodeProps) {
  const d = data as WorkflowNodeData
  const method = d.verbo ? verboToHttpMethod(d.verbo) : 'GET'
  const borderColor = METHOD_NODE_COLORS[method]

  return (
    <div
      className={cn(
        'bg-white border border-border border-l-4 min-w-[220px] max-w-[280px] shadow-sm transition-shadow',
        borderColor,
        selected && 'shadow-md ring-1 ring-primary/30'
      )}
    >
      <Handle
        type="target"
        position={Position.Left}
        className="!w-3 !h-3 !bg-white !border-2 !border-muted-foreground/40 hover:!border-primary transition-colors"
      />

      <div className="flex items-center gap-2 px-3 py-2 border-b border-border bg-muted/20">
        <MethodBadge method={method} />
        <span className="text-[11px] text-muted-foreground truncate">{d.integracaoNome ?? '—'}</span>
      </div>

      <div className="px-3 py-2.5">
        <div className="font-mono text-xs font-medium truncate">{d.recurso ?? d.label}</div>
        {d.label && d.label !== d.recurso && (
          <div className="text-[11px] text-muted-foreground mt-0.5 truncate">{d.label}</div>
        )}
      </div>

      <div className="px-3 pb-2 flex items-center gap-1">
        <CircleIcon size={6} weight="fill" className="text-muted-foreground/40" />
        <span className="text-[10px] text-muted-foreground/60">requisição</span>
      </div>

      <Handle
        type="source"
        position={Position.Right}
        className="!w-3 !h-3 !bg-white !border-2 !border-muted-foreground/40 hover:!border-primary transition-colors"
      />
    </div>
  )
})
