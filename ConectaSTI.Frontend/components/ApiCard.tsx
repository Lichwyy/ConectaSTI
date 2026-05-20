import { cn } from '@/lib/utils'
import type { Integracao, EndPoint } from '@/lib/types'
import { verboToHttpMethod } from '@/lib/types'
import { MethodBadge } from './MethodBadge'
import { LinkSimpleIcon, LockKeyIcon } from '@phosphor-icons/react'

interface ApiCardProps {
  api: Integracao
  endpoints: EndPoint[]
  selected: boolean
  onClick: () => void
}

export function ApiCard({ api, endpoints, selected, onClick }: ApiCardProps) {
  const methods = [...new Set(endpoints.map(e => verboToHttpMethod(e.verbo)))]

  return (
    <button
      onClick={onClick}
      className={cn(
        'w-full text-left border-l-4 border-l-transparent bg-white border border-border rounded-none p-4 transition-all duration-150 hover:border-l-primary hover:bg-muted/40 group',
        selected && 'border-l-primary bg-muted/40 ring-1 ring-primary/20'
      )}
    >
      <div className="flex items-start justify-between gap-2">
        <span className="font-medium text-sm truncate">{api.nome}</span>
        {api.token && (
          <span className="shrink-0 inline-flex items-center gap-1 text-[10px] border border-amber-300 bg-amber-50 text-amber-700 px-1.5 py-0.5 rounded font-mono">
            <LockKeyIcon size={10} />
            TOKEN
          </span>
        )}
      </div>

      <div className="mt-1 flex items-center gap-1.5 text-muted-foreground text-xs">
        <LinkSimpleIcon size={11} />
        <span className="truncate">{api.url}</span>
      </div>

      <div className="mt-2.5 flex items-center justify-between">
        <div className="flex flex-wrap gap-1">
          {methods.map(m => <MethodBadge key={m} method={m} />)}
        </div>
        <span className="text-[11px] text-muted-foreground shrink-0">
          {endpoints.length} endpoint{endpoints.length !== 1 ? 's' : ''}
        </span>
      </div>
    </button>
  )
}
