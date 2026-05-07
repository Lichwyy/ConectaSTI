'use client'

import { useState } from 'react'
import { motion, AnimatePresence } from 'motion/react'
import { useRouter } from 'next/navigation'
import { useFluxos } from '@/hooks/useFluxos'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import {
  PlusIcon,
  GitBranchIcon,
  TrashIcon,
  ArrowRightIcon,
  CircleNotchIcon,
} from '@phosphor-icons/react'

export default function WorkflowsPage() {
  const router = useRouter()
  const { fluxos, loading, createFluxo, deleteFluxo } = useFluxos()
  const [creating, setCreating] = useState(false)
  const [newName, setNewName] = useState('')
  const [showInput, setShowInput] = useState(false)

  async function handleCreate(e: React.FormEvent) {
    e.preventDefault()
    if (!newName.trim()) return
    setCreating(true)
    try {
      const wf = await createFluxo(newName.trim())
      router.push(`/workflows/${wf.id}`)
    } finally {
      setCreating(false)
    }
  }

  async function handleDelete(id: number, nome: string) {
    if (!confirm(`Remover o workflow "${nome}"?`)) return
    await deleteFluxo(id)
  }

  return (
    <div className="flex-1 overflow-y-auto p-8">
      <div className="max-w-4xl mx-auto">
        <div className="flex items-center justify-between mb-8">
          <div>
            <h1 className="text-xl font-semibold">Workflows</h1>
            <p className="text-xs text-muted-foreground mt-1">
              Crie automações conectando suas APIs em blocos visuais.
            </p>
          </div>

          <AnimatePresence mode="wait">
            {showInput ? (
              <motion.form
                key="input"
                initial={{ opacity: 0, width: 120 }}
                animate={{ opacity: 1, width: 320 }}
                exit={{ opacity: 0, width: 120 }}
                onSubmit={handleCreate}
                className="flex gap-2"
              >
                <Input
                  autoFocus
                  value={newName}
                  onChange={e => setNewName(e.target.value)}
                  placeholder="Nome do workflow..."
                  className="rounded-none text-xs h-8"
                  disabled={creating}
                />
                <Button type="submit" size="sm" disabled={creating || !newName.trim()} className="rounded-none h-8 gap-1">
                  {creating ? <CircleNotchIcon size={12} className="animate-spin" /> : <ArrowRightIcon size={12} />}
                  {creating ? 'Criando...' : 'Criar'}
                </Button>
                <Button
                  type="button"
                  size="sm"
                  variant="outline"
                  className="rounded-none h-8"
                  onClick={() => { setShowInput(false); setNewName('') }}
                >
                  ×
                </Button>
              </motion.form>
            ) : (
              <motion.div key="btn" initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }}>
                <Button
                  size="sm"
                  className="gap-1.5 rounded-none"
                  onClick={() => setShowInput(true)}
                >
                  <PlusIcon size={14} />
                  Novo Workflow
                </Button>
              </motion.div>
            )}
          </AnimatePresence>
        </div>

        {loading ? (
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
            {[1, 2, 3].map(i => (
              <div key={i} className="h-32 bg-muted/40 animate-pulse" />
            ))}
          </div>
        ) : fluxos.length === 0 ? (
          <motion.div
            initial={{ opacity: 0, y: 16 }}
            animate={{ opacity: 1, y: 0 }}
            className="border border-dashed border-border p-16 text-center"
          >
            <motion.div
              animate={{ scale: [1, 1.05, 1] }}
              transition={{ repeat: Infinity, duration: 3 }}
              className="text-muted-foreground/30 mx-auto w-fit"
            >
              <GitBranchIcon size={52} weight="thin" />
            </motion.div>
            <p className="text-sm font-medium text-muted-foreground mt-4">Nenhum workflow criado</p>
            <p className="text-xs text-muted-foreground/60 mt-1">
              Crie seu primeiro workflow para começar a conectar APIs.
            </p>
            <Button
              size="sm"
              className="gap-1.5 rounded-none mt-5"
              onClick={() => setShowInput(true)}
            >
              <PlusIcon size={12} />
              Criar Workflow
            </Button>
          </motion.div>
        ) : (
          <motion.div
            className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4"
            initial="hidden"
            animate="visible"
            variants={{ visible: { transition: { staggerChildren: 0.07 } } }}
          >
            {fluxos.map(wf => (
              <motion.div
                key={wf.id}
                variants={{ hidden: { opacity: 0, y: 12 }, visible: { opacity: 1, y: 0 } }}
                whileHover={{ y: -2 }}
                transition={{ duration: 0.15 }}
              >
                <div className="group relative border border-border bg-white/80 p-5 hover:border-primary/40 transition-all duration-200">
                  <div className="absolute inset-0 opacity-[0.03] pointer-events-none"
                    style={{ backgroundImage: 'radial-gradient(circle, #000 1px, transparent 1px)', backgroundSize: '12px 12px' }}
                  />

                  <div className="relative">
                    <div className="flex items-start justify-between gap-2 mb-3">
                      <div className="p-2 bg-primary/5 border border-primary/10">
                        <GitBranchIcon size={16} className="text-primary" />
                      </div>
                      <button
                        onClick={() => handleDelete(wf.id, wf.nome)}
                        className="opacity-0 group-hover:opacity-100 text-muted-foreground hover:text-destructive transition-all p-1"
                        title="Remover"
                      >
                        <TrashIcon size={13} />
                      </button>
                    </div>

                    <h3 className="font-medium text-sm mb-1 line-clamp-1">{wf.nome}</h3>
                    <div className="flex items-center gap-3 text-[11px] text-muted-foreground">
                      <span>{(wf.operacoes ?? []).length} nó{(wf.operacoes ?? []).length !== 1 ? 's' : ''}</span>
                      <span>·</span>
                      <span>{wf.criadoEm ? new Date(wf.criadoEm).toLocaleDateString('pt-BR') : '—'}</span>
                    </div>

                    <Button
                      size="sm"
                      variant="outline"
                      className="w-full mt-4 rounded-none text-xs h-7 gap-1.5 group-hover:border-primary/40 group-hover:text-primary transition-colors"
                      onClick={() => router.push(`/workflows/${wf.id}`)}
                    >
                      Abrir Editor
                      <ArrowRightIcon size={11} />
                    </Button>
                  </div>
                </div>
              </motion.div>
            ))}
          </motion.div>
        )}
      </div>
    </div>
  )
}
