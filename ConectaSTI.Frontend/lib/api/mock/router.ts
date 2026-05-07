import { store } from './store'
import type { Fluxo } from '@/lib/types'

function delay(ms = 120) {
  return new Promise(r => setTimeout(r, ms))
}

// path format: /Entity or /Entity/123
function parsePath(path: string): [string, number | null] {
  const parts = path.replace(/^\//, '').split('/')
  const entity = parts[0]
  const idPart = parts[1]
  return [entity, idPart ? Number(idPart) : null]
}

export async function mockRequest<T>(method: string, path: string, body?: unknown): Promise<T> {
  await delay()

  const [entity, entityId] = parsePath(path)
  const m = method.toUpperCase()

  // ── Integracao ─────────────────────────────────────────────────────────────
  if (entity === 'Integracao') {
    if (m === 'GET' && entityId === null) return store.integracoes as T
    if (m === 'GET' && entityId !== null) {
      const item = store.integracoes.find(i => i.id === entityId)
      if (!item) throw new Error('Integração não encontrada')
      return item as T
    }
    if (m === 'POST') {
      const item = { ...(body as object), id: store.nextId(), criadoEm: new Date().toISOString() } as T
      store.integracoes.push(item as never)
      return item
    }
    if (m === 'PUT' && entityId !== null) {
      const idx = store.integracoes.findIndex(i => i.id === entityId)
      if (idx === -1) throw new Error('Integração não encontrada')
      store.integracoes[idx] = { ...store.integracoes[idx], ...(body as object) }
      return store.integracoes[idx] as T
    }
    if (m === 'DELETE' && entityId !== null) {
      store.integracoes = store.integracoes.filter(i => i.id !== entityId)
      return undefined as T
    }
  }

  // ── EndPoint ───────────────────────────────────────────────────────────────
  if (entity === 'EndPoint') {
    if (m === 'GET') return store.endpoints as T
    if (m === 'POST') {
      const item = { ...(body as object), id: store.nextId(), criadoEm: new Date().toISOString() } as T
      store.endpoints.push(item as never)
      return item
    }
    if (m === 'PUT' && entityId !== null) {
      const idx = store.endpoints.findIndex(e => e.id === entityId)
      if (idx === -1) throw new Error('Endpoint não encontrado')
      store.endpoints[idx] = { ...store.endpoints[idx], ...(body as object) }
      return store.endpoints[idx] as T
    }
    if (m === 'DELETE' && entityId !== null) {
      store.endpoints = store.endpoints.filter(e => e.id !== entityId)
      return undefined as T
    }
  }

  // ── Funcao ─────────────────────────────────────────────────────────────────
  if (entity === 'Funcao') {
    if (m === 'GET' && entityId === null) return store.funcoes as T
    if (m === 'GET' && entityId !== null) {
      const item = store.funcoes.find(f => f.id === entityId)
      if (!item) throw new Error('Função não encontrada')
      return item as T
    }
    if (m === 'POST') {
      const item = { ...(body as object), id: store.nextId(), criadoEm: new Date().toISOString() } as T
      store.funcoes.push(item as never)
      return item
    }
    if (m === 'PUT' && entityId !== null) {
      const idx = store.funcoes.findIndex(f => f.id === entityId)
      if (idx === -1) throw new Error('Função não encontrada')
      store.funcoes[idx] = { ...store.funcoes[idx], ...(body as object) }
      return store.funcoes[idx] as T
    }
    if (m === 'DELETE' && entityId !== null) {
      store.funcoes = store.funcoes.filter(f => f.id !== entityId)
      return undefined as T
    }
  }

  // ── Fluxo ──────────────────────────────────────────────────────────────────
  if (entity === 'Fluxo') {
    if (m === 'GET' && entityId === null) return store.fluxos as T
    if (m === 'GET' && entityId !== null) {
      const fluxo = store.fluxos.find(f => f.id === entityId)
      if (!fluxo) throw new Error('Fluxo não encontrado')
      const operacoes = store.operacoes.filter(op => op.fluxoId === entityId)
      return { ...fluxo, operacoes } as T
    }
    if (m === 'POST') {
      const b = body as { nome: string }
      const item: Fluxo = { id: store.nextId(), nome: b.nome, operacoes: [], criadoEm: new Date().toISOString() }
      store.fluxos.push(item)
      return item as T
    }
    if (m === 'PUT' && entityId !== null) {
      const idx = store.fluxos.findIndex(f => f.id === entityId)
      if (idx === -1) throw new Error('Fluxo não encontrado')
      store.fluxos[idx] = { ...store.fluxos[idx], ...(body as object) }
      return store.fluxos[idx] as T
    }
    if (m === 'DELETE' && entityId !== null) {
      store.fluxos = store.fluxos.filter(f => f.id !== entityId)
      store.operacoes = store.operacoes.filter(op => op.fluxoId !== entityId)
      return undefined as T
    }
  }

  // ── No ─────────────────────────────────────────────────────────────────────
  if (entity === 'No') {
    if (m === 'GET' && entityId !== null) {
      const item = store.nos.find(n => n.id === entityId)
      if (!item) throw new Error('Nó não encontrado')
      return item as T
    }
    if (m === 'POST') {
      const item = { ...(body as object), id: store.nextId() } as T
      store.nos.push(item as never)
      return item
    }
    if (m === 'PUT' && entityId !== null) {
      const idx = store.nos.findIndex(n => n.id === entityId)
      if (idx === -1) throw new Error('Nó não encontrado')
      store.nos[idx] = { ...store.nos[idx], ...(body as object) }
      return store.nos[idx] as T
    }
    if (m === 'DELETE' && entityId !== null) {
      store.nos = store.nos.filter(n => n.id !== entityId)
      return undefined as T
    }
  }

  // ── Operacao ───────────────────────────────────────────────────────────────
  if (entity === 'Operacao') {
    if (m === 'POST') {
      const item = { ...(body as object), id: store.nextId() } as T
      store.operacoes.push(item as never)
      return item
    }
    if (m === 'PUT' && entityId !== null) {
      const idx = store.operacoes.findIndex(op => op.id === entityId)
      if (idx === -1) throw new Error('Operação não encontrada')
      store.operacoes[idx] = { ...store.operacoes[idx], ...(body as object) }
      return store.operacoes[idx] as T
    }
    if (m === 'DELETE' && entityId !== null) {
      store.operacoes = store.operacoes.filter(op => op.id !== entityId)
      return undefined as T
    }
  }

  // ── Test routes ────────────────────────────────────────────────────────────
  if (entity === 'testerequest') {
    return { status: 200, message: '[mock] Requisição simulada com sucesso.', noId: entityId } as T
  }

  if (entity === 'testefunction') {
    return { status: 200, message: '[mock] Função executada com sucesso.', resultado: body } as T
  }

  throw new Error(`[mock] Rota não implementada: ${m} /${entity}`)
}
