import type { EndPoint } from '@/lib/types'
import { client } from './client'

export async function getEndPoints(integracaoId?: number): Promise<EndPoint[]> {
  const all = await client.get<EndPoint[]>('/EndPoint')
  return integracaoId != null ? all.filter(e => e.integracaoId === integracaoId) : all
}

export async function createEndPoint(data: Omit<EndPoint, 'id' | 'criadoEm'>): Promise<EndPoint> {
  return client.post<EndPoint>('/EndPoint', data)
}

export async function updateEndPoint(id: number, data: Omit<EndPoint, 'id' | 'criadoEm'>): Promise<EndPoint> {
  return client.put<EndPoint>(`/EndPoint/${id}`, data)
}

export async function deleteEndPoint(id: number): Promise<void> {
  return client.delete(`/EndPoint/${id}`)
}
