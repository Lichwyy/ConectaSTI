# ConectaSTI Frontend — Integração com Backend + Funcionalidades do Canvas

**Data:** 2026-04-29
**Branch:** feat/frontend

---

## Visão Geral

Substituição de toda a camada de mock data por integração real com a API .NET do ConectaSTI, além da implementação das novas funcionalidades do canvas de workflow (4 tipos de nó, Funções JS, Storage). O projeto é similar ao n8n: gerencia APIs/endpoints e permite criar fluxos automatizados com blocos visuais arrastáveis.

---

## 1. Modelo de Dados

### Mapeamento backend → frontend

| Backend | Frontend (atual) | Observação |
|---|---|---|
| `Integracao` | `Api` | id é `number`, não string |
| `EndPoint` | `Endpoint` | campo `recurso` (path), `verbo` é enum numérico |
| `Fluxo` | `Workflow` | não embute nós; referencia `Operacao[]` |
| `No` | node do ReactFlow | entidade separada, 4 tipos |
| `Operacao` | não existia | liga Fluxo↔No com ordem e config de retry |
| `Funcao` | não existia | bloco JS com `corpoDaFuncao` |

### Enums

```ts
// VerboHttp
GET = 1, POST = 2, PUT = 3, DELETE = 4, PATCH = 5

// TipoNo
Requisicao = 1, FuncaoJS = 2, SalvarStorage = 3, PegarStorage = 4

// TipoErro (valores exatos a confirmar contra backend)
// assumido: Parar = 1, Continuar = 2, Repetir = 3

// BackoffType (valores exatos a confirmar contra backend)
// assumido: Constante = 1, Linear = 2, Exponencial = 3
```

### Tipos TypeScript (lib/types.ts)

```ts
export type VerboHttp = 1 | 2 | 3 | 4 | 5
export type TipoNo = 1 | 2 | 3 | 4
export type TipoErro = 1 | 2 | 3
export type BackoffType = 1 | 2 | 3

export interface Integracao {
  id: number
  nome: string
  url: string
  token?: string | null
  descricao?: string | null
  criadoEm?: string | null
}

export interface EndPoint {
  id: number
  recurso: string
  integracaoId: number
  verbo: VerboHttp
  descricao?: string | null
  criadoEm?: string | null
}

export interface Funcao {
  id: number
  nome: string
  corpoDaFuncao: string
  parametro?: string | null
  criadoEm?: string | null
}

export interface No {
  id: number
  tipo: TipoNo
  body?: string | null
  headers?: string | null
  funcaoId?: number | null
  endPointId?: number | null
  chaveValor?: string | null
}

export interface Operacao {
  id: number
  ordem: number
  noId: number
  fluxoId: number
  repetir?: boolean
  erro: TipoErro
  maxRetries?: number
  backoffType: BackoffType
  backoffDelay?: number
  backoffMultiplier?: number
  timeout?: number
}

export interface Fluxo {
  id: number
  nome: string
  operacoes?: Operacao[] | null
  criadoEm?: string | null
}
```

**Campos opcionais:** `descricao` nunca é obrigatório em nenhuma entidade — o campo é `nullable` no backend e os formulários do frontend não devem exigir seu preenchimento.

**Helpers de conversão** mantidos em `lib/types.ts`:
- `verboToHttpMethod(v: VerboHttp): HttpMethod` — converte enum numérico para string de exibição
- `httpMethodToVerbo(m: HttpMethod): VerboHttp` — inverso
- `METHOD_COLORS` e `METHOD_NODE_COLORS` permanecem mas recebem função auxiliar

---

## 2. Camada de Cliente HTTP

**Arquivo:** `lib/api/client.ts`

- URL base: `process.env.NEXT_PUBLIC_API_URL ?? 'http://localhost:5000'`
- Prefixo `/api` embutido no cliente (serviços não repetem)
- Classe `ApiError` carrega `errors: string[]` para exibição no frontend
- Erros não-2xx: tenta `res.json()` → espera array de strings; fallback para `res.statusText`
- Variável de ambiente em `.env.local` na raiz do projeto

