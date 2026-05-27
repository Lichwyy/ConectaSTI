import type { Fluxo, Operacao } from '@/lib/types'
import { client } from './client'

export type FluxoPayload = {
  nome: string
  operacoes?: Partial<Operacao>[]
}

export type FluxoExecutionResult = {
  status?: number
  resposta?: unknown
  respostaBody?: string | null
  retorno?: { mensagem: string; erro: boolean }[]
  sucesso?: boolean
}

export async function getFluxos(): Promise<Fluxo[]> {
  return client.get<Fluxo[]>('/Fluxo')
}

export async function getFluxo(id: number): Promise<Fluxo> {
  return client.get<Fluxo>(`/Fluxo/${id}`)
}

export async function createFluxo(data: FluxoPayload): Promise<Fluxo> {
  return client.post<Fluxo>('/Fluxo', data)
}

export async function updateFluxo(id: number, data: FluxoPayload): Promise<Fluxo> {
  return client.put<Fluxo>(`/Fluxo/${id}`, data)
}

export async function deleteFluxo(id: number): Promise<void> {
  return client.delete(`/Fluxo/${id}`)
}

export async function executarFluxo(id: number): Promise<FluxoExecutionResult> {
  return client.post<FluxoExecutionResult>(`/_root/executarfluxo/${id}`, {})
}
