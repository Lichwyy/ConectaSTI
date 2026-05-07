'use client'

import { memo } from 'react'
import { Handle, Position, type NodeProps } from '@xyflow/react'
import type { WorkflowNodeData } from '@/lib/types'
import { cn } from '@/lib/utils'
import { HardDriveIcon, DatabaseIcon } from '@phosphor-icons/react'

export const StorageNode = memo(function StorageNode({ data, selected }: NodeProps) {
  const d = data as WorkflowNodeData
  const isSave = d.tipo === 3
  const borderColor = isSave ? 'border-l-orange-500' : 'border-l-slate-500'
  const ringColor = isSave ? 'ring-orange-500/30' : 'ring-slate-500/30'
  const headerBg = isSave ? 'bg-orange-50/50' : 'bg-slate-50/50'
  const iconColor = isSave ? 'text-orange-500' : 'text-slate-500'
  const labelColor = isSave ? 'text-orange-600' : 'text-slate-600'
  const Icon = isSave ? HardDriveIcon : DatabaseIcon

  return (
    <div
      className={cn(
        'bg-white border border-border border-l-4 min-w-[220px] max-w-[280px] shadow-sm transition-shadow',
        borderColor,
        selected && `shadow-md ring-1 ${ringColor}`
      )}
    >
      <Handle
        type="target"
        position={Position.Left}
        className="!w-3 !h-3 !bg-white !border-2 !border-muted-foreground/40 transition-colors"
      />

      <div className={cn('flex items-center gap-2 px-3 py-2 border-b border-border', headerBg)}>
        <Icon size={12} className={cn(iconColor, 'shrink-0')} />
        <span className={cn('text-[11px] font-medium', labelColor)}>
          {isSave ? 'Salvar Storage' : 'Pegar Storage'}
        </span>
      </div>

      <div className="px-3 py-2.5">
        <div className="font-mono text-xs font-medium truncate">
          {d.chaveValor ? `chave: ${d.chaveValor}` : d.label}
        </div>
      </div>

      <Handle
        type="source"
        position={Position.Right}
        className="!w-3 !h-3 !bg-white !border-2 !border-muted-foreground/40 transition-colors"
      />
    </div>
  )
})