```
NEXT_PUBLIC_API_URL=http://localhost:5000
```

---

## 3. Serviços (lib/api/)

### Estrutura de arquivos

```
lib/api/
  client.ts          ← atualizado
  integracoes.ts     ← substitui apis.ts (removido)
  endpoints.ts       ← atualizado
  funcoes.ts         ← novo
  fluxos.ts          ← substitui workflows.ts (removido)
  nos.ts             ← novo
  operacoes.ts       ← novo
  mock/              ← removido inteiro
```

### Padrão de cada serviço

Funções puras, sem estado. Cada arquivo exporta funções nomeadas que delegam ao `client`. Exemplo para Integracao:

```ts
getIntegracoes()
getIntegracao(id: number)
createIntegracao(data: Omit<Integracao, 'id' | 'criadoEm'>)
updateIntegracao(id: number, data: Omit<Integracao, 'id' | 'criadoEm'>)
deleteIntegracao(id: number)
```

### Rotas extras

```ts
// nos.ts
testeRequest(noId: number)  → POST /testerequest/{noId}

// funcoes.ts
testeFunction(funcaoId: number, body: unknown)  → POST /testefunction/{funcaoId}
```

### Hooks atualizados

| Hook antigo | Hook novo |
|---|---|
| `useApis` | `useIntegracoes` |
| `useEndpoints` | `useEndpoints` (mantém nome, atualiza tipos) |
| `useWorkflows` | `useFluxos` |
| — | `useFuncoes` (novo) |
| — | `useNos` (novo, uso interno do canvas) |

Todos mantêm o padrão: `{ data, loading, error, reload, create, update, delete }`.

---

## 4. Arquitetura do Canvas de Workflow

### Persistência dividida

| Dado | Onde |
|---|---|
| Nome do fluxo | Backend — `Fluxo.nome` |
| Tipo e config de cada bloco | Backend — `No` |
| Ordem de execução | Backend — `Operacao.ordem` |
| Config retry/timeout/erro | Backend — `Operacao` |
| Posição X/Y no canvas | `localStorage` — chave `canvas-state-{fluxoId}` |
| Edges (conexões visuais) | `localStorage` — mesma chave |

**Formato do localStorage:**
```ts
interface CanvasState {
  positions: Record<number, { x: number; y: number }>
  edges: Edge[]
}
```

### Fluxo de carregamento

1. `GET /api/Fluxo/{id}` → retorna `Fluxo` com `operacoes[]`
2. Para cada `operacao.noId`, `GET /api/No/{id}` (em paralelo via `Promise.all`)
3. Ler `localStorage['canvas-state-{id}']` → aplicar posições e edges
4. Construir nós do ReactFlow a partir dos `No` + `Operacao`

### Fluxo de salvamento

1. `PUT /api/Fluxo/{id}` — atualiza nome
2. Para cada nó no canvas:
   - ID temporário (negativo): `POST /api/No` → `POST /api/Operacao` → atualizar id local
   - ID real: `PUT /api/No/{id}` + `PUT /api/Operacao/{id}`
3. Para nós removidos: `DELETE /api/Operacao/{id}` → `DELETE /api/No/{id}`
4. Salvar `CanvasState` no `localStorage`

**IDs temporários:** nós recém-criados no canvas recebem id temporário negativo (`-1`, `-2`, ...) até o save. Após save, o id real do backend substitui o temporário no estado local.

### Painel de detalhe do nó (barra direita)

Quando um nó está selecionado, exibe:
- Info do bloco (tipo, endpoint ou função referenciada)
- **Configuração da Operacao:**
  - Ordem (read-only)
  - Tipo de erro (dropdown)
  - Máx. repetições, Timeout
  - BackoffType + Delay + Multiplier (visíveis apenas quando erro = Repetir)
- Botão "Testar nó" → `POST /testerequest/{noId}` (só ativo quando nó tem id real)

---

