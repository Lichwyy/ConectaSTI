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

### 5. JSONPlaceholder interpolacao

- Rota: `GET /examples/interpolation/jsonplaceholder`
- URL local: `http://localhost:5184/examples/interpolation/jsonplaceholder`

O que faz:
- valida interpolacao em URL, headers e body usando JSONPlaceholder
- executa uma sequencia de chamadas HTTP e normaliza o resultado final

Retorno esperado:
- `provider`
- `post`
- `comment`
- `created`

Exemplo:

```bash
curl http://localhost:5184/examples/interpolation/jsonplaceholder
```

## Rotas governamentais

As rotas abaixo usam entrada dinamica capturada pela camada de rotas e enviada para o executor do fluxo.

Placeholders disponiveis nos endpoints, headers e bodies dos nos:

- `{{routeParams.nome}}` para parametros do caminho, como `{uf}`
- `{{queryParams.nome}}` para parametros da query string, como `?query=saude`
- `{{body.campo}}` para campos de JSON enviado no body

### 6. IBGE municipios por UF

- Rota: `GET /gov/ibge/ufs/{uf}/municipios`
- Exemplo local: `http://localhost:5184/gov/ibge/ufs/SP/municipios`
- Senha: nao

O que faz:
- captura `SP` em `routeParams.uf`
- chama a API de Localidades do IBGE em `estados/{{routeParams.uf}}/municipios`
- usa uma `FuncaoJS` para devolver municipios resumidos

Campos esperados no retorno:
- `provider`
- `uf`
- `regiao`
- `total`
- `municipios`

Exemplo:

```bash
curl http://localhost:5184/gov/ibge/ufs/SP/municipios
```

### 7. IBGE detalhe de municipio

- Rota: `GET /gov/ibge/municipios/{codigoMunicipio}`
- Exemplo local: `http://localhost:5184/gov/ibge/municipios/3550308`
- Senha: nao

O que faz:
- captura `3550308` em `routeParams.codigoMunicipio`
- busca o municipio pelo codigo IBGE
- devolve cidade, UF, regiao e regioes territoriais do IBGE

Campos esperados no retorno:
- `provider`
- `codigoIbge`
- `nome`
- `uf`
- `regiao`
- `regiaoIntermediaria`
- `regiaoImediata`

Exemplo:

```bash
curl http://localhost:5184/gov/ibge/municipios/3550308
```

### 8. Banco Central SGS ultimos pontos

- Rota: `GET /gov/bcb/sgs/{codigoSerie}/ultimos/{quantidade}`
- Exemplo local: `http://localhost:5184/gov/bcb/sgs/11/ultimos/10`
- Senha: nao

O que faz:
- captura `codigoSerie` e `quantidade` da rota
- chama o SGS do Banco Central
- resume os pontos retornados

Exemplos de series:
- `11` -> Selic
- `433` -> IPCA

Campos esperados no retorno:
- `provider`
- `total`
- `primeiro`
- `ultimo`
- `pontos`

Exemplo:

```bash
curl http://localhost:5184/gov/bcb/sgs/11/ultimos/10
```

### 9. Camara deputados por UF

- Rota: `GET /gov/camara/deputados/{uf}`
- Exemplo local: `http://localhost:5184/gov/camara/deputados/SP`
- Senha: nao

O que faz:
- captura `SP` em `routeParams.uf`
- chama Dados Abertos da Camara
- devolve deputados resumidos da UF

Campos esperados no retorno:
- `provider`
- `total`
- `deputados`
- `links`

Exemplo:

```bash
curl http://localhost:5184/gov/camara/deputados/SP
```

### 10. Senado senadores atuais

- Rota: `GET /gov/senado/senadores`
- URL local: `http://localhost:5184/gov/senado/senadores`
- Senha: nao

O que faz:
- chama Dados Abertos do Senado
- normaliza a lista de senadores em exercicio

Campos esperados no retorno:
- `provider`
- `total`
- `senadores`

Exemplo:

```bash
curl http://localhost:5184/gov/senado/senadores
```

### 11. TSE busca de datasets

- Rota: `GET /gov/tse/datasets?query=eleicoes`
- URL local: `http://localhost:5184/gov/tse/datasets?query=eleicoes`
- Senha: nao

