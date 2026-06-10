'use client'

import { useMemo, useState } from 'react'
import { motion } from 'motion/react'
import {
  FloppyDiskIcon,
  XIcon,
} from '@phosphor-icons/react'
import type { FluxoVersionado, HttpMethod, Rota, VerboHttp } from '@/lib/types'
import { httpMethodToVerbo, verboToHttpMethod } from '@/lib/types'
import type { RotaPayload } from '@/lib/api/rotas'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { cn } from '@/lib/utils'

const METHODS: HttpMethod[] = ['GET', 'POST', 'PUT', 'DELETE', 'PATCH']

const METHOD_CLASSES: Record<HttpMethod, string> = {
  GET:    'border-emerald-400 bg-emerald-50 text-emerald-700',
  POST:   'border-indigo-400 bg-indigo-50 text-indigo-700',
  PUT:    'border-amber-400 bg-amber-50 text-amber-700',
  DELETE: 'border-red-400 bg-red-50 text-red-700',
  PATCH:  'border-violet-400 bg-violet-50 text-violet-700',
}

type Props = {
  fluxosVersionados: FluxoVersionado[]
  initial?: Rota
  loadingVersions?: boolean
  onSave: (data: RotaPayload) => Promise<void>
  onCancel: () => void
}

function trimOrNull(value: string) {
  const trimmed = value.trim()
  return trimmed ? trimmed : null
}

function normalizePath(value: string) {
  return value.trim().replace(/^\/+/, '')
}

function isValidJson(value: string) {
  if (!value.trim()) return true

  try {
    JSON.parse(value)
    return true
  } catch {
    return false
  }
}