## 5. Novas Funcionalidades

### 5a — Página de Funções (/funcoes)

Segue o padrão visual das páginas de APIs e Endpoints: painel esquerdo (lista) + painel direito (detalhe/formulário).

**Formulário:**
- `nome` (obrigatório)
- `corpoDaFuncao` (obrigatório, textarea monospace — corpo JS)
- `parametro` (opcional — descreve o input esperado)

**Ação de teste inline:**
- Campo JSON de entrada + botão "Testar"
- Chama `POST /testefunction/{id}` com o JSON digitado
- Exibe resposta ou erro inline no painel

**Sidebar:** novo item "Funções" com ícone de código (`CodeIcon` do Phosphor)

### 5b — Tipos de nó no canvas

| Tipo | Cor da borda | Campo principal |
|---|---|---|
| `Requisicao` (1) | Por método HTTP (existente) | EndPoint referenciado |
| `FuncaoJS` (2) | Roxa (`border-l-violet-500`) | Nome da Funcao |
| `SalvarStorage` (3) | Laranja (`border-l-orange-500`) | `chaveValor` (chave onde salvar) |
| `PegarStorage` (4) | Azul-cinza (`border-l-slate-500`) | `chaveValor` (chave a recuperar) |

### 5c — Paleta de blocos (NodePalette) atualizada

- **Seção "APIs"** — lista Integracoes → EndPoints (arrastar cria nó `Requisicao`)
- **Seção "Funções"** — lista Funcoes cadastradas (arrastar cria nó `FuncaoJS`)
- **Seção "Storage"** — dois itens fixos: "Salvar Storage" e "Pegar Storage"

---

## 6. Páginas Existentes — Ajustes

| Página | Mudança |
|---|---|
| `/apis` | Usa `Integracao` internamente; labels na UI permanecem "APIs" |
| `/endpoints` | Atualiza tipos (`recurso`, `verbo` numérico → exibição de método) |
| `/workflows` | Rota mantida; internamente usa `Fluxo` |
| `/workflows/[id]` | Canvas com 4 tipos de nó, save para backend, config de Operacao |
| `/funcoes` | Nova página |

**Removido:**
- `lib/api/mock/` inteiro
- Tipos antigos `Api`, `Endpoint`, `Workflow`
- Hooks `useApis` (substituído por `useIntegracoes`)
- `lib/api/apis.ts`, `lib/api/workflows.ts` (substituídos)

**Não muda:**
- Rotas Next.js (nenhuma rota nova exceto `/funcoes`)
- Design visual — sem refactor de estilo
- ReactFlow e lógica de drag-and-drop existente
- Padrão de hooks com `loading`, `error`, `reload`
- Componentes shadcn já instalados

---

## 7. Instalação de Componentes Shadcn Necessários

Verificar se os seguintes já estão instalados antes de usar; instalar via `bunx shadcn@latest add <nome>` se necessário:

- `textarea` — editor de código da Funcao
- `dialog` — modal de teste inline
- `toast` / `sonner` — feedback de erros da API

---

## 8. Ordem de Implementação (Feature-by-feature)

1. `lib/types.ts` — novos tipos e helpers de conversão
2. `lib/api/client.ts` — error handling + base path
3. `.env.local` — variável de URL
4. Serviços: `integracoes.ts`, `endpoints.ts` atualizados
5. Hooks: `useIntegracoes`, `useEndpoints` atualizados
6. Páginas `/apis` e `/endpoints` adaptadas aos novos tipos
7. Serviço e hook `funcoes.ts` / `useFuncoes`
8. Página `/funcoes` nova
9. Serviços `nos.ts`, `operacoes.ts`, `fluxos.ts`
10. Hook `useFluxos`
11. Canvas: lógica de save/load com backend + localStorage
12. Novos tipos de nó no canvas (FuncaoJS, SalvarStorage, PegarStorage)
13. Painel de detalhe com config de Operacao
14. NodePalette com seções de Funções e Storage
15. Sidebar atualizada com item Funções
