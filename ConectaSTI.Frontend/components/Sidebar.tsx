'use client'

import {
  HouseIcon,
  LinkSimpleIcon,
  PlugIcon,
  GitBranchIcon,
  CodeIcon,
  type Icon,
} from '@phosphor-icons/react'
import { useState } from 'react'
import { useRouter, usePathname } from 'next/navigation'

const options: {
  id: string
  label: string
  href: string
  icon: Icon
}[] = [
  { id: 'home',      label: 'Visão Geral', href: '/',          icon: HouseIcon        },
  { id: 'apis',      label: 'APIs',        href: '/apis',      icon: LinkSimpleIcon   },
  { id: 'endpoints', label: 'Endpoints',   href: '/endpoints', icon: PlugIcon         },
  { id: 'funcoes',   label: 'Funções JS',  href: '/funcoes',   icon: CodeIcon         },
  { id: 'workflows', label: 'Workflows',   href: '/workflows', icon: GitBranchIcon    },
]

export default function Sidebar() {
  const router = useRouter()
  const pathname = usePathname()
  const [hovered, setHovered] = useState<string>('')

  const isBuilder = /^\/workflows\/.+/.test(pathname)
  if (isBuilder) return null

  const active = options.find(o => o.href === '/' ? pathname === '/' : pathname.startsWith(o.href))?.id ?? ''

  return (
    <div className="flex flex-col items-center backdrop-blur-sm border-r border-border p-4 w-52 shrink-0">
      <div className="text-base font-bold mb-6 border-b border-border pb-3 w-full text-center tracking-tight">
        ConectaSTI
      </div>

      <nav className="flex flex-col gap-1 w-full">
        {options.map(option => {
          const isActive = active === option.id
          const isHovered = hovered === option.id
          return (
            <button
              key={option.id}
              className={`
                w-full flex items-center gap-2.5 py-2 px-3 text-xs transition-all duration-150 text-left
                ${isActive
                  ? 'bg-primary text-primary-foreground'
                  : isHovered
                    ? 'bg-muted/60 text-foreground'
                    : 'text-muted-foreground hover:text-foreground'
                }
              `}
              onMouseEnter={() => setHovered(option.id)}
              onMouseLeave={() => setHovered('')}
              onClick={() => router.push(option.href)}
            >
              <option.icon size={14} />
              <span>{option.label}</span>
            </button>
          )
        })}
      </nav>

      <div className="mt-auto pt-4 border-t border-border w-full">
        <p className="text-[10px] text-muted-foreground text-center">v0.1.0-dev</p>
      </div>
    </div>
  )
}
