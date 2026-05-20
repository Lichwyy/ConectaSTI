import { cn } from '@/lib/utils'
import type { EndPoint } from '@/lib/types'
import { verboToHttpMethod } from '@/lib/types'
import { MethodBadge } from './MethodBadge'

interface EndpointCardProps {
  endpoint: EndPoint
  apiName?: string
  selected?: boolean
  onClick?: () => void
  onDelete?: () => void
  compact?: boolean
}

export function EndpointCard({ endpoint, apiName, selected, onClick, onDelete, compact }: EndpointCardProps) {
  const method = verboToHttpMethod(endpoint.verbo)

  return (
    <div
      onClick={onClick}
      className={cn(
        'group flex items-start gap-3 border border-border bg-white p-3 transition-all duration-150 rounded-none',
        onClick && 'cursor-pointer hover:bg-muted/40 hover:border-l-2 hover:border-l-primary',
        selected && 'bg-muted/40 border-l-2 border-l-primary ring-1 ring-primary/20',
        compact && 'p-2'
      )}
    >
      <MethodBadge method={method} className="mt-0.5 shrink-0" />
      <div className="flex-1 min-w-0">
        <div className="font-mono text-xs text-foreground truncate">{endpoint.recurso}</div>
        {!compact && endpoint.descricao && (
          <div className="text-[11px] text-muted-foreground mt-0.5 line-clamp-1">{endpoint.descricao}</div>
        )}
        {apiName && (
          <div className="text-[10px] text-muted-foreground mt-0.5 truncate">{apiName}</div>
        )}
      </div>
      {onDelete && (
        <button
          onClick={e => { e.stopPropagation(); onDelete() }}
          className="opacity-0 group-hover:opacity-100 text-muted-foreground hover:text-destructive text-xs transition-opacity px-1"
          title="Remover"
        >
          ×
        </button>
      )}
    </div>
  )
}
