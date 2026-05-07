import type { Operacao } from '@/lib/types'
import { client } from './client'

export async function createOperacao(data: Omit<Operacao, 'id'>): Promise<Operacao> {
  return client.post<Operacao>('/Operacao', data)
}

export async function updateOperacao(id: number, data: Omit<Operacao, 'id'>): Promise<Operacao> {
  return client.put<Operacao>(`/Operacao/${id}`, data)
}

export async function deleteOperacao(id: number): Promise<void> {
  return client.delete(`/Operacao/${id}`)
}
