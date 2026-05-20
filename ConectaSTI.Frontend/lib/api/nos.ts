import type { No } from '@/lib/types'
import { client } from './client'

export async function getNo(id: number): Promise<No> {
  return client.get<No>(`/No/${id}`)
}

export async function createNo(data: Omit<No, 'id'>): Promise<No> {
  return client.post<No>('/No', data)
}

export async function updateNo(id: number, data: Omit<No, 'id'>): Promise<No> {
  return client.put<No>(`/No/${id}`, data)
}

export async function deleteNo(id: number): Promise<void> {
  return client.delete(`/No/${id}`)
}

export async function testeRequest(noId: number): Promise<unknown> {
  return client.post<unknown>(`/testerequest/${noId}`, {})
}
