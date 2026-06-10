import type { Rota } from '@/lib/types'
import { client } from './client'

export type RotaPayload = Omit<Rota, 'id' | 'criadoEm' | 'ultimaAlteracao' | 'pipelineVersao'>

export async function getRotas(): Promise<Rota[]> {
  return client.get<Rota[]>('/Rota')
}

export async function createRota(data: RotaPayload): Promise<Rota> {
  return client.post<Rota>('/Rota', data)
}

export async function updateRota(id: number, data: RotaPayload): Promise<Rota> {
  return client.put<Rota>(`/Rota/${id}`, data)
}

export async function deleteRota(id: number): Promise<void> {
  return client.delete(`/Rota/${id}`)
}
