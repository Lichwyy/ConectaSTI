'use client'

import { useState, useMemo } from 'react'
import { motion, AnimatePresence } from 'motion/react'
import { useFuncoes } from '@/hooks/useFuncoes'
import { testeFunction } from '@/lib/api/funcoes'
import type { Funcao } from '@/lib/types'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import {
  PlusIcon,
  MagnifyingGlassIcon,
  CodeIcon,
  PencilSimpleIcon,
  TrashIcon,
  PlayIcon,
  CircleNotchIcon,
  XIcon,
  FloppyDiskIcon,
} from '@phosphor-icons/react'

type PanelMode = 'empty' | 'new' | 'edit' | 'detail'

export default function FuncoesPage() {
  const { funcoes, loading, createFuncao, updateFuncao, deleteFuncao } = useFuncoes()

  const [search, setSearch] = useState('')
  const [selectedId, setSelectedId] = useState<number | null>(null)
  const [panelMode, setPanelMode] = useState<PanelMode>('empty')

  const filtered = useMemo(
    () => funcoes.filter(f => f.nome.toLowerCase().includes(search.toLowerCase())),
    [funcoes, search]
  )

  const selected = funcoes.find(f => f.id === selectedId) ?? null

  async function handleDelete(id: number) {
    if (!confirm('Remover esta função?')) return
    await deleteFuncao(id)
    setSelectedId(null)
    setPanelMode('empty')
  }

  return (
    <div className="flex h-screen w-full overflow-hidden">
      {/* Left panel */}
      <div className="flex flex-col w-72 shrink-0 border-r border-border bg-white/60 backdrop-blur-sm">
        <div className="p-4 border-b border-border">
          <div className="flex items-center justify-between mb-3">
            <h1 className="text-sm font-semibold">Funções JS</h1>
            <Button
              size="sm"
              variant="outline"
              className="h-7 gap-1 text-xs rounded-none"
              onClick={() => { setSelectedId(null); setPanelMode('new') }}
            >
              <PlusIcon size={12} />
              Nova
            </Button>
          </div>
          <div className="relative">
            <MagnifyingGlassIcon size={13} className="absolute left-2.5 top-1/2 -translate-y-1/2 text-muted-foreground" />
            <Input
              value={search}
              onChange={e => setSearch(e.target.value)}
              placeholder="Buscar funções..."
              className="pl-7 h-7 text-xs rounded-none"
            />
          </div>
        </div>

        <div className="flex-1 overflow-y-auto">
          {loading ? (
            <div className="p-4 space-y-2">
              {[1, 2, 3].map(i => <div key={i} className="h-14 bg-muted/40 animate-pulse" />)}
            </div>
          ) : filtered.length === 0 ? (
            <div className="p-6 text-center text-xs text-muted-foreground">
              {search ? 'Nenhuma função encontrada.' : 'Nenhuma função cadastrada.'}
            </div>
          ) : (
            <motion.div
              className="p-2 space-y-1.5"
              initial="hidden"
              animate="visible"
              variants={{ visible: { transition: { staggerChildren: 0.05 } } }}
            >
              {filtered.map(f => (
                <motion.div
                  key={f.id}
                  variants={{ hidden: { opacity: 0, y: 8 }, visible: { opacity: 1, y: 0 } }}
                >
                  <button
                    onClick={() => { setSelectedId(f.id); setPanelMode('detail') }}
                    className={`w-full text-left border-l-4 border-l-transparent bg-white border border-border rounded-none p-3 transition-all duration-150 hover:border-l-violet-500 hover:bg-muted/40 ${selectedId === f.id ? 'border-l-violet-500 bg-muted/40 ring-1 ring-violet-500/20' : ''}`}
                  >
                    <div className="flex items-center gap-2">
                      <CodeIcon size={13} className="text-violet-500 shrink-0" />
                      <span className="font-medium text-sm truncate">{f.nome}</span>
                    </div>
                    {f.parametro && (
                      <p className="text-[11px] text-muted-foreground mt-1 truncate font-mono">
                        param: {f.parametro}
                      </p>
                    )}
                  </button>
                </motion.div>
              ))}
            </motion.div>
          )}
        </div>
      </div>

      {/* Right panel */}
      <div className="flex-1 overflow-y-auto">
        <AnimatePresence mode="wait">
          {panelMode === 'empty' && (
            <motion.div
              key="empty"
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
              exit={{ opacity: 0 }}
              className="flex h-full flex-col items-center justify-center gap-4 text-center p-8"
            >
              <motion.div
                animate={{ y: [0, -6, 0] }}
                transition={{ repeat: Infinity, duration: 3, ease: 'easeInOut' }}
                className="text-muted-foreground/40"
              >
                <CodeIcon size={56} weight="thin" />
              </motion.div>
              <div>
                <p className="text-sm font-medium text-muted-foreground">Nenhuma função selecionada</p>
                <p className="text-xs text-muted-foreground/60 mt-1">
                  Crie funções JavaScript para usar nos seus workflows.
                </p>
              </div>
              <Button
                size="sm"
                variant="outline"
                className="gap-1.5 rounded-none text-xs mt-2"
                onClick={() => setPanelMode('new')}
              >
                <PlusIcon size={12} />
                Criar primeira função
              </Button>
            </motion.div>
          )}

          {(panelMode === 'new' || panelMode === 'edit') && (
            <motion.div key="form" initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }}>
              <FuncaoForm
                initial={panelMode === 'edit' ? selected ?? undefined : undefined}
                onSave={async data => {
                  if (panelMode === 'edit' && selectedId) {
                    await updateFuncao(selectedId, data)
                    setPanelMode('detail')
                  } else {
                    const f = await createFuncao(data)
                    setSelectedId(f.id)
                    setPanelMode('detail')
                  }
                }}
                onCancel={() => setPanelMode(selected ? 'detail' : 'empty')}
              />
            </motion.div>
          )}

          {panelMode === 'detail' && selected && (
            <motion.div
              key={`detail-${selected.id}`}
              initial={{ opacity: 0, x: 12 }}
              animate={{ opacity: 1, x: 0 }}
              exit={{ opacity: 0 }}
              transition={{ duration: 0.2 }}
              className="p-6 max-w-3xl"
            >
              <div className="flex items-start justify-between mb-6 pb-4 border-b">
                <div className="flex items-center gap-2">
                  <CodeIcon size={18} className="text-violet-500" />
                  <h2 className="font-semibold text-base">{selected.nome}</h2>
                </div>
                <div className="flex gap-2">
                  <Button
                    size="sm"
                    variant="outline"
                    className="gap-1.5 rounded-none text-xs h-7"
                    onClick={() => setPanelMode('edit')}
                  >
                    <PencilSimpleIcon size={12} />
                    Editar
                  </Button>
                  <Button
                    size="sm"
                    variant="outline"
                    className="gap-1.5 rounded-none text-xs h-7 text-destructive hover:text-destructive"
                    onClick={() => handleDelete(selected.id)}
                  >
                    <TrashIcon size={12} />
                  </Button>
                </div>
              </div>

              {selected.parametro && (
                <div className="mb-4">
                  <p className="text-[10px] uppercase tracking-wider text-muted-foreground font-medium mb-1">Parâmetro</p>
                  <p className="text-xs font-mono text-muted-foreground">{selected.parametro}</p>
                </div>
              )}

              <div className="mb-6">
                <p className="text-[10px] uppercase tracking-wider text-muted-foreground font-medium mb-2">Código</p>
                <pre className="bg-muted/30 border border-border p-4 text-xs font-mono overflow-x-auto whitespace-pre-wrap leading-relaxed">
                  {selected.corpoDaFuncao}
                </pre>
              </div>

              <FuncaoTester funcaoId={selected.id} />
            </motion.div>
          )}
        </AnimatePresence>
      </div>
    </div>
  )
}

