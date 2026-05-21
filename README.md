# ConectaSTI

Backend em **C# / ASP.NET Core (.NET)** com **PostgreSQL**, organizado em múltiplos projetos e pronto para rodar localmente via **Docker Compose** (recomendado) ou via **.NET SDK**.

> Referência do repositório/estrutura: branch `ColocandoRotas`

---

## Sumário

- [Visão geral](#visão-geral)
- [Arquitetura / Projetos](#arquitetura--projetos)
- [Requisitos](#requisitos)
- [Como rodar com Docker (recomendado)](#como-rodar-com-docker-recomendado)
- [Como rodar sem Docker (usando dotnet)](#como-rodar-sem-docker-usando-dotnet)
- [Configurações (appsettings e variáveis de ambiente)](#configurações-appsettings-e-variáveis-de-ambiente)
- [Banco de dados e migrações](#banco-de-dados-e-migrações)
- [Documentação da API (OpenAPI / Scalar)](#documentação-da-api-openapi--scalar)
- [Portas e endpoints](#portas-e-endpoints)
- [Licença](#licença)

---

## Visão geral

O ConectaSTI é um backend com:

- **PostgreSQL** como banco de dados.
- **2 serviços web**:
  - `conectasti-api` (API principal)
  - `conectasti-rotas` (serviço de rotas, com integração via `ExecutionEngine`)
- **NHibernate** (há arquivos `nhibernate.cfg.xml` e mapeamentos no assembly do domínio).
- Pipeline de API com **OpenAPI** e UI via **Scalar** em ambiente de desenvolvimento.

---

## Arquitetura / Projetos

A solução fica em:

- `ConectaSTI.Backend/ConectaSTI/ConectaSTI.slnx`

Projetos principais:

- `ConectaSTI.Backend/ConectaSTI/ConectaSTI.Api`
  - API principal (controllers, autenticação, CORS liberado em desenvolvimento, OpenAPI/Scalar em dev).
- `ConectaSTI.Backend/ConectaSTI/ConectaSTI.Rotas`
  - Serviço separado que se comunica com a API via `ExecutionEngine`.
- `ConectaSTI.Backend/ConectaSTI/ConectaSTI.Dominio`
  - Domínio e mapeamentos (usado no `nhibernate.cfg.xml`).
- `ConectaSTI.Backend/ConectaSTI/ConectaSTI.Executor`
  - Componentes de execução/serviços.
- Dependências genéricas (na solução):
  - `ConectaSTI.Backend/FGB.Api`
  - `ConectaSTI.Backend/FGB`

> Observação: os projetos web usam `TargetFramework net10.0`.

---

## Requisitos

### Para rodar com Docker (recomendado)
- Docker
- Docker Compose

### Para rodar sem Docker
- .NET SDK compatível com `net10.0`
- PostgreSQL disponível localmente ou remoto

---

## Como rodar com Docker (recomendado)

O arquivo `ConectaSTI.Backend/docker-compose.yml` sobe:

- `postgres` (imagem `postgres:17`)
- `conectasti-api` (build usando `ConectaSTI/ConectaSTI.Api/Dockerfile`)
- `conectasti-rotas` (build usando `ConectaSTI/ConectaSTI.Rotas/Dockerfile`)

### 1) Subir os containers

```bash
cd ConectaSTI.Backend
docker compose up --build
```

### 2) Acessar os serviços

- API: `http://localhost:5055`
- Rotas: `http://localhost:5184`
- Postgres: `localhost:5432` (usuário/senha abaixo)

---

## Como rodar sem Docker (usando dotnet)

> Esta opção é útil se você não quiser containerizar, mas exige Postgres rodando por fora.

### 1) Subir Postgres (fora do Docker do projeto)
Crie um banco e usuário compatíveis com a connection string padrão (veja abaixo), ou ajuste o `appsettings.json`.

Padrão esperado:
- **Database**: `ConectaSTI`
- **User**: `postgres`
- **Password**: `1234`
- **Port**: `5432`

### 2) Rodar a API
A partir da pasta do projeto:

```bash
cd ConectaSTI.Backend/ConectaSTI/ConectaSTI.Api
dotnet restore
dotnet run
```

### 3) Rodar o serviço de Rotas
Em outro terminal:

```bash
cd ConectaSTI.Backend/ConectaSTI/ConectaSTI.Rotas
dotnet restore
dotnet run
```

> Se você rodar sem Docker, confirme que o `ExecutionEngine` do serviço Rotas aponta para a URL correta da API.

---

## Configurações (appsettings e variáveis de ambiente)

### Connection string padrão (local)

A API e o Rotas possuem `appsettings.json` com:

```json
"ConnectionStrings": {
  "Default": "Server=localhost;Port=5432;Database=ConectaSTI;User Id=postgres;Password=1234"
}
```

### Variáveis usadas no Docker Compose

No Docker, a connection string é injetada por variável de ambiente (no formato do .NET):

- `ConnectionStrings__Default=Server=postgres;Port=5432;Database=ConectaSTI;User Id=postgres;Password=1234`

E também:

- `ASPNETCORE_ENVIRONMENT=Development`
- `ASPNETCORE_URLS=http://+:8080`

No serviço **Rotas**, existe ainda:

- `ExecutionEngine=http://conectasti-api:8080`

### Autenticação (BifrostAuth)

A API possui configurações em `appsettings.json`:

- `BifrostAuth:JwtKey`
- `BifrostAuth:Issuer`
- `BifrostAuth:ClientId`

**Recomendação:** para produção, substitua a `JwtKey` por um segredo forte e mantenha via variável de ambiente/secret manager.

---

## Banco de dados e migrações

A API tem um fluxo de migração controlado por configuração:

- `MigrateDb` (boolean)
- `MigrationFolder` (string, padrão: `Migracoes`)

Quando `MigrateDb=true`, ao iniciar a aplicação ela executa a atualização do banco usando `MigrationFolder`.

No Docker Compose:
- `conectasti-api` roda com `MigrateDb: "true"`
- `conectasti-rotas` roda com `MigrateDb: "false"`

---

## Documentação da API (OpenAPI / Scalar)

Em ambiente `Development`, a API habilita OpenAPI + UI via **Scalar**.

Ao abrir:

- `http://localhost:5055/`

a aplicação redireciona para a UI do Scalar (rota `/scalar/v1`).

---

## Portas e endpoints

### Portas (Docker)
- `conectasti-api`: `localhost:5055` → container `8080`
- `conectasti-rotas`: `localhost:5184` → container `8080`
- `postgres`: `localhost:5432`

---

## Licença

Este projeto está sob a licença **MIT**. Consulte o arquivo `LICENSE`.
