import type { Funcao } from '@/lib/types'
import { client } from './client'

export async function getFuncoes(): Promise<Funcao[]> {
  return client.get<Funcao[]>('/Funcao')
}

export async function getFuncao(id: number): Promise<Funcao> {
  return client.get<Funcao>(`/Funcao/${id}`)
}

export async function createFuncao(data: Omit<Funcao, 'id' | 'criadoEm'>): Promise<Funcao> {
  return client.post<Funcao>('/Funcao', data)
}

export async function updateFuncao(id: number, data: Omit<Funcao, 'id' | 'criadoEm'>): Promise<Funcao> {
  return client.put<Funcao>(`/Funcao/${id}`, data)
}

export async function deleteFuncao(id: number): Promise<void> {
  return client.delete(`/Funcao/${id}`)
}

export async function testeFunction(id: number, body: unknown): Promise<unknown> {
  return client.post<unknown>(`/testefunction/${id}`, body)
}