function FuncaoForm({
  initial,
  onSave,
  onCancel,
}: {
  initial?: Funcao
  onSave: (data: Omit<Funcao, 'id' | 'criadoEm'>) => Promise<void>
  onCancel: () => void
}) {
  const [nome, setNome] = useState(initial?.nome ?? '')
  const [corpo, setCorpo] = useState(initial?.corpoDaFuncao ?? '')
  const [parametro, setParametro] = useState(initial?.parametro ?? '')
  const [saving, setSaving] = useState(false)
  const [errors, setErrors] = useState<Record<string, string>>({})

  function validate() {
    const e: Record<string, string> = {}
    if (!nome.trim()) e.nome = 'Nome obrigatório'
    if (!corpo.trim()) e.corpo = 'Código obrigatório'
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
        corpoDaFuncao: corpo,
        parametro: parametro.trim() || null,
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
          <h2 className="font-semibold text-sm">{initial ? 'Editar Função' : 'Nova Função JS'}</h2>
          <p className="text-xs text-muted-foreground mt-0.5">Defina uma função JavaScript para usar nos workflows.</p>
        </div>
        <button type="button" onClick={onCancel} className="text-muted-foreground hover:text-foreground">
          <XIcon size={16} />
        </button>
      </div>

      <div className="flex flex-col gap-1.5">
        <label className="text-xs font-medium">Nome <span className="text-destructive">*</span></label>
        <Input
          value={nome}
          onChange={e => setNome(e.target.value)}
          placeholder="Ex: formatarData, calcularTotal..."
          className="font-mono text-xs rounded-none"
        />
        {errors.nome && <p className="text-[11px] text-destructive">{errors.nome}</p>}
      </div>

      <div className="flex flex-col gap-1.5">
        <label className="text-xs font-medium">Parâmetro <span className="text-muted-foreground font-normal">(opcional)</span></label>
        <Input
          value={parametro ?? ''}
          onChange={e => setParametro(e.target.value)}
          placeholder="Descreva o input esperado..."
          className="text-xs rounded-none"
        />
      </div>

      <div className="flex flex-col gap-1.5 flex-1">
        <label className="text-xs font-medium">Código <span className="text-destructive">*</span></label>
        <textarea
          value={corpo}
          onChange={e => setCorpo(e.target.value)}
          placeholder={'// Escreva o corpo da função JavaScript\nreturn input * 2;'}
          className="flex-1 min-h-[240px] w-full border border-input bg-background px-3 py-2 text-xs font-mono resize-none outline-none focus:ring-1 focus:ring-ring transition-shadow rounded-none leading-relaxed"
        />
        {errors.corpo && <p className="text-[11px] text-destructive">{errors.corpo}</p>}
      </div>

      <div className="flex gap-2 pt-4 border-t">
        <Button type="submit" disabled={saving} size="sm" className="gap-1.5 rounded-none">
          <FloppyDiskIcon size={14} />
          {saving ? 'Salvando...' : 'Salvar Função'}
        </Button>
        <Button type="button" variant="outline" size="sm" onClick={onCancel} className="rounded-none">
          Cancelar
        </Button>
      </div>
    </motion.form>
  )
}

