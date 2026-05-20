import type { Edge } from '@xyflow/react'

export type HttpMethod = 'GET' | 'POST' | 'PUT' | 'DELETE' | 'PATCH'
export type VerboHttp = 1 | 2 | 3 | 4 | 5
export type TipoNo = 1 | 2 | 3 | 4
export type TipoErro = 1 | 2 | 3
export type BackoffType = 1 | 2 | 3

export interface Integracao {
  id: number
  nome: string
  url: string
  token?: string | null
  descricao?: string | null
  criadoEm?: string | null
}

export interface EndPoint {
  id: number
  recurso: string
  integracaoId: number
  verbo: VerboHttp
  descricao?: string | null
  criadoEm?: string | null
}

export interface Funcao {
  id: number
  nome: string
  corpoDaFuncao: string
  parametro?: string | null
  criadoEm?: string | null
}

export interface No {
  id: number
  tipo: TipoNo
  body?: string | null
  headers?: string | null
  funcaoId?: number | null
  endPointId?: number | null
  chaveValor?: string | null
}

export interface Operacao {
  id: number
  ordem: number
  noId: number
  fluxoId: number
  repetir?: boolean
  erro: TipoErro
  maxRetries?: number
  backoffType: BackoffType
  backoffDelay?: number
  backoffMultiplier?: number
  timeout?: number
}

export interface Fluxo {
  id: number
  nome: string
  operacoes?: Operacao[] | null
  criadoEm?: string | null
}

export interface WorkflowNodeData extends Record<string, unknown> {
  noId: number
  operacaoId?: number
  tipo: TipoNo
  label: string
  body?: string | null
  headers?: string | null
  // Tipo 1 - Requisicao
  endpointId?: number
  integracaoId?: number
  verbo?: VerboHttp
  integracaoNome?: string
  recurso?: string
  // Tipo 2 - FuncaoJS
  funcaoId?: number
  funcaoNome?: string
  // Tipo 3|4 - Storage
  chaveValor?: string
  // Operacao config
  ordem?: number
  erro: TipoErro
  maxRetries?: number
  backoffType: BackoffType
  backoffDelay?: number
  backoffMultiplier?: number
  timeout?: number
}

export interface CanvasState {
  positions: Record<string, { x: number; y: number }>
  edges: Edge[]
}

export const VERBO_METHOD_MAP: Record<VerboHttp, HttpMethod> = {
  1: 'GET',
  2: 'POST',
  3: 'PUT',
  4: 'DELETE',
  5: 'PATCH',
}

export const METHOD_VERBO_MAP: Record<HttpMethod, VerboHttp> = {
  GET: 1,
  POST: 2,
  PUT: 3,
  DELETE: 4,
  PATCH: 5,
}

export function verboToHttpMethod(v: VerboHttp): HttpMethod {
  return VERBO_METHOD_MAP[v]
}

export function httpMethodToVerbo(m: HttpMethod): VerboHttp {
  return METHOD_VERBO_MAP[m]
}

export const METHOD_COLORS: Record<HttpMethod, { bg: string; text: string; border: string }> = {
  GET:    { bg: 'bg-emerald-100',  text: 'text-emerald-700',  border: 'border-emerald-300' },
  POST:   { bg: 'bg-indigo-100',   text: 'text-indigo-700',   border: 'border-indigo-300'  },
  PUT:    { bg: 'bg-amber-100',    text: 'text-amber-700',    border: 'border-amber-300'   },
  DELETE: { bg: 'bg-red-100',      text: 'text-red-700',      border: 'border-red-300'     },
  PATCH:  { bg: 'bg-violet-100',   text: 'text-violet-700',   border: 'border-violet-300'  },
}

export const METHOD_NODE_COLORS: Record<HttpMethod, string> = {
  GET:    'border-l-emerald-500',
  POST:   'border-l-indigo-500',
  PUT:    'border-l-amber-500',
  DELETE: 'border-l-red-500',
  PATCH:  'border-l-violet-500',
}

export const DEFAULT_OPERACAO: Pick<WorkflowNodeData, 'erro' | 'backoffType' | 'maxRetries' | 'backoffDelay' | 'backoffMultiplier' | 'timeout'> = {
  erro: 1,
  backoffType: 1,
  maxRetries: 0,
  backoffDelay: 0,
  backoffMultiplier: 1,
  timeout: 30000,
}
