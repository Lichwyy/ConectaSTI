import type { Integracao } from '@/lib/types'
import { client } from './client'

export async function getIntegracoes(): Promise<Integracao[]> {
  return client.get<Integracao[]>('/Integracao')
}

export async function getIntegracao(id: number): Promise<Integracao> {
  return client.get<Integracao>(`/Integracao/${id}`)
}

export async function createIntegracao(data: Omit<Integracao, 'id' | 'criadoEm'>): Promise<Integracao> {
  return client.post<Integracao>('/Integracao', data)
}

export async function updateIntegracao(id: number, data: Omit<Integracao, 'id' | 'criadoEm'>): Promise<Integracao> {
  return client.put<Integracao>(`/Integracao/${id}`, data)
}

export async function deleteIntegracao(id: number): Promise<void> {
  return client.delete(`/Integracao/${id}`)
}
