import { cn } from '@/lib/utils'
import { METHOD_COLORS, type HttpMethod } from '@/lib/types'

export function MethodBadge({ method, className }: { method: HttpMethod; className?: string }) {
  const { bg, text, border } = METHOD_COLORS[method]
  return (
    <span
      className={cn(
        'inline-flex items-center rounded border px-1.5 py-0.5 text-[10px] font-bold tracking-wider font-mono',
        bg, text, border, className
      )}
    >
      {method}
    </span>
  )
}
