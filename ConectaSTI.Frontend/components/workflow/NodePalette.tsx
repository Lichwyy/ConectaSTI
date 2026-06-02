'use client'

import { useState } from 'react'
import type { Integracao, EndPoint, Funcao } from '@/lib/types'
import { verboToHttpMethod } from '@/lib/types'
import { MethodBadge } from '@/components/MethodBadge'
import { Input } from '@/components/ui/input'
import {
  MagnifyingGlassIcon,
  CaretDownIcon,
  CaretRightIcon,
  CodeIcon,
  HardDriveIcon,
  DatabaseIcon,
} from '@phosphor-icons/react'

interface NodePaletteProps {
  integracoes: Integracao[]
  endpoints: EndPoint[]
  funcoes: Funcao[]
}

export function NodePalette({ integracoes, endpoints, funcoes }: NodePaletteProps) {
  const [search, setSearch] = useState('')
  const [collapsed, setCollapsed] = useState<Record<string, boolean>>({})

  const toggle = (id: string) => setCollapsed(c => ({ ...c, [id]: !c[id] }))

  const q = search.toLowerCase()

  const filteredIntegracoes = integracoes.filter(api => {
    if (!q) return true
    if (api.nome.toLowerCase().includes(q)) return true
    return endpoints.some(e => e.integracaoId === api.id && (
      e.recurso.toLowerCase().includes(q) || (e.descricao ?? '').toLowerCase().includes(q)
    ))
  })

  const filteredFuncoes = funcoes.filter(f =>
    !q || f.nome.toLowerCase().includes(q)
  )

  function dragData(payload: object) {
    return JSON.stringify(payload)
  }

  return (
    <div className="flex flex-col h-full w-56 border-r border-border bg-white/60 backdrop-blur-sm">
      <div className="p-3 border-b border-border">
        <p className="text-[10px] uppercase tracking-wider text-muted-foreground font-medium mb-2">
          Blocos disponíveis
        </p>
        <div className="relative">
          <MagnifyingGlassIcon size={11} className="absolute left-2.5 top-1/2 -translate-y-1/2 text-muted-foreground" />
          <Input
            value={search}
            onChange={e => setSearch(e.target.value)}
            placeholder="Buscar..."
            className="pl-7 h-7 text-xs rounded-none"
          />
        </div>
      </div>

      <div className="flex-1 overflow-y-auto p-2 space-y-3">
        {/* APIs section */}
        <div>
          <p className="text-[10px] uppercase tracking-wider text-muted-foreground font-medium px-2 py-1">APIs</p>
          {filteredIntegracoes.length === 0 && (
            <p className="text-[11px] text-muted-foreground text-center py-2">
              {integracoes.length === 0 ? 'Nenhuma API cadastrada.' : 'Sem resultados.'}
            </p>
          )}
          {filteredIntegracoes.map(api => {
            const apiEndpoints = endpoints
              .filter(e => e.integracaoId === api.id)
              .filter(e => !q || e.recurso.toLowerCase().includes(q) || (e.descricao ?? '').toLowerCase().includes(q) || api.nome.toLowerCase().includes(q))
            const isOpen = !collapsed[`api-${api.id}`]

            return (
              <div key={api.id}>
                <button
                  onClick={() => toggle(`api-${api.id}`)}
                  className="w-full flex items-center gap-1.5 px-2 py-1.5 hover:bg-muted/40 transition-colors text-left"
                >
                  {isOpen ? <CaretDownIcon size={10} className="text-muted-foreground shrink-0" /> : <CaretRightIcon size={10} className="text-muted-foreground shrink-0" />}
                  <span className="text-xs font-medium truncate flex-1">{api.nome}</span>
                  <span className="text-[10px] text-muted-foreground">{apiEndpoints.length}</span>
                </button>

                {isOpen && (
                  <div className="ml-3 space-y-0.5 mb-1">
                    {apiEndpoints.length === 0 ? (
                      <p className="text-[10px] text-muted-foreground px-2 py-1">Sem endpoints.</p>
                    ) : apiEndpoints.map(ep => (
                      <div
                        key={ep.id}
                        draggable
                        onDragStart={e => {
                          e.dataTransfer.setData('application/reactflow', dragData({
                            nodeType: 'reqNode',
                            tipo: 1,
                            label: ep.descricao || ep.recurso,
                            data: {
                              endpointId: ep.id,
                              integracaoId: api.id,
                              verbo: ep.verbo,
                              integracaoNome: api.nome,
                              recurso: ep.recurso,
                            },
                          }))
                          e.dataTransfer.effectAllowed = 'move'
                        }}
                        className="flex items-center gap-2 px-2 py-1.5 cursor-grab active:cursor-grabbing hover:bg-primary/5 border border-transparent hover:border-primary/20 transition-all select-none"
                        title={ep.descricao ?? ep.recurso}
                      >
                        <MethodBadge method={verboToHttpMethod(ep.verbo)} className="shrink-0 text-[9px] px-1 py-0" />
                        <span className="font-mono text-[11px] truncate text-foreground">{ep.recurso}</span>
                      </div>
                    ))}
                  </div>
                )}
              </div>
            )
          })}
        </div>

        {/* Funcoes section */}
        <div className="border-t pt-2">
          <button
            onClick={() => toggle('funcoes')}
            className="w-full flex items-center gap-1.5 px-2 py-1 hover:bg-muted/40 transition-colors text-left"
          >
            {!collapsed['funcoes'] ? <CaretDownIcon size={10} className="text-muted-foreground shrink-0" /> : <CaretRightIcon size={10} className="text-muted-foreground shrink-0" />}
            <p className="text-[10px] uppercase tracking-wider text-muted-foreground font-medium flex-1">Funções JS</p>
            <span className="text-[10px] text-muted-foreground">{filteredFuncoes.length}</span>
          </button>
          {!collapsed['funcoes'] && (
            <div className="ml-1 space-y-0.5 mt-1">
              {filteredFuncoes.length === 0 ? (
                <p className="text-[10px] text-muted-foreground px-2 py-1">
                  {funcoes.length === 0 ? 'Nenhuma função cadastrada.' : 'Sem resultados.'}
                </p>
              ) : filteredFuncoes.map(f => (
                <div
                  key={f.id}
                  draggable
                  onDragStart={e => {
                    e.dataTransfer.setData('application/reactflow', dragData({
                      nodeType: 'funcaoNode',
                      tipo: 2,
                      label: f.nome,
                      data: { funcaoId: f.id, funcaoNome: f.nome },
                    }))
                    e.dataTransfer.effectAllowed = 'move'
                  }}
                  className="flex items-center gap-2 px-2 py-1.5 cursor-grab active:cursor-grabbing hover:bg-violet-50 border border-transparent hover:border-violet-200 transition-all select-none"
                >
                  <CodeIcon size={11} className="text-violet-500 shrink-0" />
                  <span className="text-[11px] truncate">{f.nome}</span>
                </div>
              ))}
            </div>
          )}
        </div>

        {/* Storage section */}
        <div className="border-t pt-2">
          <p className="text-[10px] uppercase tracking-wider text-muted-foreground font-medium px-2 py-1">Storage</p>
          <div className="space-y-0.5 ml-1">
            <div
              draggable
              onDragStart={e => {
                e.dataTransfer.setData('application/reactflow', dragData({
                  nodeType: 'storageNode',
                  tipo: 3,
                  label: 'Salvar Storage',
                  data: {},
                }))
                e.dataTransfer.effectAllowed = 'move'
              }}
              className="flex items-center gap-2 px-2 py-1.5 cursor-grab active:cursor-grabbing hover:bg-orange-50 border border-transparent hover:border-orange-200 transition-all select-none"
            >
              <HardDriveIcon size={11} className="text-orange-500 shrink-0" />
              <span className="text-[11px]">Salvar Storage</span>
            </div>
            <div
              draggable
              onDragStart={e => {
                e.dataTransfer.setData('application/reactflow', dragData({
                  nodeType: 'storageNode',
                  tipo: 4,
                  label: 'Pegar Storage',
                  data: {},
                }))
                e.dataTransfer.effectAllowed = 'move'
              }}
              className="flex items-center gap-2 px-2 py-1.5 cursor-grab active:cursor-grabbing hover:bg-slate-100 border border-transparent hover:border-slate-300 transition-all select-none"
            >
              <DatabaseIcon size={11} className="text-slate-500 shrink-0" />
              <span className="text-[11px]">Pegar Storage</span>
            </div>
          </div>
        </div>
      </div>

      <div className="p-3 border-t border-border">
        <p className="text-[10px] text-muted-foreground text-center">
          Arraste um bloco para o canvas
        </p>
      </div>
    </div>
  )
}
