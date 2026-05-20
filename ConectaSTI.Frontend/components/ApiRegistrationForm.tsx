'use client'

import { useState } from 'react'
import { motion } from 'motion/react'
import type { Integracao } from '@/lib/types'
import { Input } from '@/components/ui/input'
import { Button } from '@/components/ui/button'
import { EyeIcon, EyeSlashIcon, FloppyDiskIcon, XIcon } from '@phosphor-icons/react'

interface ApiRegistrationFormProps {
  initial?: Integracao
  onSave: (data: Omit<Integracao, 'id' | 'criadoEm'>) => Promise<void>
  onCancel: () => void
}

export function ApiRegistrationForm({ initial, onSave, onCancel }: ApiRegistrationFormProps) {
  const [nome, setNome] = useState(initial?.nome ?? '')
  const [url, setUrl] = useState(initial?.url ?? '')
  const [token, setToken] = useState(initial?.token ?? '')
  const [descricao, setDescricao] = useState(initial?.descricao ?? '')
  const [showToken, setShowToken] = useState(false)
  const [saving, setSaving] = useState(false)
  const [errors, setErrors] = useState<Record<string, string>>({})

  function validate() {
    const e: Record<string, string> = {}
    if (!nome.trim()) e.nome = 'Nome obrigatório'
    if (!url.trim()) e.url = 'URL obrigatória'
    else {
      try { new URL(url) } catch { e.url = 'URL inválida' }
    }
    return e
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    const errs = validate()
    if (Object.keys(errs).length) { setErrors(errs); return }
    setSaving(true)
    try {
      await onSave({
        nome: nome.trim(),
        url: url.trim(),
        token: token.trim() || null,
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
          <h2 className="font-semibold text-sm">{initial ? 'Editar API' : 'Nova API'}</h2>
          <p className="text-xs text-muted-foreground mt-0.5">
            {initial ? 'Atualize os dados da API cadastrada.' : 'Preencha os dados para cadastrar uma nova API.'}
          </p>
        </div>
        <button type="button" onClick={onCancel} className="text-muted-foreground hover:text-foreground transition-colors">
          <XIcon size={16} />
        </button>
      </div>

      <Field label="Nome" error={errors.nome} required>
        <Input
          value={nome}
          onChange={e => setNome(e.target.value)}
          placeholder="Ex: ViaCEP, Stripe, Minha API..."
          className="font-mono text-xs rounded-none"
        />
      </Field>

      <Field label="URL Base" error={errors.url} required hint="Ex: https://api.exemplo.com/v1">
        <Input
          value={url}
          onChange={e => setUrl(e.target.value)}
          placeholder="https://api.exemplo.com"
          className="font-mono text-xs rounded-none"
        />
      </Field>

      <Field label="Token de Autenticação" hint="Opcional — Bearer token, API Key, etc.">
        <div className="relative">
          <Input
            type={showToken ? 'text' : 'password'}
            value={token ?? ''}
            onChange={e => setToken(e.target.value)}
            placeholder="sk-... / eyJ..."
            className="font-mono text-xs pr-9 rounded-none"
          />
          <button
            type="button"
            onClick={() => setShowToken(v => !v)}
            className="absolute right-2.5 top-1/2 -translate-y-1/2 text-muted-foreground hover:text-foreground transition-colors"
          >
            {showToken ? <EyeSlashIcon size={14} /> : <EyeIcon size={14} />}
          </button>
        </div>
      </Field>

      <Field label="Descrição" hint="Opcional">
        <textarea
          value={descricao ?? ''}
          onChange={e => setDescricao(e.target.value)}
          placeholder="Descreva o propósito desta API..."
          rows={3}
          className="w-full border border-input bg-background px-3 py-2 text-xs font-mono resize-none outline-none focus:ring-1 focus:ring-ring transition-shadow rounded-none"
        />
      </Field>

      <div className="mt-auto flex gap-2 pt-4 border-t">
        <Button type="submit" disabled={saving} size="sm" className="gap-1.5 rounded-none">
          <FloppyDiskIcon size={14} />
          {saving ? 'Salvando...' : 'Salvar API'}
        </Button>
        <Button type="button" variant="outline" size="sm" onClick={onCancel} className="rounded-none">
          Cancelar
        </Button>
      </div>
    </motion.form>
  )
}

function Field({ label, error, hint, required, children }: {
  label: string; error?: string; hint?: string; required?: boolean; children: React.ReactNode
}) {
  return (
    <div className="flex flex-col gap-1.5">
      <label className="text-xs font-medium text-foreground">
        {label} {required && <span className="text-destructive">*</span>}
      </label>
      {children}
      {hint && !error && <p className="text-[11px] text-muted-foreground">{hint}</p>}
      {error && <p className="text-[11px] text-destructive">{error}</p>}
    </div>
  )
}
