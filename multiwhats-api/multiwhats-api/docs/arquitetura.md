# Arquitetura

## Visão Geral

O projeto segue o padrão **Clean Architecture** com separação em camadas bem definidas:

```
Frontend / Cliente HTTP
        │
        ▼
  ┌─────────────────────────────────────┐
  │         Controllers (HTTP)          │
  │     Validação de input, chama       │
  │         Use Cases                   │
  └──────────────┬──────────────────────┘
                 │
  ┌──────────────▼──────────────────────┐
  │         Use Cases (Negócio)         │          ┌─────────────────────────────────────┐
  │     Orquestra operações, regras     │          │                                     │
  │                                      ────────> │   PostgreSQL Database adapter       │
  │         de negócio                  │          │                                     │
  └──────────────┬──────────────────────┘          └─────────────────────────────────────┘
                 │
  ┌──────────────▼──────────────────────┐
  │       Repositories (Dados)          │
  │     Abstrai acesso ao banco,        │
  │         consultas                   │
  └──────────────┬──────────────────────┘
                 │
  ┌──────────────▼──────────────────────┐
  │       DbContext (EF Core)           │
  │     Mapeamento, migrations,         │
  │         audit automático            │
  └──────────────┬──────────────────────┘
                 │
            PostgreSQL (local)
```

## Padrões Utilizados

### 1. Use Case Pattern

Cada operação de negócio é encapsulada em uma classe dedicada:

- **Interface** (`I{Verb}{Entity}UseCase`) — contrato
- **Implementação** (`{Verb}{Entity}UseCase`) — lógica

Controller → chama UseCase → UseCase chama Repository

**Exemplo:**
```
ClientsController → ICreateClientUseCase → CreateClientUseCase → IClientRepository
```

### 2. Repository Pattern

Abstrai o acesso ao banco de dados. Cada entidade tem:

- Interface: `I{Entity}Repository`
- Implementação: `{Entity}Repository`

Opera sobre o `AppDbContext`, expõe métodos de CRUD e consultas.

### 3. Strategy Pattern

Tipos de mensagem WhatsApp são tratados por strategies específicas:

```
IMessageStrategy (interface)
    ├── TextMessageStrategy
    ├── ImageMessageStrategy
    ├── VideoMessageStrategy
    ├── AudioMessageStrategy
    ├── DocumentMessageStrategy
    └── StickerMessageStrategy

MessageStrategyFactory → seleciona a strategy pelo MessageType
```

Cada strategy implementa:
- `BuildNodePayload()` — monta o payload para enviar ao Node.js
- `BuildMessageFields()` — preenche os campos da entidade Message

### 4. Dependency Injection

Todas as dependências são registradas em `Program.cs` via DI container:
- Use Cases → Scoped
- Repositories → Scoped
- DbContext → Scoped
- TokenService, AuditService → Singleton/Scoped

## Fluxo de Dados

### Mensagem Enviada (ASP.NET → WhatsApp)

```
1. Cliente envia POST /api/messages/send
2. SendMessageUseCase recebe requisição
3. Seleciona IMessageStrategy via MessageStrategyFactory
4. Monta payload via BuildNodePayload()
5. Envia HTTP POST para Node.js (localhost:3333/api/enviar)
6. Node.js envia via WhatsApp Web (Puppeteer)
7. Salva Message no banco com Direction = Outgoing, DeliveryStatus = Pending
8. Emite SignalR notification para frontend
```

### Mensagem Recebida (WhatsApp → ASP.NET)

```
1. WhatsApp entrega mensagem ao Node.js
2. Node.js faz POST /api/webhook/whatsapp
3. SaveIncomingMessageUseCase recebe o webhook
4. Salva Message no banco (dedup por WhatssAppMessageId; fromMe → Outgoing)
5. Atualiza Contact/Chat (LastMessageAt)
6. Emite SignalR notification para frontend
```

### Status de Entrega (WhatsApp → ASP.NET)

```
1. WhatsApp atualiza ACK da mensagem (1=enviada, 2=entregue, 3=lida)
2. Node.js evento 'message_ack' faz POST /api/webhook/status
3. UpdateMessageDeliveryStatusUseCase localiza Message por WhatssAppMessageId
4. Atualiza DeliveryStatus (evita regressão)
5. Emite SignalR 'MessageDeliveryStatusChanged' para o frontend
```

## Comunicação Entre Serviços

```
ASP.NET Core API (porta 5261)
        │
        ├── POST http://localhost:3333/api/enviar
        │       → Envia mensagens via WhatsApp
        │
        ├── POST http://localhost:3333/api/sync
        │       → Sincroniza mensagens recentes dos chats
        │
        └── Recebe POST /api/webhook/whatsapp
        │       ← Recebe mensagens do WhatsApp
        │
        └── Recebe POST /api/webhook/status
                ← Recebe status de entrega do WhatsApp

SignalR Hub (/whatsappHub)
        │
        └── Notifica frontend em tempo real
```

## Soft Delete

Todas as entidades que herdam de `BaseEntity` usam soft delete:

- Campo `IsDeleted` (bool)
- Global query filter: `HasQueryFilter(e => !e.IsDeleted)`
- `SaveChanges()` converte delete em `IsDeleted = true`
- Nunca remove registros fisicamente do banco

## Auto-Audit

`AppDbContext.SaveChangesAsync()` sobrescrito para:
- Preencher `CreatedAt` em inserts
- Preencher `LastUpdate` em updates
- Preencher `CreatedByUserId` / `LastUpdatedByUserId`
- Converter hard deletes em soft deletes