function FuncaoTester({ funcaoId }: { funcaoId: number }) {
  const [input, setInput] = useState('')
  const [result, setResult] = useState<string | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [running, setRunning] = useState(false)

  async function handleTest() {
    setRunning(true)
    setResult(null)
    setError(null)
    try {
      let body: unknown = input.trim()
      if (body) {
        try { body = JSON.parse(input) } catch { /* use raw string */ }
      }
      const res = await testeFunction(funcaoId, body)
      setResult(JSON.stringify(res, null, 2))
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Erro ao executar função')
    } finally {
      setRunning(false)
    }
  }

  return (
    <div className="border-t pt-5">
      <p className="text-[10px] uppercase tracking-wider text-muted-foreground font-medium mb-3">Testar função</p>
      <div className="space-y-3">
        <div className="flex flex-col gap-1.5">
          <label className="text-xs text-muted-foreground">Input (JSON)</label>
          <textarea
            value={input}
            onChange={e => setInput(e.target.value)}
            placeholder='{"valor": 42}'
            rows={3}
            className="w-full border border-input bg-background px-3 py-2 text-xs font-mono resize-none outline-none focus:ring-1 focus:ring-ring rounded-none"
          />
        </div>
        <Button
          size="sm"
          variant="outline"
          className="gap-1.5 rounded-none text-xs h-7"
          onClick={handleTest}
          disabled={running}
        >
          {running ? <CircleNotchIcon size={12} className="animate-spin" /> : <PlayIcon size={12} />}
          {running ? 'Executando...' : 'Executar'}
        </Button>
        {result && (
          <div className="bg-emerald-50 border border-emerald-200 p-3">
            <p className="text-[10px] text-emerald-700 font-medium mb-1">Resultado</p>
            <pre className="text-xs font-mono text-emerald-800 overflow-x-auto">{result}</pre>
          </div>
        )}
        {error && (
          <div className="bg-red-50 border border-red-200 p-3">
            <p className="text-[10px] text-red-700 font-medium mb-1">Erro</p>
            <p className="text-xs font-mono text-red-800">{error}</p>
          </div>
        )}
      </div>
    </div>
  )
}