export function RotaRegistrationForm({ fluxosVersionados, initial, loadingVersions = false, onSave, onCancel }: Props) {
  const [nome, setNome] = useState(initial?.nome ?? '')
  const [descricao, setDescricao] = useState(initial?.descricao ?? '')
  const [caminho, setCaminho] = useState(initial?.caminho ?? '')
  const [method, setMethod] = useState<HttpMethod>(initial ? verboToHttpMethod(initial.metodo) : 'GET')
  const [pipelineVersaoId, setPipelineVersaoId] = useState<number | ''>(initial?.pipelineVersaoId ?? '')
  const [senhaAcesso, setSenhaAcesso] = useState(initial?.senhaAcesso ?? '')
  const [usarFluxoMaisAtual, setUsarFluxoMaisAtual] = useState(Boolean(initial?.usarFluxoMaisAtual))
  const [rateLimit, setRateLimit] = useState(Boolean(initial?.rateLimit))
  const [rateLimitRequests, setRateLimitRequests] = useState(String(initial?.rateLimitRequests ?? 10))
  const [rateLimitInterval, setRateLimitInterval] = useState(String(initial?.rateLimitInterval ?? 60))
  const [idempotencia, setIdempotencia] = useState(Boolean(initial?.idempotencia))
  const [jsonSchemaReq, setJsonSchemaReq] = useState(initial?.jsonSchemaReq ?? '')
  const [jsonSchemaResp, setJsonSchemaResp] = useState(initial?.jsonSchemaResp ?? '')
  const [saving, setSaving] = useState(false)
  const [errors, setErrors] = useState<Record<string, string>>({})

  const selectedVersion = useMemo(
    () => fluxosVersionados.find(version => version.id === pipelineVersaoId),
    [fluxosVersionados, pipelineVersaoId]
  )

  function validate() {
    const nextErrors: Record<string, string> = {}
    if (!nome.trim()) nextErrors.nome = 'Nome obrigatório'
    if (!normalizePath(caminho)) nextErrors.caminho = 'Caminho obrigatório'
    if (!pipelineVersaoId) nextErrors.pipelineVersaoId = 'Selecione uma versão de workflow'
    if (rateLimit && Number(rateLimitRequests) <= 0) nextErrors.rateLimitRequests = 'Informe um limite maior que zero'
    if (rateLimit && Number(rateLimitInterval) <= 0) nextErrors.rateLimitInterval = 'Informe um intervalo maior que zero'
    if (!isValidJson(jsonSchemaReq)) nextErrors.jsonSchemaReq = 'JSON inválido'
    if (!isValidJson(jsonSchemaResp)) nextErrors.jsonSchemaResp = 'JSON inválido'
    return nextErrors
  }

  async function handleSubmit(event: React.FormEvent) {
    event.preventDefault()

    const nextErrors = validate()
    if (Object.keys(nextErrors).length) {
      setErrors(nextErrors)
      return
    }

    setSaving(true)
    try {
      await onSave({
        nome: nome.trim(),
        descricao: trimOrNull(descricao),
        caminho: normalizePath(caminho),
        metodo: httpMethodToVerbo(method) as VerboHttp,
        pipelineVersaoId: pipelineVersaoId as number,
        senhaAcesso: trimOrNull(senhaAcesso),
        usarFluxoMaisAtual,
        rateLimit,
        rateLimitRequests: rateLimit ? Number(rateLimitRequests) : 0,
        rateLimitInterval: Number(rateLimitInterval) || 60,
        idempotencia,
        jsonSchemaReq: trimOrNull(jsonSchemaReq),
        jsonSchemaResp: trimOrNull(jsonSchemaResp),
      })
    } finally {
      setSaving(false)
    }
  }

  return (
    <motion.form
      initial={{ opacity: 0, x: 16 }}
      animate={{ opacity: 1, x: 0 }}
      transition={{ duration: 0.2 }}
      onSubmit={handleSubmit}
      className="flex min-h-full flex-col gap-5 p-6"
    >
      <div className="flex items-center justify-between border-b border-border pb-4">
        <div>
          <h2 className="text-sm font-semibold">{initial ? 'Editar Rota' : 'Nova Rota'}</h2>
          <p className="mt-0.5 text-xs text-muted-foreground">Associe um caminho público a uma versão de workflow.</p>
        </div>
        <button type="button" onClick={onCancel} className="text-muted-foreground transition-colors hover:text-foreground">
          <XIcon size={16} />
        </button>
      </div>

      <div className="grid grid-cols-1 gap-4 xl:grid-cols-2">
        <Field label="Nome" required error={errors.nome}>
          <Input value={nome} onChange={event => setNome(event.target.value)} className="h-8 rounded-none text-xs" />
        </Field>

        <Field label="Workflow versionado" required error={errors.pipelineVersaoId}>
          <Select
            value={pipelineVersaoId ? String(pipelineVersaoId) : ''}
            onValueChange={value => setPipelineVersaoId(Number(value))}
            disabled={loadingVersions}
          >
            <SelectTrigger className="h-8 rounded-none text-xs">
              <SelectValue placeholder={loadingVersions ? 'Carregando versões...' : 'Selecione uma versão'} />
            </SelectTrigger>
            <SelectContent>
              {fluxosVersionados.map(version => (
                <SelectItem key={version.id} value={String(version.id)} className="text-xs">
                  {version.nome} · v{version.versao}{version.atual ? ' · atual' : ''}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </Field>
      </div>

      <Field label="Caminho" required error={errors.caminho}>
        <div className="flex items-center border border-input bg-background">
          <span className="border-r border-input bg-muted/40 px-2 py-2 font-mono text-[11px] text-muted-foreground">/</span>
          <Input
            value={caminho}
            onChange={event => setCaminho(event.target.value)}
            placeholder="minha/rota/{parametro}"
            className="h-8 flex-1 rounded-none border-0 font-mono text-xs focus-visible:ring-0"
          />
        </div>
      </Field>

      <div className="flex flex-col gap-1.5">
        <label className="text-xs font-medium">Método HTTP <span className="text-destructive">*</span></label>
        <div className="flex flex-wrap gap-2">
          {METHODS.map(item => (
            <button
              key={item}
              type="button"
              onClick={() => setMethod(item)}
              className={cn(
                'rounded-none border px-3 py-1.5 font-mono text-[11px] font-bold tracking-wider transition-all',
                method === item ? METHOD_CLASSES[item] : 'border-border text-muted-foreground hover:bg-muted/40'
              )}
            >
              {item}
            </button>
          ))}
        </div>
      </div>

      {selectedVersion && (
        <div className="border border-border bg-muted/30 px-3 py-2 text-xs text-muted-foreground">
          Pipeline #{selectedVersion.id} · Fluxo #{selectedVersion.fluxoId} · {selectedVersion.nome}_v{selectedVersion.versao}
        </div>
      )}

      <Field label="Descrição">
        <textarea
          value={descricao}
          onChange={event => setDescricao(event.target.value)}
          rows={3}
          className="w-full resize-none rounded-none border border-input bg-background px-3 py-2 text-xs outline-none transition-shadow focus:ring-1 focus:ring-ring"
        />
      </Field>

      <div className="grid grid-cols-1 gap-4 xl:grid-cols-2">
        <Field label="Senha de acesso">
          <Input
            value={senhaAcesso}
            onChange={event => setSenhaAcesso(event.target.value)}
            placeholder="Header X-Rota-Senha"
            className="h-8 rounded-none text-xs"
          />
        </Field>

        <div className="grid grid-cols-1 gap-2 sm:grid-cols-3">
          <Toggle checked={usarFluxoMaisAtual} onChange={setUsarFluxoMaisAtual} label="Fluxo mais atual" />
          <Toggle checked={rateLimit} onChange={setRateLimit} label="Rate limit" />
          <Toggle checked={idempotencia} onChange={setIdempotencia} label="Idempotência" />
        </div>
      </div>

      {rateLimit && (
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <Field label="Requisições" error={errors.rateLimitRequests}>
            <Input
              type="number"
              min={1}
              value={rateLimitRequests}
              onChange={event => setRateLimitRequests(event.target.value)}
              className="h-8 rounded-none text-xs"
            />
          </Field>
          <Field label="Intervalo em segundos" error={errors.rateLimitInterval}>
            <Input
              type="number"
              min={1}
              value={rateLimitInterval}
              onChange={event => setRateLimitInterval(event.target.value)}
              className="h-8 rounded-none text-xs"
            />
          </Field>
        </div>
      )}

      <div className="grid grid-cols-1 gap-4 xl:grid-cols-2">
        <Field label="Schema da requisição" error={errors.jsonSchemaReq}>
          <textarea
            value={jsonSchemaReq}
            onChange={event => setJsonSchemaReq(event.target.value)}
            rows={7}
            placeholder='{"type":"object"}'
            className="w-full resize-none rounded-none border border-input bg-background px-3 py-2 font-mono text-[11px] outline-none transition-shadow focus:ring-1 focus:ring-ring"
          />
        </Field>
        <Field label="Schema da resposta" error={errors.jsonSchemaResp}>
          <textarea
            value={jsonSchemaResp}
            onChange={event => setJsonSchemaResp(event.target.value)}
            rows={7}
            placeholder='{"type":"object"}'
            className="w-full resize-none rounded-none border border-input bg-background px-3 py-2 font-mono text-[11px] outline-none transition-shadow focus:ring-1 focus:ring-ring"
          />
        </Field>
      </div>

      <div className="mt-auto flex gap-2 border-t border-border pt-4">
        <Button type="submit" size="sm" disabled={saving} className="gap-1.5 rounded-none">
          <FloppyDiskIcon size={14} />
          {saving ? 'Salvando...' : 'Salvar Rota'}
        </Button>
        <Button type="button" size="sm" variant="outline" onClick={onCancel} className="rounded-none">
          Cancelar
        </Button>
      </div>
    </motion.form>
  )
}

function Field({ label, required, error, children }: { label: string; required?: boolean; error?: string; children: React.ReactNode }) {
  return (
    <div className="flex flex-col gap-1.5">
      <label className="text-xs font-medium">
        {label} {required && <span className="text-destructive">*</span>}
      </label>
      {children}
      {error && <p className="text-[11px] text-destructive">{error}</p>}
    </div>
  )
}

function Toggle({ checked, onChange, label }: { checked: boolean; onChange: (value: boolean) => void; label: string }) {
  return (
    <label className="flex min-h-8 cursor-pointer items-center gap-2 border border-border bg-white px-3 py-2 text-xs">
      <input
        type="checkbox"
        checked={checked}
        onChange={event => onChange(event.target.checked)}
        className="h-3.5 w-3.5 accent-primary"
      />
      <span>{label}</span>
    </label>
  )
}
