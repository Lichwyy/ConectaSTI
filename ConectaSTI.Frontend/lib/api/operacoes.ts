import type { Operacao } from '@/lib/types'
import { client } from './client'

export async function getOperacoes(): Promise<Operacao[]> {
  return client.get<Operacao[]>('/Operacao')
}
