import type { Integracao, EndPoint, Funcao, No, Operacao, Fluxo } from '@/lib/types'

let _id = 200
function nextId() { return _id++ }

// ── Seed data ────────────────────────────────────────────────────────────────

const integracoes: Integracao[] = [
  { id: 1, nome: 'ViaCEP', url: 'https://viacep.com.br/ws', descricao: 'Consulta de CEPs brasileiros.', criadoEm: '2025-01-10T10:00:00Z' },
  { id: 2, nome: 'JSONPlaceholder', url: 'https://jsonplaceholder.typicode.com', token: 'mock-token-abc', descricao: 'API de testes com dados fictícios.', criadoEm: '2025-01-12T14:30:00Z' },
  { id: 3, nome: 'OpenWeather', url: 'https://api.openweathermap.org/data/2.5', token: 'weather-key-xyz', descricao: 'Dados meteorológicos em tempo real.', criadoEm: '2025-02-01T08:00:00Z' },
]

const endpoints: EndPoint[] = [
  { id: 1, integracaoId: 1, recurso: '/{cep}/json', verbo: 1, descricao: 'Retorna endereço por CEP.', criadoEm: '2025-01-10T10:05:00Z' },
  { id: 2, integracaoId: 2, recurso: '/posts', verbo: 1, descricao: 'Lista todos os posts.', criadoEm: '2025-01-12T14:35:00Z' },
  { id: 3, integracaoId: 2, recurso: '/posts', verbo: 2, descricao: 'Cria um novo post.', criadoEm: '2025-01-12T14:36:00Z' },
  { id: 4, integracaoId: 2, recurso: '/users/{id}', verbo: 1, descricao: 'Retorna usuário por ID.', criadoEm: '2025-01-12T14:37:00Z' },
  { id: 5, integracaoId: 2, recurso: '/posts/{id}', verbo: 3, descricao: 'Atualiza um post.', criadoEm: '2025-01-12T14:38:00Z' },
  { id: 6, integracaoId: 2, recurso: '/posts/{id}', verbo: 4, descricao: 'Remove um post.', criadoEm: '2025-01-12T14:39:00Z' },
  { id: 7, integracaoId: 3, recurso: '/weather', verbo: 1, descricao: 'Clima atual por cidade.', criadoEm: '2025-02-01T08:05:00Z' },
  { id: 8, integracaoId: 3, recurso: '/forecast', verbo: 1, descricao: 'Previsão dos próximos 5 dias.', criadoEm: '2025-02-01T08:06:00Z' },
]

const funcoes: Funcao[] = [
  { id: 1, nome: 'formatarData', corpoDaFuncao: 'return new Date(input).toLocaleDateString("pt-BR");', parametro: 'string ISO date', criadoEm: '2025-03-01T10:00:00Z' },
  { id: 2, nome: 'extrairCampo', corpoDaFuncao: 'return input?.data ?? input;', parametro: 'objeto com campo data opcional', criadoEm: '2025-03-05T11:00:00Z' },
]

const fluxos: Fluxo[] = [
  { id: 1, nome: 'Busca de Endereço por CEP', operacoes: [], criadoEm: '2025-02-01T09:00:00Z' },
]

const nos: No[] = []
const operacoes: Operacao[] = []

// ── Mutable store ────────────────────────────────────────────────────────────

export const store = {
  integracoes: [...integracoes],
  endpoints: [...endpoints],
  funcoes: [...funcoes],
  fluxos: [...fluxos],
  nos: [...nos],
  operacoes: [...operacoes],
  nextId,
}
