# Exemplos de Fluxos Seedados

Este arquivo descreve os fluxos de exemplo que sobem junto com as migrations do `ConectaSTI`.

## Rotas protegidas

As rotas abaixo exigem o header `X-Rota-Senha`:

- `GET /examples/pokemon/ditto` -> `X-Rota-Senha: poke-ditto-123`
- `GET /examples/agify/maria/cache` -> `X-Rota-Senha: agify-cache-123`

## Rotas disponiveis

### 1. Pokemon Ditto resumido

- Rota: `GET /examples/pokemon/ditto`
- URL local: `http://localhost:5184/examples/pokemon/ditto`
- Senha: `poke-ditto-123`
- Header obrigatorio: `X-Rota-Senha`

O que faz:
- chama a PokeAPI em `pokemon/ditto`
- usa uma `FuncaoJS` para reduzir o retorno
- devolve apenas os campos principais do Pokemon

Campos esperados no retorno:
- `id`
- `name`
- `baseExperience`
- `height`
- `weight`
- `types`
- `abilities`
- `officialArtwork`
- `latestCry`

Exemplo:

```bash
curl http://localhost:5184/examples/pokemon/ditto ^
  -H "X-Rota-Senha: poke-ditto-123"
```

### 2. ViaCEP resumido

- Rota: `GET /examples/cep/01001000`
- URL local: `http://localhost:5184/examples/cep/01001000`
- Senha: nao

O que faz:
- chama a ViaCEP para o CEP `01001000`
- usa uma `FuncaoJS` para devolver apenas os dados mais uteis

Campos esperados no retorno:
- `cep`
- `logradouro`
- `bairro`
- `localidade`
- `uf`
- `ddd`
- `ibge`

Exemplo:

```bash
curl http://localhost:5184/examples/cep/01001000
```

### 3. Agify salvar cache

- Rota: `GET /examples/agify/maria/save`
- URL local: `http://localhost:5184/examples/agify/maria/save`
- Senha: nao

O que faz:
- chama a Agify para `maria`
- usa uma `FuncaoJS` para normalizar o payload
- salva o resultado no storage com a chave `seed:agify:maria`

Campos esperados no retorno:
- `name`
- `age`
- `count`
- `provider`
- `cacheKey`
- `cachedAt`

Exemplo:

```bash
curl http://localhost:5184/examples/agify/maria/save
```

### 4. Agify ler cache

- Rota: `GET /examples/agify/maria/cache`
- URL local: `http://localhost:5184/examples/agify/maria/cache`
- Senha: `agify-cache-123`
- Header obrigatorio: `X-Rota-Senha`

O que faz:
- le o valor salvo anteriormente no storage pela rota de save
- usa uma `FuncaoJS` para devolver um resumo diferente

Campos esperados no retorno:
- `source`
- `cacheKey`
- `cachedAt`
- `name`
- `age`
- `count`
- `summary`

Exemplo:

```bash
curl http://localhost:5184/examples/agify/maria/cache ^
  -H "X-Rota-Senha: agify-cache-123"
```

## Ordem recomendada de teste

1. Testar `GET /examples/cep/01001000`
2. Testar `GET /examples/pokemon/ditto` com `X-Rota-Senha`
3. Testar `GET /examples/agify/maria/save`
4. Testar `GET /examples/agify/maria/cache` com `X-Rota-Senha`

## Observacoes

- As rotas com senha usam o header `X-Rota-Senha`
- O fluxo `agify/maria/cache` depende do `agify/maria/save` ter sido executado antes
- As seeds sao aplicadas por migration
- Para inspecionar os registros pela API administrativa:
  - `http://localhost:5055/api/Rota`
  - `http://localhost:5055/api/Fluxo`
  - `http://localhost:5055/api/FluxoVersionado`