O que faz:
- captura `query=eleicoes` em `queryParams.query`
- busca datasets no catalogo CKAN de Dados Abertos do TSE
- devolve uma lista resumida de datasets

Campos esperados no retorno:
- `provider`
- `total`
- `datasets`

Exemplo:

```bash
curl "http://localhost:5184/gov/tse/datasets?query=eleicoes"
```

### 12. TSE datasets por body

- Rota: `POST /gov/tse/datasets/body`
- URL local: `http://localhost:5184/gov/tse/datasets/body`
- Senha: nao

O que faz:
- recebe `query` e `rows` no JSON enviado pelo cliente
- usa `{{body.query}}` e `{{body.rows}}` para consultar o catalogo CKAN do TSE
- demonstra uma rota publica `POST` usando body para parametrizar uma API governamental

Campos esperados no retorno:
- `provider`
- `modo`
- `total`
- `datasets`

Exemplo:

```bash
curl -X POST http://localhost:5184/gov/tse/datasets/body ^
  -H "Content-Type: application/json" ^
  -d "{\"query\":\"eleicoes\",\"rows\":5}"
  -d "{\"query\":\"partidos\",\"rows\":5}"
  -d "{\"query\":\"candidatos\",\"rows\":5}"
  -d "{\"query\":\"prestacao de contas\",\"rows\":5}"
  -d "{\"query\":\"filiados\",\"rows\":5}"
  -d "{\"query\":\"votacao\",\"rows\":5}"
  -d "{\"query\":\"zonas eleitorais\",\"rows\":5}"
  -d "{\"query\":\"urna\",\"rows\":5}"
  -d "{\"query\":\"municipios\",\"rows\":10}"
  -d "{\"query\":\"comparecimento\",\"rows\":5}"
```

### 13. TSE datasets pre-definido

- Rota: `POST /gov/tse/datasets/predefinido`
- URL local: `http://localhost:5184/gov/tse/datasets/predefinido`
- Senha: nao

O que faz:
- consulta o mesmo catalogo CKAN do TSE
- usa parametros fixos definidos no fluxo: `q=eleicoes` e `rows=5`
- demonstra a alternativa em que a rota apenas dispara uma integracao ja configurada

Campos esperados no retorno:
- `provider`
- `modo`
- `total`
- `datasets`

Exemplo:

```bash
curl -X POST http://localhost:5184/gov/tse/datasets/predefinido
```

## Ordem recomendada de teste

1. Testar `GET /examples/cep/01001000`
2. Testar `GET /examples/pokemon/ditto` com `X-Rota-Senha`
3. Testar `GET /examples/agify/maria/save`
4. Testar `GET /examples/agify/maria/cache` com `X-Rota-Senha`
5. Testar `GET /examples/interpolation/jsonplaceholder`
6. Testar `GET /gov/ibge/ufs/SP/municipios`
7. Testar `GET /gov/ibge/municipios/3550308`
8. Testar `GET /gov/bcb/sgs/11/ultimos/10`
9. Testar `GET /gov/camara/deputados/SP`
10. Testar `GET /gov/senado/senadores`
11. Testar `GET /gov/tse/datasets?query=eleicoes`
12. Testar `POST /gov/tse/datasets/body`
13. Testar `POST /gov/tse/datasets/predefinido`

## Observacoes

- As rotas com senha usam o header `X-Rota-Senha`
- O fluxo `agify/maria/cache` depende do `agify/maria/save` ter sido executado antes
- As seeds sao aplicadas por migration
- As rotas antigas continuam funcionando sem entrada dinamica
- A API CKAN do TSE funciona de forma estavel por consulta HTTP com parametros; por isso os exemplos novos usam rotas publicas `POST` para receber body, mas consultam o TSE pelo endpoint de busca ja validado
- Portal da Transparencia, Consulta CNPJ oficial, Compras.gov.br e dados.gov.br ficaram fora desta leva porque exigem chave, retornaram autorizacao obrigatoria ou precisam de confirmacao/configuracao especifica
- Para inspecionar os registros pela API administrativa:
  - `http://localhost:5055/api/Rota`
  - `http://localhost:5055/api/Fluxo`
  - `http://localhost:5055/api/FluxoVersionado`
