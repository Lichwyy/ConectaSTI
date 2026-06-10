import type { LogFluxo, LogOperacao } from '@/lib/types'
import { client } from './client'

export async function getLogsFluxo(): Promise<LogFluxo[]> {
  return client.get<LogFluxo[]>('/LogFluxo')
}

export async function getLogsOperacao(): Promise<LogOperacao[]> {
  return client.get<LogOperacao[]>('/LogOperacao')
}

