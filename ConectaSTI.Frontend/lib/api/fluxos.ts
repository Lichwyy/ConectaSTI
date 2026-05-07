import type { Fluxo } from '@/lib/types'
import { client } from './client'

export async function getFluxos(): Promise<Fluxo[]> {
  return client.get<Fluxo[]>('/Fluxo')
}

export async function getFluxo(id: number): Promise<Fluxo> {
  return client.get<Fluxo>(`/Fluxo/${id}`)
}

export async function createFluxo(nome: string): Promise<Fluxo> {
  return client.post<Fluxo>('/Fluxo', { nome })
}

export async function updateFluxo(id: number, data: { nome: string }): Promise<Fluxo> {
  return client.put<Fluxo>(`/Fluxo/${id}`, data)
}

export async function deleteFluxo(id: number): Promise<void> {
  return client.delete(`/Fluxo/${id}`)
}
