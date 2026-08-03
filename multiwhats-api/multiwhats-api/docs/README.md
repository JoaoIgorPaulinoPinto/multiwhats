# MultiWhats API

API REST + WebSocket para gerenciamento de conversas WhatsApp, contatos, clientes, ocorrências e tarefas.

## Stack

| Camada | Tecnologia |
|---|---|
| Backend | ASP.NET Core 10 (Web API + SignalR) |
| Database | PostgreSQL (Npgsql EF Core 10) |
| Auth | JWT Bearer (HMAC-SHA256) + BCrypt |
| WhatsApp | Node.js + WhatsApp Web.js (Puppeteer) |
| Real-time | ASP.NET Core SignalR |

## Pré-requisitos

- .NET 10 SDK
- PostgreSQL 14+ (banco principal)
- Node.js 18+ (para o serviço de messageria)

## Como Rodar

```bash
# API (.NET)
dotnet run

# Serviço Node.js (WhatsApp)
cd ../messageria
npm start
```

- API: `http://localhost:5261`
- Swagger: `http://localhost:5261/swagger`
- Node.js: `http://localhost:3333`

## Documentação

| Documento | Descrição |
|---|---|
| [Arquitetura](arquitetura.md) | Padrões, camadas, fluxo de dados |
| [Entidades](entidades.md) | Modelagem de dados e relacionamentos |
| [Enums](enums.md) | Referência dos tipos enumerados |
| [API](api.md) | Todas as rotas, payloads e respostas |
| [Autenticação](autenticacao.md) | JWT, roles e autorização |
| [Auditoria](auditoria.md) | Sistema de rastreabilidade |
| [Comandos](commands.md) | Comandos úteis (dotnet, docker, ef) |
| [Webhook](webhook.md) | Integração Node.js ↔ ASP.NET |

## Estrutura do Projeto

```
src/
├── controllers/        # 9 controllers (pontas de entrada HTTP)
├── data/
│   ├── db/             # AppDbContext + Factory
│   ├── entities/       # 11 entidades
│   ├── enums/          # 8 enums
│   ├── dtos/           # 35 DTOs (requests, responses, webhook)
│   └── strategies/     # Strategy Pattern para tipos de mensagem
├── repositories/       # 9 interfaces + 9 implementações
├── services/           # TokenService, AuditService, UseCaseLogger, SignalR Hub
├── usecases/           # 27 interfaces + 24 implementações
└── helpers/            # PhoneNumberHelper
```
