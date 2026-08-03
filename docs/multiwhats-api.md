# Documentação Completa - MultiWhats API

## Índice
1. [Visão Geral](#visão-geral)
2. [Tecnologias Utilizadas](#tecnologias-utilizadas)
3. [Arquitetura](#arquitetura)
4. [Estrutura de Diretórios](#estrutura-de-diretórios)
5. [Modelo de Dados (Entidades)](#modelo-de-dados-entidades)
6. [API Endpoints](#api-endpoints)
7. [Camada de Apresentação (Controllers)](#camada-de-apresentação-controllers)
8. [Camada de Use Cases (Casos de Uso)](#camada-de-use-cases-casos-de-uso)
9. [Camada de Repositories (Repositórios)](#camada-de-repositories-repositórios)
10. [Camada de Serviços (Services)](#camada-de-serviços-services)
11. [WebSocket / SignalR](#websocket--signalr)
12. [Bridge Node.js (Messageria)](#bridge-nodejs-messageria)
13. [Autenticação e Autorização](#autenticação-e-autorização)
14. [Auditoria](#auditoria)
15. [Fluxos de Dados Principais](#fluxos-de-dados-principais)
16. [Configuração e Execução](#configuração-e-execução)
17. [Banco de Dados](#banco-de-dados)
18. [API Documentation (Swagger)](#api-documentation-swagger)

---

## Visão Geral

O **MultiWhats API** é um sistema de **CRM + WhatsApp Multi-empresas** que permite gerenciar múltiplos clientes (empresas), contatos do WhatsApp, conversas (chats), mensagens, ocorrências e tarefas em um único painel. Ele funciona como uma ponte entre o WhatsApp (via whatsapp-web.js) e uma interface web, provendo uma API REST + WebSocket para comunicação em tempo real.

O sistema é composto por **duas partes** que rodam simultaneamente:

1. **API Principal (C# ASP.NET Core 10)** - Responsável pela lógica de negócio, banco de dados, autenticação e exposição de endpoints REST + SignalR Hub.
2. **Messageria (Node.js)** - Ponte com o WhatsApp usando a biblioteca `whatsapp-web.js`. Conecta-se ao WhatsApp Web, escuta mensagens recebidas em tempo real e envia mensagens via API.

---

## Tecnologias Utilizadas

### API Principal (.NET)
| Tecnologia | Versão | Propósito |
|---|---|---|
| ASP.NET Core | 10.0 | Framework web |
| .NET | 10.0 | Runtime |
| Entity Framework Core | 10.0 | ORM para banco de dados |
| PostgreSQL (Npgsql) | 10.0 | Banco de dados relacional |
| JWT Bearer Authentication | 10.0 | Autenticação stateless |
| SignalR | 10.0 | WebSocket para tempo real |
| Swagger (Swashbuckle) | 7.3 | Documentação da API |
| BCrypt.Net-Next | 4.0.3 | Hash de senhas |
| C# 13 | 13.0 | Linguagem |

### Messageria (Node.js)
| Tecnologia | Versão | Propósito |
|---|---|---|
| Node.js | - | Runtime JavaScript |
| whatsapp-web.js | 1.34.6 (fixa) | Cliente WhatsApp Web automatizado |
| Express | 5.2.1 | Servidor HTTP |
| Axios | 1.18.1 | Cliente HTTP para chamar a API .NET |
| qrcode-terminal | 0.12.0 | Exibir QR Code no terminal |

---

## Arquitetura

A API segue uma **arquitetura em camadas** inspirada em Clean Architecture com Use Cases:

```
┌─────────────────────────────────────────────┐
│                Controllers                   │  ← Camada de transporte (HTTP)
├─────────────────────────────────────────────┤
│              Use Cases (Casos de Uso)        │  ← Camada de aplicação (lógica de negócio)
├─────────────────────────────────────────────┤
│              Repositories                    │  ← Camada de persistência (acesso a dados)
├─────────────────────────────────────────────┤
│           AppDbContext (EF Core)             │  ← ORM + Mapeamento
├─────────────────────────────────────────────┤
│                PostgreSQL                    │  ← Banco de dados
└─────────────────────────────────────────────┘

┌─────────────────────────────────────────────┐
│           Services (Transversais)            │  ← Token, Audit, WhatsApp Hub
└─────────────────────────────────────────────┘
```

### Princípios da Arquitetura

- **Separação por Interface**: Cada Use Case tem sua própria interface (ex: `ILoginUseCase`, `ICreateChatUseCase`), permitindo desacoplamento e testabilidade.
- **Repository Pattern**: Toda lógica de acesso a dados fica nos repositórios, que implementam interfaces.
- **Soft Delete**: Nenhum registro é permanentemente excluído do banco. Todas as entidades herdam de `BaseEntity` que contém `IsDeleted`.
- **Auditoria Automática**: Toda operação de escrita gera logs de auditoria via `UseCaseLogger`.
- **Injeção de Dependência**: Tudo é registrado via DI do .NET (AddScoped, AddSingleton).

---

## Estrutura de Diretórios

```
multiwhats-api/
├── Properties/
│   └── launchSettings.json          ← Configurações de perfil de execução
├── Migrations/
│   └── *_InitialMigration.cs        ← Migração inicial do EF Core
├── messageria/
│   ├── index.js                     ← Servidor Node.js (WhatsApp bridge)
│   └── package.json                 ← Dependências Node.js
├── src/
│   ├── controllers/                 ← Endpoints HTTP (9 controllers)
│   ├── data/
│   │   ├── db/                      ← AppDbContext + Factory
│   │   ├── dtos/
│   │   │   ├── Requests/            ← DTOs de entrada (15 arquivos)
│   │   │   ├── Responses/           ← DTOs de saída (10 arquivos)
│   │   │   └── Webhook/             ← DTO do webhook WhatsApp
│   │   ├── entities/                ← Entidades do banco (10 entidades)
│   │   └── enums/                   ← Enumerações (8 enums)
│   ├── helpers/
│   │   └── PhoneNumberHelper.cs     ← Utilitário de formatação
│   ├── repositories/
│   │   ├── interfaces/              ← Interfaces dos repositórios (9)
│   │   └── repositories/            ← Implementações (9)
│   ├── services/                    ← Serviços transversais (5)
│   └── usecases/
│       ├── interfaces/              ← Interfaces dos use cases (23)
│       └── usecases/                ← Implementações (23)
├── Program.cs                       ← Ponto de entrada da aplicação
├── multiwhats-api.csproj            ← Projeto .NET
├── appsettings.json                 ← Configurações (produção)
├── appsettings.Development.json     ← Configurações (desenvolvimento)
├── Dockerfile                       ← Build Docker multi-estágio
└── package.json                     ← Scripts npm (raiz)
```

---

## Modelo de Dados (Entidades)

### Diagrama de Relacionamentos

```
┌──────────┐     ┌──────────┐     ┌───────────┐
│  Client  │1────N│ Contact  │1────1│   Chat    │
└──────────┘     └──────────┘     └───────────┘
     │                                 │
     │                                 │
     │                                 │1
     │1                                │
     │                                 │
     │    ┌──────────┐     ┌──────────┐│
     │    │  Group   │     │ Messages │N│
     │    └──────────┘     └──────────┘│
     │         │                 │     │
     │         │                 │N    │
     │         │                 │     │
     │    ┌──────────┐     ┌───────────┘
     │    │Contacts  │     │
     │    │ (Group)  │     │N
     │    └──────────┘     │
     │                ┌────────────┐
     │                │Occurrences │
     │                └────────────┘
     │N                    │N
     │                     │
┌───────────┐     ┌───────────┐
│ClientTasks│     │ Messages  │
└───────────┘     └───────────┘
                        │N
                        │
                   ┌────────┐
                   │  User  │
                   └────────┘
```

### Entidades em Detalhe

#### BaseEntity (Abstrata)
| Propriedade | Tipo | Descrição |
|---|---|---|
| CreatedAt | DateTime | Data de criação |
| LastUpdate | DateTime | Data da última alteração |
| IsDeleted | bool | Soft delete (filtro global) |
| CreatedByUserId | int? | ID do usuário que criou |
| LastUpdatedByUserId | int? | ID do último usuário que alterou |

**Comportamento**: O EF Core possui `HasQueryFilter(e => !e.IsDeleted)` globalmente, então registros "deletados" são automaticamente excluídos das queries.

#### User
| Campo | Tipo | Descrição |
|---|---|---|
| Id | int (PK) | ID autoincremento |
| Name | string (único) | Nome de usuário (login) |
| Password | string | Senha **com hash BCrypt** (work factor 12); legadas em texto puro são migradas no primeiro login |
| Role | UserRole | Support, Dev, ou Admin |
| IsActive | bool | Se o usuário está ativo |

#### Client (Empresa/Cliente)
| Campo | Tipo | Descrição |
|---|---|---|
| Id | int (PK) | ID |
| Name | string | Nome da empresa |
| MainPhoneNumber | string | Telefone principal |
| Status | ClientStatus | Active ou Inactive |
| Relacionamentos | | Contacts[], Chats[], Tasks[] |

#### Contact
| Campo | Tipo | Descrição |
|---|---|---|
| Id | int (PK) | ID |
| Jid | string (único) | JID do WhatsApp (ex: 5511999999999@c.us) |
| PhoneNumber | string | Número de telefone |
| Name | string | Nome do contato |
| PushName | string | Nome configurado no WhatsApp |
| ProfilePicUrl | string? | URL da foto de perfil |
| IsBlocked | bool | Se está bloqueado |
| IsGroup | bool | Se é um grupo (vs contato individual) |
| LastMessageAt | DateTime? | Data da última mensagem |
| ClientId | int? | FK para Client (empresa associada) |
| GroupId | int? | FK para Group |
| Relacionamentos | | Chat (1:1), Client, Group |

#### Chat (Conversa)
| Campo | Tipo | Descrição |
|---|---|---|
| Id | int (PK) | ID |
| Jid | string (único) | JID do WhatsApp |
| PhoneNumber | string | Número de telefone |
| Name | string | Nome da conversa |
| ContactId | int? (único) | FK para Contact (1:1) |
| ClientId | int? | FK para Client |
| LastMessageAt | DateTime? | Timestamp da última mensagem |
| LastMessageBody | string? | Corpo da última mensagem |
| AssignedToUserId | int? | FK para User (atendente responsável) |
| Relacionamentos | | Contact (1:1), Client, User (assignee), Messages[], Occurrences[] |

**Nota**: Todo contato pode ter no máximo **um chat**, e vice-versa. É uma relação 1:1.

#### Message
| Campo | Tipo | Descrição |
|---|---|---|
| Id | int (PK) | ID |
| MessageId | string | ID da mensagem no WhatsApp |
| WhatssAppMessageId | string (índice único) | ID do WhatsApp usado para correlação/dedup (ex: `true_...`) |
| FromJid | string | JID de origem |
| ToJid | string | JID de destino |
| PhoneNumber | string | Número de telefone |
| Body | string | Conteúdo da mensagem |
| Direction | MessageDirection | Incoming (0) ou Outgoing (1) |
| Type | MessageType | Text, Image, Audio, Video, Document, Sticker, Contact, Location, Unknown |
| Timestamp | long? | Timestamp Unix |
| SentAt | DateTime? | Data de envio |
| NotifyName | string? | Nome de exibição do remetente |
| HasMedia | bool | Se possui mídia |
| MediaUrl | string? | URL da mídia |
| MediaMimeType | string? | Tipo MIME da mídia |
| MediaFilename | string? | Nome do arquivo de mídia |
| MediaSize | long? | Tamanho do arquivo |
| MediaCaption | string? | Legenda da mídia |
| DeliveryStatus | DeliveryStatus | Pending, Sent, Delivered, Read, Failed (outgoing inicia em Pending; incoming em Delivered) |
| IsForwarded | bool | Se foi encaminhada |
| ChatId | int | FK para Chat |
| UserId | int? | FK para User (quem enviou via sistema) |
| OccurrenceId | int? | FK para Occurrence (se vinculada a uma ocorrência) |
| ReplyToId | int? | FK para Message (mensagem respondida) |
| Relacionamentos | | Chat, User, Occurrence, ReplyTo |

#### Occurrence (Ocorrência)
| Campo | Tipo | Descrição |
|---|---|---|
| Id | int (PK) | ID |
| Title | string | Título |
| Description | string? | Descrição |
| Status | OccurrenceStatus | Open, InProgress, Resolved, Closed |
| Priority | Priority | Low, Medium, High, Urgent |
| ChatId | int | FK para Chat |
| AssignedToUserId | int? | FK para User (responsável) |
| Relacionamentos | | Chat, User (assignee), Messages[] |

#### ClientTask (Tarefa)
| Campo | Tipo | Descrição |
|---|---|---|
| Id | int (PK) | ID |
| Title | string | Título |
| Description | string? | Descrição |
| Status | ClientTaskStatus | Open, InProgress, Completed, Cancelled |
| Priority | Priority | Low, Medium, High, Urgent |
| DueDate | DateTime? | Data de vencimento |
| ClientId | int | FK para Client |
| AssignedToUserId | int? | FK para User (responsável) |
| Relacionamentos | | Client, User (assignee) |

#### Group
| Campo | Tipo | Descrição |
|---|---|---|
| Id | int (PK) | ID |
| Name | string | Nome do grupo |
| Description | string? | Descrição |
| WhatsAppGroupId | string | ID do grupo no WhatsApp |
| Relacionamentos | | Contacts[] |

#### Device (Dispositivo WhatsApp)
| Campo | Tipo | Descrição |
|---|---|---|
| Id | int (PK) | ID |
| Jid | string | JID do dispositivo |
| PhoneNumber | string | Número de telefone |
| PushName | string | Nome de exibição |
| Platform | string | Plataforma (android, ios, web) |
| ConnectedAt | DateTime? | Data de conexão |
| UpdatedAt | DateTime? | Data da última atualização |

**Nota**: Device **não herda de BaseEntity** - não possui soft delete, CreatedAt, etc.

#### AuditLog (Log de Auditoria)
| Campo | Tipo | Descrição |
|---|---|---|
| Id | int (PK) | ID |
| UserId | int? | ID do usuário |
| UserName | string? | Nome do usuário |
| UserRole | string? | Papel do usuário |
| EntityType | string | Tipo da entidade (ex: "Client", "Contact") |
| EntityId | string | ID da entidade |
| Action | string | Ação (Create, Update, Delete, Assign, etc.) |
| Description | string | Descrição legível |
| OldValues | string? | Valores antigos (JSON) |
| NewValues | string? | Novos valores (JSON) |
| Timestamp | DateTime | Data/hora |

### Enums

#### UserRole
| Valor | Descrição |
|---|---|
| Support (0) | Suporte - acesso básico |
| Dev (1) | Desenvolvedor - acesso intermediário |
| Admin (2) | Administrador - acesso total |

#### ClientStatus
| Valor | Descrição |
|---|---|
| Active (0) | Cliente ativo |
| Inactive (1) | Cliente inativo |

#### ClientTaskStatus
| Valor | Descrição |
|---|---|
| Open (0) | Em aberto |
| InProgress (1) | Em andamento |
| Completed (2) | Concluída |
| Cancelled (3) | Cancelada |

#### OccurrenceStatus
| Valor | Descrição |
|---|---|
| Open (0) | Em aberto |
| InProgress (1) | Em andamento |
| Resolved (2) | Resolvida |
| Closed (3) | Fechada |

#### MessageDirection
| Valor | Descrição |
|---|---|
| Incoming (0) | Recebida |
| Outgoing (1) | Enviada |

#### MessageType
| Valor | Descrição |
|---|---|
| Text, Image, Audio, Video, Document, Sticker, Contact, Location, Unknown |

#### DeliveryStatus
| Valor | Descrição | ACK whatsapp-web.js |
|---|---|---|
| Pending | Aguardando envio | 0 |
| Sent | Enviada | 1 |
| Delivered | Entregue ao destinatário | 2 |
| Read | Lida pelo destinatário | 3 (READ/PLAYED) |
| Failed | Falha no envio | -1 |

Default: `Pending` para mensagens enviadas (outgoing) e `Delivered` para recebidas. Atualizado via `POST /api/webhook/status` (evento `message_ack` do Node.js) — sempre avança, nunca regressa.

#### Priority
| Valor | Descrição |
|---|---|
| Low, Medium, High, Urgent |

---

## API Endpoints

### Autenticação (`/api/auth`)

#### POST `/api/auth/register` - [AllowAnonymous]
Registra um novo usuário no sistema.

**Request:**
```json
{
  "name": "joao",
  "password": "123456",
  "registrationCode": "09A88A0C"
}
```

**Response (200):**
```json
{
  "id": 1,
  "name": "joao",
  "role": "Support",
  "isActive": true,
  "createdAt": "2026-07-21T00:00:00"
}
```

**Regras de Negócio:**
- Usuário criado com Role padrão = `Auth:DefaultUserRole` (Support por padrão)
- Nome deve ser único
- Senha é guardada com **hash BCrypt** (work factor 12)
- Se `Auth:RequireRegistrationCode=true` (default), o campo `registrationCode` é obrigatório — o código é validado, único, expira após `Auth:RegistrationCodeExpiryHours` (48h) e é marcado como usado
- Não é possível definir role ou isActive no registro

---

#### POST `/api/auth/codes` - [Authorize(Roles = "Admin,Dev")]
Gera um ou mais códigos de permissão para cadastro de novos usuários.

**Request:**
```json
{
  "count": 1
}
```

**Response (200):**
```json
[
  { "id": 11, "code": "09A88A0C", "expiresAt": "2026-08-05T13:42:29", "isUsed": false }
]
```

**Regras de Negócio:**
- Códigos são hex aleatórios (64 bytes) com até 10 tentativas contra colisão (`ExistsAsync`)
- Expiração conforme `Auth:RegistrationCodeExpiryHours`
- O frontend expõe a geração na tela **Configurações → Usuários → Gerar código de permissão**

---

#### POST `/api/auth/login` - [AllowAnonymous]
Autentica um usuário e retorna um token JWT.

**Request:**
```json
{
  "name": "joao",
  "password": "123456"
}
```

**Response (200):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "user": { "id": 1, "name": "joao", "role": "Support", "isActive": true }
}
```

**Regras de Negócio:**
- Verifica se o usuário existe por nome
- Compara senha com **hash BCrypt** (`PasswordHelper.Verify`); senhas legadas em texto puro são migradas para hash no primeiro login
- Verifica se usuário está ativo (`IsActive`)
- Gera token JWT com claims: NameIdentifier (id), Name, Role
- Token expira conforme `JwtSettings.ExpiryInMinutes` (60 min)
- Em cada request autenticado, o backend relê role/status do usuário no banco (`OnTokenValidated`) — mudanças valem sem novo login

---

#### POST `/api/auth/logout` - [Authorize]
Invalida o token JWT atual (revoga via blacklist).

**Headers:** `Authorization: Bearer <token>`

**Response (200):** `{ "message": "Logout realizado com sucesso" }`

**Regras de Negócio:**
- Extrai o JTI (JWT ID) do token
- Adiciona o JTI ao `TokenBlacklistService` em memória
- O JTI fica na blacklist pelo mesmo tempo de expiração do token (8h)
- Na próxima validação, o token é rejeitado

---

### Clientes / Empresas (`/api/clients`) - [Authorize]

#### POST `/api/clients`
Cria uma nova empresa (client).

**Request:**
```json
{
  "name": "Empresa Exemplo",
  "mainPhoneNumber": "5511999999999",
  "status": 0
}
```

**Response (201):** ClientResponse

---

#### GET `/api/clients`
Lista todas as empresas (não deletadas).

**Response (200):** `ClientResponse[]`

Cada ClientResponse contém:
```json
{
  "id": 1,
  "name": "Empresa Exemplo",
  "mainPhoneNumber": "5511999999999",
  "status": "Active",
  "contactCount": 5,
  "createdAt": "2026-07-21T00:00:00",
  "lastUpdate": "2026-07-21T00:00:00"
}
```

---

#### GET `/api/clients/{id}`
Obtém uma empresa específica por ID.

---

#### PUT `/api/clients/{id}`
Atualiza uma empresa.

**Request:**
```json
{
  "name": "Novo Nome",
  "mainPhoneNumber": "5511888888888",
  "status": 1
}
```

---

#### DELETE `/api/clients/{id}`
Marca uma empresa como deletada (soft delete).

---

#### GET `/api/clients/{id}/contacts`
Lista todos os contatos associados a uma empresa.

**Response (200):** `ContactResponse[]`

---

### Contatos (`/api/contacts`) - [Authorize]

#### POST `/api/contacts`
Cria um novo contato.

**Request:**
```json
{
  "jid": "5511999999999@c.us",
  "phoneNumber": "5511999999999",
  "name": "João Silva",
  "clientId": 1,
  "isGroup": false
}
```

---

#### GET `/api/contacts`
Lista todos os contatos.

**Response (200):** `ContactResponse[]`

---

#### GET `/api/contacts/{id}`
Obtém um contato específico.

---

#### DELETE `/api/contacts/{id}`
Soft delete de um contato.

---

#### PATCH `/api/contacts/{id}/assign`
Associa um contato a uma empresa (client).

**Request:**
```json
{
  "clientId": 1
}
```

---

#### PATCH `/api/contacts/{id}/unassign`
Desassocia um contato da empresa atual (remove `clientId`).

---

### Mensagens (`/api/messages`) - [Authorize]

#### POST `/api/messages/send`
Envia uma mensagem via WhatsApp.

**Request:**
```json
{
  "jid": "5511999999999@c.us",
  "text": "Olá, tudo bem?"
}
```

**Fluxo:**
1. Validação do JID (não vazio)
2. Sanitiza o número de telefone (remove não-dígitos)
3. Envia requisição HTTP para o serviço Node.js (`POST http://localhost:3333/api/enviar`)
4. Cria registros de Message + Chat no banco (atualiza lastMessageAt, lastMessageBody)
5. Atualiza ou cria o chat (verifica se já existe pelo JID)
6. Dispara evento SignalR `MessageSent`
7. Retorna MessageResponse

---

#### GET `/api/messages`
Lista todas as mensagens.

---

#### GET `/api/messages/{id}`
Obtém uma mensagem específica.

---

#### GET `/api/messages/phone/{phoneNumber}`
Busca mensagens por número de telefone.

---

### Chats / Conversas (`/api/chats`) - [Authorize]

#### GET `/api/chats`
Lista chats com paginação.

**Query Parameters:**
- `page` (int, default 1)
- `pageSize` (int, default 20)

**Response (200):** PaginatedResponse<ChatResponse>

```json
{
  "items": [...],
  "totalCount": 100,
  "page": 1,
  "pageSize": 20,
  "totalPages": 5,
  "hasPrevious": false,
  "hasNext": true
}
```

Cada ChatResponse:
```json
{
  "id": 1,
  "jid": "5511999999999@c.us",
  "phoneNumber": "5511999999999",
  "name": "João",
  "contactId": 1,
  "contactName": "João Silva",
  "clientId": 1,
  "clientName": "Empresa Exemplo",
  "lastMessageAt": "2026-07-21T12:00:00",
  "lastMessageBody": "Olá!",
  "assignedToUserId": null,
  "assignedToUserName": null,
  "unreadCount": 0,
  "createdAt": "2026-07-21T10:00:00"
}
```

**Ordenação:** Do mais recente para o mais antigo (por lastMessageAt).

---

#### GET `/api/chats/{id}`
Obtém um chat específico.

---

#### GET `/api/chats/{id}/messages`
Obtém mensagens de um chat com paginação.

**Query Parameters:**
- `page` (int, default 1)
- `pageSize` (int, default 50)

---

#### GET `/api/chats/{id}/occurrences`
Obtém ocorrências vinculadas a um chat.

---

### Ocorrências (`/api/occurrences`) - [Authorize]

#### POST `/api/occurrences`
Cria uma nova ocorrência vinculada a um chat.

**Request:**
```json
{
  "title": "Problema com pagamento",
  "description": "Cliente reportou erro no boleto",
  "priority": 2,
  "chatId": 1,
  "assignedToUserId": 1
}
```

---

#### GET `/api/occurrences`
Lista todas as ocorrências.

---

#### GET `/api/occurrences/{id}`
Obtém uma ocorrência específica.

---

#### PUT `/api/occurrences/{id}`
Atualiza uma ocorrência (title, description, status, priority, assignedToUserId).

---

#### DELETE `/api/occurrences/{id}`
Soft delete de uma ocorrência.

---

### Tarefas (`/api/tasks`) - [Authorize]

#### POST `/api/tasks`
Cria uma nova tarefa vinculada a uma empresa.

**Request:**
```json
{
  "title": "Revisar contrato",
  "description": "Revisar cláusulas do contrato",
  "priority": 1,
  "dueDate": "2026-08-01T00:00:00",
  "clientId": 1,
  "assignedToUserId": 1
}
```

---

#### GET `/api/tasks`
Lista todas as tarefas.

---

#### GET `/api/tasks/{id}`
Obtém uma tarefa específica.

---

#### PUT `/api/tasks/{id}`
Atualiza uma tarefa.

---

#### DELETE `/api/tasks/{id}`
Soft delete de uma tarefa.

---

#### PATCH `/api/tasks/{id}/status` - [Authorize(Roles = "Admin,Dev")]
Atualiza apenas o status de uma tarefa. Restrito a Admin e Dev.

**Request:**
```json
{
  "status": 1
}
```

---

### Dispositivo (`/api/device`) - [AllowAnonymous]

#### POST `/api/device`
Registra ou atualiza informações do dispositivo WhatsApp conectado.

**Request:**
```json
{
  "jid": "5511999999999@c.us",
  "phoneNumber": "5511999999999",
  "pushName": "João",
  "platform": "android"
}
```

**Endpoint público** (sem autenticação) porque é chamado pelo serviço Node.js interno.

---

#### GET `/api/device`
Obtém informações do dispositivo atual (primeiro registro).

**Endpoint público** pelo mesmo motivo.

---

### Webhook WhatsApp (`/api/webhook/whatsapp`) - [AllowAnonymous]

#### POST `/api/webhook/whatsapp`
Recebe mensagens recebidas do WhatsApp via Node.js bridge.

**Request:**
```json
{
  "from": "5511999999999@c.us",
  "phoneNumber": "5511999999999",
  "body": "Olá, gostaria de saber mais sobre os serviços",
  "timestamp": 1721500000,
  "notifyName": "João Silva",
  "messageType": "Text",
  "hasMedia": false,
  "messageId": "ABEGkR6A...",
  "isForwarded": false
}
```

**Fluxo:**
1. Recebe o payload do Node.js
2. `SaveIncomingMessageUseCase` processa:
   - Dedup por `WhatssAppMessageId` (índice único)
   - Verifica se já existe um chat para este JID
   - Se não existir, cria um novo Chat + (opcionalmente) um Contact
   - Cria o registro de Message (Direction = Incoming; mensagens self-sent/fromMe viram Outgoing)
   - Atualiza lastMessageAt, lastMessageBody no chat
3. Dispara **exatamente um** broadcast SignalR: `MessageSent` se a mensagem for Outgoing, `MessageReceived` se for Incoming
4. Retorna `{ "message": "Notificação enviada para a Web!", "messageId": novoId }`

---

#### POST `/api/webhook/status` - [AllowAnonymous]
Recebe atualização de **status de entrega (ACK)** das mensagens enviadas, reportada pelo Node.js no evento `message_ack`.

**Request:**
```json
{
  "messageId": "true_5511999999999@c.us_3EB0C0A1...",
  "deliveryStatus": 2
}
```

**Fluxo:**
1. Recebe o payload do Node.js (mensagens enviadas — `msg.fromMe` apenas)
2. `UpdateMessageDeliveryStatusUseCase` localiza a mensagem por `WhatssAppMessageId`
3. Atualiza `DeliveryStatus` no banco
4. Dispara evento SignalR `MessageDeliveryStatusChanged` (payload = `MessageDetailResponse`) para o frontend atualizar os indicadores em tempo real
5. Retorna `{ "message": "Status atualizado" | "Mensagem não encontrada" }`

**Mapeamento ACK → DeliveryStatus (no `index.js`):**

| ACK whatsapp-web.js | Valor | DeliveryStatus |
|---|---|---|
| `ACK_ERROR` | -1 | Failed (4) |
| `ACK_PENDING` | 0 | Pending (0) |
| `ACK_SERVER` | 1 | Sent (1) |
| `ACK_DEVICE` | 2 | Delivered (2) |
| `ACK_READ` | 3 | Read (3) |
| `ACK_PLAYED` | 4 | Read (3) |

---

## Camada de Apresentação (Controllers)

Cada controller segue o mesmo padrão:

```csharp
[ApiController]
[Route("api/[controller]")]
public class XxxController : ControllerBase
{
    // Dependências injetadas: use cases + logger
    // Endpoints delegam para use cases
}
```

### AuthController
- Injeta: `ILoginUseCase`, `ILogoutUseCase`, `IRegisterUserUseCase`, `UseCaseLogger`

### ClientsController
- CRUD completo + GetContacts
- Injeta: `ICreateClientUseCase`, `IGetClientsUseCase`, `IUpdateClientUseCase`, `IDeleteClientUseCase`, IClientRepository (para contacts)

### ContactsController
- CRUD + Assign/Unassign
- Injeta: `ICreateContactUseCase`, `IGetContactsUseCase`, `IDeleteContactUseCase`, `IAssignContactUseCase`

### MessagesController
- Send + List + GetById + GetByPhoneNumber
- Injeta: `ISendMessageUseCase`, `IGetMessagesUseCase`

### ChatsController
- List paginado + GetById + GetMessages + GetOccurrences
- Injeta: `ICreateChatUseCase`, `IGetChatsUseCase`, IChatRepository, IMessageRepository, IOccurrenceRepository

### OccurrencesController
- CRUD completo
- Injeta: `ICreateOccurrenceUseCase`, `IGetOccurrencesUseCase`, `IUpdateOccurrenceUseCase`, `IDeleteOccurrenceUseCase`

### TasksController
- CRUD + UpdateStatus (Admin/Dev only)
- Injeta: `ICreateTaskUseCase`, `IGetTasksUseCase`, `IUpdateTaskUseCase`, `IDeleteTaskUseCase`, `IUpdateTaskStatusUseCase`

### DeviceController
- SaveDevice + GetCurrentDevice (AllowAnonymous)
- Injeta: `ISaveDeviceUseCase`, IDeviceRepository

### WebhookController
- ReceiveMessage (AllowAnonymous) — `POST /api/webhook/whatsapp`
- ReceiveDeliveryStatus (AllowAnonymous) — `POST /api/webhook/status`
- Injeta: `ISaveIncomingMessageUseCase`, `IUpdateMessageDeliveryStatusUseCase`

---

## Camada de Use Cases (Casos de Uso)

Cada use case encapsula **uma única operação de negócio**. Eles são registrados como `AddScoped` no Program.cs.

### AuthUseCases

#### LoginUseCase
1. Busca usuário por nome (`IUserRepository.GetByName`)
2. Verifica senha com `BCrypt.Verify` (`PasswordHelper`); senhas legadas em texto puro são migradas para hash no primeiro login
3. Verifica `IsActive`
4. Gera token JWT via `TokenService.GenerateToken`
5. Retorna `LoginResponse` com token + dados do usuário

#### LogoutUseCase
1. Extrai JTI do token atual via `HttpContext`
2. Adiciona JTI ao `TokenBlacklistService`
3. Retorna mensagem de sucesso

#### RegisterUserUseCase
1. Valida tamanho mínimo da senha (`Auth:PasswordMinLength`)
2. Se `Auth:RequireRegistrationCode=true` (default), normaliza o código (`Trim().ToUpperInvariant()`) e valida com `RegistrationCode.IsValid()` (não usado + não expirado); se inválido → erro
3. Verifica se nome de usuário já existe
4. Cria entidade User com Role padrão (`Auth:DefaultUserRole`) e senha com **hash BCrypt**
5. Salva via `IUserRepository`
6. Marca o código de permissão como usado (`MarkAsUsed(userId)`)
7. Cria audit log
8. Retorna UserResponse

#### GenerateRegistrationCodeUseCase
1. Gera `count` códigos hex aleatórios (64 bytes, `RandomNumberGenerator`) — até 10 tentativas contra colisão (`ExistsAsync`)
2. Define `ExpiresAt = UtcNow + Auth:RegistrationCodeExpiryHours`
3. Salva via `IRegistrationCodeRepository`
4. Retorna `RegistrationCodeResponse[]`

### ClientUseCases

#### CreateClientUseCase
1. Mapeia `CreateClientRequest` para entidade `Client` (com `Status` padrão `Active` se não informado)
2. Salva via `IClientRepository`
3. Gera audit log
4. Retorna `ClientResponse`

#### GetClientsUseCase
1. Busca todos os clients via `IClientRepository.GetAll()` (inclui contagem de contacts)
2. Mapeia para `ClientResponse[]`
3. Retorna lista

#### UpdateClientUseCase
1. Busca client por ID (lança KeyNotFoundException se não existir)
2. Atualiza campos: Name, MainPhoneNumber, Status
3. Salva via repositório
4. Gera audit log
5. Retorna `ClientResponse`

#### DeleteClientUseCase
1. Busca client por ID
2. Marca `IsDeleted = true`
3. Salva via repositório
4. Gera audit log

### ContactUseCases

#### CreateContactUseCase
1. Mapeia request para entidade Contact
2. Se fornecido `clientId`, busca e associa o Client
3. Salva via `IContactRepository`
4. Gera audit log
5. Retorna ContactResponse

#### GetContactsUseCase
1. Busca todos os contatos via `IContactRepository.GetAll()`
2. Mapeia para ContactResponse[] (inclui nome do client, nome do grupo)
3. Retorna lista

#### DeleteContactUseCase
1. Busca contato por ID
2. Marca `IsDeleted = true`
3. Salva
4. Gera audit log

#### AssignContactUseCase
1. Busca contato por ID
2. Se `clientId` for nulo, apenas desassocia (`contact.ClientId = null`)
3. Caso contrário, busca o Client e associa
4. Salva
5. Gera audit log

### MessageUseCases

#### SendMessageUseCase
1. Valida JID (não vazio)
2. Sanitiza phoneNumber (strip non-digits)
3. Envia requisição HTTP POST para `Messageria:BaseUrl/api/enviar` (strategy por tipo de mensagem)
4. Busca ou cria Chat para o JID (se não existir, cria)
5. Cria registro de Message com Direction = Outgoing (DeliveryStatus = Pending)
6. Atualiza `lastMessageAt` e `lastMessageBody` no Chat
7. Dispara SignalR `MessageSent` (payload = MessageDetailResponse) — o broadcast é suprimido se a mensagem já tiver sido salva pelo webhook (self-sent), para evitar duplicidade de eventos
8. Gera audit log
9. Retorna MessageResponse

#### SaveIncomingMessageUseCase
1. Recebe WhatsAppWebhookDto (dedup por `WhatssAppMessageId`)
2. Busca Chat por JID
3. Se não existir Chat, cria um novo (e opcionalmente um Contact se não for grupo)
4. Cria Message — Direction = Incoming (ou Outgoing se `fromMe`) com DeliveryStatus = Delivered (incoming) / Pending (outgoing)
5. Atualiza Chat (lastMessageAt, lastMessageBody)
6. Se for a primeira mensagem, atualiza `Contact.LastMessageAt`
7. Dispara **exatamente um** broadcast SignalR por mensagem: `MessageSent` (outgoing) ou `MessageReceived` (incoming)
8. Retorna o ID da mensagem criada

#### UpdateMessageDeliveryStatusUseCase
1. Recebe `{ messageId, deliveryStatus }` do endpoint `/api/webhook/status`
2. Localiza a Message por `WhatssAppMessageId`
3. Atualiza `DeliveryStatus` no banco
4. Dispara SignalR `MessageDeliveryStatusChanged` (payload = MessageDetailResponse)
5. Retorna a mensagem atualizada (ou `null` se não encontrada)

#### GetMessagesUseCase
1. Se `chatId` for fornecido: busca mensagens paginadas por ChatId (ordenado por timestamp ASC)
2. Se `chatId` for nulo: busca todas mensagens paginadas
3. Se `phoneNumber` for fornecido: busca mensagens por número
4. Mapeia para PaginatedResponse<MessageResponse>

### ChatUseCases

#### CreateChatUseCase
1. Mapeia `CreateChatRequest` para entidade Chat
2. Se fornecido `contactId`, busca e associa Contact
3. Salva via `IChatRepository`
4. Gera audit log
5. Retorna ChatResponse

#### GetChatsUseCase
1. Busca chats paginados via `IChatRepository.GetAllPagedAsync()` (ordenado por lastMessageAt DESC)
2. Mapeia para `PaginatedResponse<ChatResponse>` (com contactName, clientName, etc.)
3. Retorna

### OccurrenceUseCases (mesmo padrão CRUD)

### TaskUseCases (mesmo padrão CRUD + UpdateStatus)

#### UpdateTaskStatusUseCase
1. Busca tarefa por ID
2. Atualiza Status com o valor recebido
3. Salva
4. Gera audit log

---

## Camada de Repositories (Repositórios)

Cada repositório implementa uma interface e estende operações CRUD básicas:

```csharp
public class XxxRepository : IXxxRepository
{
    private readonly AppDbContext _context;
    
    // GetAll(), GetById(), Add(), Update(), Delete()
    // Consultas específicas adicionais
}
```

### Repositórios e suas Consultas Específicas

| Repositório | Consultas Específicas |
|---|---|
| ChatRepository | `GetAllPagedAsync(page, pageSize)` com Includes (Contact, Client, AssignedUser), `GetByIdWithIncludes`, `GetByJid` |
| ClientRepository | `GetAll` com contagem de Contacts |
| ContactRepository | `GetAllWithIncludes` (Client, Group) |
| MessageRepository | `GetByChatIdPagedAsync`, `GetByPhoneNumber` |
| OccurrenceRepository | `GetByChatId` |
| UserRepository | `GetByName` |
| DeviceRepository | `GetCurrentAsync` (primeiro registro) |
| GroupRepository | Básico |
| ClientTaskRepository | Básico |

---

## Camada de Serviços (Services)

### TokenService
- **Registro**: Singleton
- **Gera token JWT**: Cria `JwtSecurityToken` com:
  - Claims: `NameIdentifier` (userId), `Name` (userName), `Role` (userRole)
  - Issuer, Audience das configurações
  - Expiração: **8 horas** (hardcoded)
  - Assinatura: HMAC-SHA256 com secret do appsettings
- Retorna token como string

### TokenBlacklistService
- **Registro**: Singleton
- Armazena JTIs revogados em um `ConcurrentDictionary<string, DateTime>`
- Métodos: `Revoke(jti, expiry)`, `IsRevoked(jti)`
- Limpeza automática: na verificação, remove entradas expiradas
- Usado no evento `OnTokenValidated` do JWT middleware

### AuditService
- Método estático `GenerateAuditLogs()` chamado no `SaveChangesAsync` do `AppDbContext`
- Detecta entidades com `EntityState.Added`, `Modified`, `Deleted`
- Extrai `OldValues`, `NewValues` como JSON
- Extrai `UserId` do `HttpContext` (via `IHttpContextAccessor`)
- Cria entradas `AuditLog` no banco

### UseCaseLogger
- **Registro**: Scoped
- Cria e salva `AuditLog` para operações específicas
- Também transmite via SignalR: `await hubContext.Clients.All.SendAsync("LogReceived", auditLog)`
- Métodos: `LogAsync(entityType, entityId, action, description)`

### WhatsappHub
- SignalR Hub vazio (herda de `Hub`)
- Os eventos são enviados do servidor para os clientes via `IHubContext<WhatsappHub>`
- Eventos emitidos:
  - `LogReceived` - Pelo UseCaseLogger
  - `MessageSent` - Pelo SendMessageUseCase / SaveIncomingMessageUseCase (outgoing)
  - `MessageReceived` - Pelo SaveIncomingMessageUseCase (incoming)
  - `MessageDeliveryStatusChanged` - Pelo UpdateMessageDeliveryStatusUseCase

---

## WebSocket / SignalR

O hub SignalR está disponível em `/whatsappHub` e usa **transporte WebSocket** com fallback para outros transportes.

### Eventos do Servidor para Cliente

| Evento | Disparado Por | Payload |
|---|---|---|
| `MessageReceived` | SaveIncomingMessageUseCase (incoming) | MessageDetailResponse |
| `MessageSent` | SendMessageUseCase / SaveIncomingMessageUseCase (outgoing) | MessageDetailResponse |
| `MessageDeliveryStatusChanged` | UpdateMessageDeliveryStatusUseCase | MessageDetailResponse |
| `LogReceived` | UseCaseLogger | AuditLog |

### Conexão do Cliente

O cliente front-end se conecta via `@microsoft/signalr`:

```typescript
const connection = new signalR.HubConnectionBuilder()
  .withUrl("http://localhost:5261/whatsappHub", {
    accessTokenFactory: () => localStorage.getItem("token")
  })
  .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
  .build();
```

A autenticação é feita via query string `access_token` que o SignalR anexa automaticamente.

---

## Bridge Node.js (Messageria)

O diretório `messageria/` contém um servidor Node.js independente que age como ponte entre a API .NET e o WhatsApp Web.

### index.js - Funcionamento Completo

```javascript
import { Client, LocalAuth, MessageMedia } from "whatsapp-web.js";
import express from "express";
import axios from "axios";
import qrcode from "qrcode-terminal";
```

**Inicialização:**
1. Cria cliente `whatsapp-web.js` com autenticação local (`LocalAuth`) - salva sessão em `./.wwebjs_auth/`
2. Configura `puppeteer.headless: true` com `executablePath` do Chrome (`CHROME_PATH`)
3. Porta `3333` (env `PORT`); URLs da API .NET configuráveis via env

**Eventos do WhatsApp:**

| Evento | Ação |
|---|---|
| `qr` | Gera QR code no terminal para autenticação |
| `authenticated` / `ready` | Loga conexão; envia POST `/api/device` com os dados do dispositivo |
| `message_create` | Envia mensagens (recebidas e enviadas) via POST `/api/webhook/whatsapp` |
| `message_ack` | Envia status de entrega via POST `/api/webhook/status` (apenas `fromMe`) |

**Processamento de mensagens (`processarEMandarParaAspNet`):**
1. Ignora grupos, newsletters e status ("@g.us", "@newsletter", "@broadcast")
2. Extrai: from/to (jid), body, timestamp, notifyName, hasMedia, type, isForwarded, id (`_serialized`/`$1`)
3. Para mídia: baixa via `message.downloadMedia()`, converte para base64
4. Monta payload e envia via axios para `ASPNET_WEBHOOK_URL` com **retry (3 tentativas)** — o backend deduplica por messageId
5. Correlaciona envios via API com o `message_create` (fila `sendsAguardandoId`) para obter o ID real e evitar duplicação

**Endpoints Express:**

| Método | Rota | Função |
|---|---|---|
| POST | `/api/enviar` | Recebe `{ jid, mensagem, type, mediaBase64, ... }` da API .NET e envia via `client.sendMessage()` |
| POST | `/api/sync` | Sincroniza mensagens recentes dos chats existentes (dedup no backend) |
| GET | `/` | Healthcheck "WhatsApp Bridge Online" |

**Inicialização do servidor:**
- Express escuta na porta 3333 (env `PORT`)
- Se a porta já estiver em uso, o processo encerra (evita disputa pela sessão do navegador)

### Fluxo de Mensagens

```
┌──────────┐     HTTP POST      ┌──────────────┐
│  Node.js  │ ──────────────────▶ │  API .NET    │
│ (whatsapp │   /api/webhook/    │  /api/webhook │
│  web.js)  │   whatsapp         │  /whatsapp    │
│           │                    │              │
│  Porta    │ ◀────────────────── │  Porta 5261  │
│  3333     │   HTTP POST        │              │
│           │   /api/enviar      │              │
└──────────┘                    └──────────────┘
     │                                │
     │ WhatsApp Web                   │ SignalR
     ▼                                ▼
  ┌──────────┐                 ┌──────────────┐
  │ WhatsApp │                 │  Front-end   │
  │  Web     │                 │  (React)     │
  └──────────┘                 └──────────────┘
```

---

## Autenticação e Autorização

### Configuração JWT (Program.cs)

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            RoleClaimType = ClaimTypes.Role,
            ClockSkew = TimeSpan.Zero
        };
        
        // Evento: verifica blacklist
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var jti = context.Principal?.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
                var blacklistService = context.HttpContext.RequestServices
                    .GetRequiredService<TokenBlacklistService>();
                if (jti != null && blacklistService.IsRevoked(jti))
                    context.Fail("Token revogado.");
            }
        };
    });
```

### Configuração SignalR com JWT

```csharp
builder.Services.AddSignalR()
    .AddHubOptions<WhatsappHub>(options =>
    {
        options.AddFilter<AuthFilter>(); // Filtro de autorização personalizado
    });
```

O `AuthFilter` verifica o token JWT na query string (enviado automaticamente pelo SignalR).

### Hierarquia de Roles

| Role | Nível | Acesso Especial |
|---|---|---|
| Support | 0 | CRUD básico, não pode alterar status de tarefas |
| Dev | 1 | Pode alterar status de tarefas |
| Admin | 2 | Pode alterar status de tarefas, controle total |

### Endpoints Públicos (AllowAnonymous)
- `POST /api/auth/register`
- `POST /api/auth/login`
- `POST /api/device`
- `GET /api/device`
- `POST /api/webhook/whatsapp`
- `POST /api/webhook/status`

### Endpoints com Restrição de Role
- `PATCH /api/tasks/{id}/status` - Requer Admin ou Dev
- `POST /api/auth/codes` - Requer Admin ou Dev
- `GET/PUT /api/users` - GET: autenticado; PUT: Admin ou Dev
- `GET/PUT/POST /api/admin/config` - Requer Admin ou Dev

---

## Auditoria

### Auditoria Automática (DbContext)

O `AppDbContext` sobrescreve `SaveChangesAsync`:

```csharp
public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
{
    var entries = ChangeTracker.Entries()
        .Where(e => e.Entity is BaseEntity && 
                    (e.State == EntityState.Added || e.State == EntityState.Modified));
    
    foreach (var entry in entries)
    {
        var entity = (BaseEntity)entry.Entity;
        entity.LastUpdate = DateTime.UtcNow;
        if (entry.State == EntityState.Added)
            entity.CreatedAt = DateTime.UtcNow;
    }
    
    await AuditService.GenerateAuditLogs(this, _httpContextAccessor, cancellationToken);
    return await base.SaveChangesAsync(cancellationToken);
}
```

### Auditoria Manual (UseCaseLogger)

O `UseCaseLogger` permite logging manual:
1. Cria `AuditLog` com tipo, ID, ação e descrição
2. Salva no banco
3. Transmite em tempo real via SignalR (`LogReceived`)

### Tabela AuditLog

| Campo | Exemplo |
|---|---|
| UserId | 1 |
| UserName | "joao" |
| UserRole | "Admin" |
| EntityType | "Client" |
| EntityId | "5" |
| Action | "Create" |
| Description | "Cliente 'Empresa X' criado" |
| OldValues | null |
| NewValues | "{\"Name\":\"Empresa X\",\"Status\":0}" |
| Timestamp | 2026-07-21T12:00:00 |

---

## Fluxos de Dados Principais

### 1. Recebimento de Mensagem (Tempo Real)

```
WhatsApp Web 
    → whatsapp-web.js (Node.js)
    → Evento 'message_create' / 'message'
    → POST /api/webhook/whatsapp (API .NET)
    → SaveIncomingMessageUseCase (dedup por WhatssAppMessageId)
        → Busca/Cria Chat
        → Cria Message (Incoming | Outgoing se fromMe)
        → Atualiza Chat (lastMessageAt, lastMessageBody)
    → SignalR 'MessageReceived' (incoming) | 'MessageSent' (outgoing)
    → Clientes Web recebem em tempo real
```

### 2. Envio de Mensagem + Status de Entrega

```
Usuário Web
    → POST /api/messages/send (API .NET)
    → SendMessageUseCase
        → Valida JID
        → Envia HTTP POST para Node.js (/api/enviar)
        → Node.js → client.sendMessage() → WhatsApp Web
        → Cria Message (Outgoing, DeliveryStatus=Pending) + Chat no banco
        → SignalR 'MessageSent'
    → Clientes Web recebem confirmação
    → WhatsApp muda o ACK (enviada → entregue → lida)
    → Node.js evento 'message_ack' → POST /api/webhook/status { messageId, deliveryStatus }
    → UpdateMessageDeliveryStatusUseCase
        → Atualiza DeliveryStatus no banco
        → SignalR 'MessageDeliveryStatusChanged'
    → Frontend atualiza o indicador (✓ → ✓✓ → ✓✓ azul)
```

### 3. Login

```
Usuário
    → POST /api/auth/login
    → LoginUseCase
        → Busca User por nome
        → Compara senha (BCrypt; legadas migradas no 1º login)
        → Verifica IsActive
        → TokenService.GenerateToken()
    → Retorna token JWT + UserResponse
```

### 4. Logout

```
Usuário
    → POST /api/auth/logout (com token JWT)
    → LogoutUseCase
        → Extrai JTI do token
        → Adiciona à TokenBlacklistService
    → Token não pode mais ser usado
```

### 5. Criação de Tarefa

```
Usuário
    → POST /api/tasks (com token JWT)
    → TasksController
    → CreateTaskUseCase
        → Valida dados
        → Cria ClientTask no banco
        → UseCaseLogger.LogAsync() (auditoria + SignalR)
    → Retorna TaskResponse
```

---

## Configuração e Execução

### Pré-requisitos

- .NET 10.0 SDK
- Node.js 18+
- PostgreSQL 14+ (banco principal)
- Navegador Chrome/Chromium (para whatsapp-web.js)

### Variáveis de Conexão (appsettings.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=multiwhats_db;Username=postgres;Password=12345678;",
    "Xampp": "Server=localhost;Port=3306;Database=multiwhats;User=root;Password=12345678;"
  },
  "JwtSettings": {
    "Secret": "SuaChaveSecretaSuperPoderosaEALeatoriaComMaisDe32Caracteres!",
    "Issuer": "MinhaApiEmissor",
    "Audience": "MeuAppCliente",
    "ExpiryInMinutes": 60
  },
  "Auth": {
    "PasswordMinLength": 6,
    "RequireRegistrationCode": false,
    "RegistrationCodeExpiryHours": 48,
    "DefaultUserRole": "Support"
  },
  "Messageria": { "BaseUrl": "http://localhost:3333" }
}
```

> A connection string `Xampp` (MySQL) é apenas para o sync legado (desativado no código). O banco principal é **PostgreSQL**.

### Executando

**Opção 1: Via npm (raiz)**
```bash
npm start
```
Roda ambos os serviços simultaneamente via `concurrently`:
- `npm run start:messageria` → Node.js na porta 3333
- `npm run start:api` → .NET na porta 5261 (http)

**Opção 2: Separadamente**
```bash
# Terminal 1 - API .NET
cd multiwhats-api
dotnet run

# Terminal 2 - Node.js
cd multiwhats-api/messageria
npm start
```

### Endpoints por Ambiente

| Serviço | Ambiente | URL |
|---|---|---|
| API .NET | Desenvolvimento | http://localhost:5261 |
| API .NET | HTTPS | https://localhost:7069 |
| Node.js | - | http://localhost:3333 |
| SignalR Hub | - | http://localhost:5261/whatsappHub |
| Swagger | Desenvolvimento | http://localhost:5261/swagger |

---

## Banco de Dados

### Provider

**PostgreSQL** via `Npgsql.EntityFrameworkCore.PostgreSQL` versão 10.0.3.

A connection string `DefaultConnection` aponta para o PostgreSQL (banco principal). A string `Xampp` (MySQL) existe apenas para o sync legado, desativado no código.

### Migrations

Para recriar o banco:
```bash
dotnet ef database update
```

Para gerar nova migration:
```bash
dotnet ef migrations add NomeDaMigration
```

### Comportamentos do EF Core

- **Soft Delete Global**: `HasQueryFilter(e => !e.IsDeleted)` aplicado em `BaseEntity`
- **Cascading**: `OnDelete(DeleteBehavior.SetNull)` para FKs opcionais, `Cascade` para obrigatórias
- **Timestamps Automáticos**: `CreatedAt` e `LastUpdate` atualizados no `SaveChangesAsync`
- **Índices**: JID único em Contacts e Chats; User.Name único; `WhatssAppMessageId` único em Messages; Code único em RegistrationCodes
- **Seed**: `SystemConfigService.SeedDefaultParametersAsync()` insere os parâmetros padrão em `system_parameters` no startup

---

## API Documentation (Swagger)

Disponível em ambiente de desenvolvimento:
- URL: `http://localhost:5261/swagger`
- Usa `Swashbuckle.AspNetCore` com security definition Bearer

### Configuração

```csharp
builder.Services.AddSwaggerGen();
// ...
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
```
