import type { FluxoVersionado } from '@/lib/types'
import { client } from './client'

export async function getFluxosVersionados(): Promise<FluxoVersionado[]> {
  return client.get<FluxoVersionado[]>('/FluxoVersionado')
}
