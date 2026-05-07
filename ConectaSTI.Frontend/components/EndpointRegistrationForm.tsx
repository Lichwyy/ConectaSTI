'use client'

import { useState } from 'react'
import { motion } from 'motion/react'
import type { Integracao, EndPoint, HttpMethod, VerboHttp } from '@/lib/types'
import { httpMethodToVerbo, verboToHttpMethod } from '@/lib/types'
import { Input } from '@/components/ui/input'
import { Button } from '@/components/ui/button'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { FloppyDiskIcon, XIcon } from '@phosphor-icons/react'
import { MethodBadge } from './MethodBadge'
import { cn } from '@/lib/utils'

const METHODS: HttpMethod[] = ['GET', 'POST', 'PUT', 'DELETE', 'PATCH']

const METHOD_CLASSES: Record<HttpMethod, string> = {
  GET:    'border-emerald-400 bg-emerald-50 text-emerald-700',
  POST:   'border-indigo-400 bg-indigo-50 text-indigo-700',
  PUT:    'border-amber-400 bg-amber-50 text-amber-700',
  DELETE: 'border-red-400 bg-red-50 text-red-700',
  PATCH:  'border-violet-400 bg-violet-50 text-violet-700',
}

interface EndpointRegistrationFormProps {
  apis: Integracao[]
  initial?: EndPoint
  prefilledApiId?: number
  onSave: (data: Omit<EndPoint, 'id' | 'criadoEm'>) => Promise<void>
  onCancel: () => void
}

export function EndpointRegistrationForm({ apis, initial, prefilledApiId, onSave, onCancel }: EndpointRegistrationFormProps) {
  const [integracaoId, setIntegracaoId] = useState<number | ''>(initial?.integracaoId ?? prefilledApiId ?? '')
  const [recurso, setRecurso] = useState(initial?.recurso ?? '')
  const [method, setMethod] = useState<HttpMethod>(initial ? verboToHttpMethod(initial.verbo) : 'GET')
  const [descricao, setDescricao] = useState(initial?.descricao ?? '')
  const [saving, setSaving] = useState(false)
  const [errors, setErrors] = useState<Record<string, string>>({})

  const isApiFixed = Boolean(prefilledApiId)
  const selectedApi = apis.find(a => a.id === integracaoId)

  function validate() {
    const e: Record<string, string> = {}
    if (!integracaoId) e.integracaoId = 'Selecione uma API'
    if (!recurso.trim()) e.recurso = 'Endpoint obrigatório'
    return e
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    const errs = validate()
    if (Object.keys(errs).length) { setErrors(errs); return }
    setSaving(true)
    try {
      await onSave({
        integracaoId: integracaoId as number,
        recurso: recurso.trim(),
        verbo: httpMethodToVerbo(method) as VerboHttp,
        descricao: descricao.trim() || null,
      })
    } finally {
      setSaving(false)
    }
  }

  return (
    <motion.form
      initial={{ opacity: 0, x: 16 }}
      animate={{ opacity: 1, x: 0 }}
      transition={{ duration: 0.25 }}
      onSubmit={handleSubmit}
      className="flex flex-col gap-5 p-6 h-full"
    >
      <div className="flex items-center justify-between border-b pb-4">
        <div>
          <h2 className="font-semibold text-sm">{initial ? 'Editar Endpoint' : 'Novo Endpoint'}</h2>
          <p className="text-xs text-muted-foreground mt-0.5">
            {isApiFixed && selectedApi
              ? <>Adicionando endpoint em <span className="font-medium text-foreground">{selectedApi.nome}</span></>
              : 'Preencha os dados do endpoint.'}
          </p>
        </div>
        <button type="button" onClick={onCancel} className="text-muted-foreground hover:text-foreground transition-colors">
          <XIcon size={16} />
        </button>
      </div>

      <div className="flex flex-col gap-1.5">
        <label className="text-xs font-medium">API <span className="text-destructive">*</span></label>
        {isApiFixed && selectedApi ? (
          <div className="border border-border bg-muted/30 px-3 py-2 text-xs font-mono text-muted-foreground rounded-none">
            {selectedApi.nome} — <span className="text-foreground">{selectedApi.url}</span>
          </div>
        ) : (
          <Select value={integracaoId ? String(integracaoId) : ''} onValueChange={v => setIntegracaoId(Number(v))}>
            <SelectTrigger className="rounded-none text-xs font-mono">
              <SelectValue placeholder="Selecione a API..." />
            </SelectTrigger>
            <SelectContent>
              {apis.map(api => (
                <SelectItem key={api.id} value={String(api.id)} className="text-xs font-mono">
                  {api.nome}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        )}
        {errors.integracaoId && <p className="text-[11px] text-destructive">{errors.integracaoId}</p>}
      </div>

      <div className="flex flex-col gap-1.5">
        <label className="text-xs font-medium">Método HTTP <span className="text-destructive">*</span></label>
        <div className="flex gap-2 flex-wrap">
          {METHODS.map(m => (
            <button
              key={m}
              type="button"
              onClick={() => setMethod(m)}
              className={cn(
                'border px-3 py-1.5 text-[11px] font-bold tracking-wider font-mono transition-all rounded-none',
                method === m
                  ? METHOD_CLASSES[m]
                  : 'border-border text-muted-foreground hover:bg-muted/40'
              )}
            >
              {m}
            </button>
          ))}
        </div>
      </div>

      <div className="flex flex-col gap-1.5">
        <label className="text-xs font-medium">Endpoint (recurso) <span className="text-destructive">*</span></label>
        <div className="flex items-center border border-input rounded-none overflow-hidden">
          {selectedApi && (
            <span className="bg-muted/50 px-2 py-2 text-[10px] font-mono text-muted-foreground border-r border-input shrink-0 max-w-[180px] truncate">
              {selectedApi.url}
            </span>
          )}
          <Input
            value={recurso}
            onChange={e => setRecurso(e.target.value)}
            placeholder="/recurso ou /recurso/{id}"
            className="font-mono text-xs border-0 rounded-none flex-1 focus-visible:ring-0"
          />
        </div>
        {selectedApi && recurso && (
          <div className="flex items-center gap-1.5 mt-0.5">
            <MethodBadge method={method} />
            <span className="text-[11px] font-mono text-muted-foreground truncate">{selectedApi.url}{recurso}</span>
          </div>
        )}
        {errors.recurso && <p className="text-[11px] text-destructive">{errors.recurso}</p>}
      </div>

      <div className="flex flex-col gap-1.5">
        <label className="text-xs font-medium">Descrição <span className="text-muted-foreground font-normal">(opcional)</span></label>
        <textarea
          value={descricao ?? ''}
          onChange={e => setDescricao(e.target.value)}
          placeholder="Descreva o que este endpoint faz..."
          rows={3}
          className="w-full border border-input bg-background px-3 py-2 text-xs font-mono resize-none outline-none focus:ring-1 focus:ring-ring transition-shadow rounded-none"
        />
      </div>

      <div className="mt-auto flex gap-2 pt-4 border-t">
        <Button type="submit" disabled={saving} size="sm" className="gap-1.5 rounded-none">
          <FloppyDiskIcon size={14} />
          {saving ? 'Salvando...' : 'Salvar Endpoint'}
        </Button>
        <Button type="button" variant="outline" size="sm" onClick={onCancel} className="rounded-none">
          Cancelar
        </Button>
      </div>
    </motion.form>
  )
}
