# Entidades

## Diagrama de Relacionamentos

```
Client ──1:N── Contact ──1:N── Chat ──1:N── Message
  │                │            │
  │                └──1:N── Occurrence
  │
  └──1:N── ClientTask

Group ──1:N── Contact

User ──(criado/alterado por)── AuditLog
User ──1:N── RegistrationCode (criado por)
User ──(atualizado por)── SystemParameter
```

## BaseEntity

Todas as entidades (exceto `Device`, `AuditLog` e `RegistrationCode`) herdam de `BaseEntity`:

| Campo | Tipo | Descrição |
|---|---|---|
| Id | int (PK) | Identificador único |
| CreatedAt | DateTime | Data de criação (auto) |
| LastUpdate | DateTime | Última atualização (auto) |
| IsDeleted | bool | Soft delete flag |
| CreatedByUserId | int? | FK → User (quem criou) |
| LastUpdatedByUserId | int? | FK → User (quem atualizou) |

---

## Client (Empresa/Cliente)

Representa uma empresa ou cliente do sistema.

| Campo | Tipo | Descrição |
|---|---|---|
| Id | int | PK |
| Name | string | Nome da empresa (obrigatório) |
| MainPhoneNumber | string? | Telefone principal |
| Status | ClientStatus | `Active` \| `Inactive` |

**Relacionamentos:**
- `1:N` → Contacts (um cliente tem vários contatos)
- `1:N` → ClientTasks (demandas da empresa)

**Índices:** Nenhum único adicional.

---

## Contact (Número de WhatsApp)

Representa um contato/telefone no WhatsApp.

| Campo | Tipo | Descrição |
|---|---|---|
| Id | int | PK |
| Jid | string | ID WhatsApp (`5511999999999@c.us`) **UNIQUE** |
| PhoneNumber | string | Número limpo (`5511999999999`) |
| Name | string? | Nome salvo na agenda |
| PushName | string? | Nome do perfil WhatsApp |
| ProfilePicUrl | string? | URL da foto de perfil |
| IsBlocked | bool | Se está bloqueado |
| IsGroup | bool | Se é grupo |
| LastMessageAt | DateTime? | Última mensagem recebida |
| ClientId | int? | FK → Client (atrelável via PATCH) |
| GroupId | int? | FK → Group |

**Relacionamentos:**
- `N:1` → Client (opcional)
- `N:1` → Group (opcional)
- `1:N` → Messages
- `1:N` → Occurrences (chamados)

**Índices:** UNIQUE em `Jid`, INDEX em `PhoneNumber`.

---

## Chat (Conversa)

| Campo | Tipo | Descrição |
|---|---|---|
| Id | int | PK |
| Jid | string | JID da conversa (`5511999999999@c.us`) **UNIQUE** |
| PhoneNumber | string? | Número limpo |
| Name | string? | Nome do contato/chat |
| ContactId | int? | FK → Contact |
| ClientId | int? | FK → Client |
| LastMessageAt | DateTime? | Última mensagem (timestamp) |
| LastMessageId | int? | FK → Message (última mensagem) |
| AssignedToUserId | int? | FK → User (responsável) |

**Relacionamentos:**
- `N:1` → Contact, Client, User (AssignedTo)
- `1:N` → Messages, Occurrences

---

## Message (Mensagem)

Mensagem enviada ou recebida no WhatsApp. Toda mensagem pertence a um `Chat`.

| Campo | Tipo | Descrição |
|---|---|---|
| Id | int | PK |
| WhatssAppMessageId | string? | ID serializado do WhatsApp (**UNIQUE**, usado no dedup e no webhook de status) |
| FromJid | string | JID do remetente |
| ToJid | string? | JID do destinatário |
| PhoneNumber | string? | Número limpo |
| Body | string? | Texto da mensagem |
| Direction | MessageDirection | `Incoming` \| `Outgoing` |
| Type | MessageType | `Text` \| `Image` \| `Audio` \| `Video` \| `Document` \| `Sticker` \| `Contact` \| `Location` \| `Unknown` |
| Timestamp | long | Unix timestamp |
| SentAt | DateTime | DateTime do envio/recebimento |
| NotifyName | string? | Nome de notificação |
| HasMedia | bool | Se possui mídia |
| MediaUrl | string? | Base64 ou URL da mídia (TEXT) |
| MediaMimeType | string? | Tipo MIME |
| MediaFilename | string? | Nome do arquivo |
| MediaSize | long? | Tamanho em bytes |
| MediaCaption | string? | Legenda da mídia |
| DeliveryStatus | DeliveryStatus | `Pending` \| `Sent` \| `Delivered` \| `Read` \| `Failed` (default: Pending para outgoing, Delivered para incoming) |
| IsForwarded | bool | Se foi encaminhada |
| Source | MessageSource | `Phone` \| `System` (corrigido no webhook de envio) |
| FromMe | bool | Se foi enviada pelo próprio número |
| ChatId | int | FK → Chat (obrigatório) |
| UserId | int? | FK → User (quem enviou) |
| OccurrenceId | int? | FK → Occurrence (vínculo opcional) |
| ReplyToId | int? | FK → Message (resposta a) |

**Relacionamentos:**
- `N:1` → Chat, Contact (via Chat), User, Occurrence
- `N:1` → Message (ReplyTo — set null on delete)
- `1:N` → Chat (via Chat.LastMessage)

**Índices:** UNIQUE em `WhatssAppMessageId`; INDEX em `PhoneNumber`, `Timestamp`.

---

## Occurrence (Chamado)

Chamado/ticket vinculado a um contato.

| Campo | Tipo | Descrição |
|---|---|---|
| Id | int | PK |
| Title | string | Título do chamado |
| Description | string? | Descrição detalhada |
| Status | OccurrenceStatus | `Open` \| `InProgress` \| `Resolved` \| `Closed` |
| Priority | Priority | `Low` \| `Medium` \| `High` \| `Urgent` |
| ContactId | int | FK → Contact (obrigatório) |
| AssignedToUserId | int? | FK → User (responsável) |

**Relacionamentos:**
- `N:1` → Contact
- `N:1` → User (atribuído)
- `1:N` → Messages

---

## ClientTask (Demanda da Empresa)

Tarefa/demand vinculada a um cliente.

| Campo | Tipo | Descrição |
|---|---|---|
| Id | int | PK |
| Title | string | Título da tarefa |
| Description | string? | Descrição detalhada |
| Status | ClientTaskStatus | `Open` \| `InProgress` \| `Completed` \| `Cancelled` |
| Priority | Priority | `Low` \| `Medium` \| `High` \| `Urgent` |
| DueDate | DateTime? | Data de vencimento |
| ClientId | int | FK → Client |
| AssignedToUserId | int? | FK → User (responsável) |

**Relacionamentos:**
- `N:1` → Client
- `N:1` → User (atribuído)

---

## Group (Grupo WhatsApp)

Representa um grupo no WhatsApp.

| Campo | Tipo | Descrição |
|---|---|---|
| Id | int | PK |
| Name | string | Nome do grupo |
| Description | string? | Descrição |
| WhatsAppGroupId | string? | ID do grupo no WhatsApp |

**Relacionamentos:**
- `1:N` → Contacts (membros do grupo)

---

## User (Usuário do Sistema)

Usuário que acessa a API.

| Campo | Tipo | Descrição |
|---|---|---|
| Id | int | PK |
| Name | string | Nome de login **UNIQUE** |
| Password | string | Senha com hash **BCrypt** (work factor 12; legadas migram no 1º login) |
| Role | UserRole | `Support` \| `Dev` \| `Admin` |
| IsActive | bool | Se a conta está ativa |

**Índices:** UNIQUE em `Name`.

---

## RegistrationCode (Código de Permissão)

Código de cadastro gerado por Admin/Dev para criar novos usuários.

| Campo | Tipo | Descrição |
|---|---|---|
| Id | int | PK |
| Code | string | Valor hex (64 bytes aleatórios via `RandomNumberGenerator`), **UNIQUE** |
| IsUsed | bool | Se já foi usado |
| UsedByUserId | int? | FK → User (quem usou) |
| CreatedByUserId | int | FK → User (quem gerou) |
| ExpiresAt | DateTime | Validade (UTC + `Auth:RegistrationCodeExpiryHours`) |
| CreatedAt | DateTime | Data de criação |

**Regras:** o registro valida `!IsUsed && ExpiresAt > now`; ao usar, `MarkAsUsed(userId)` é chamado dentro da mesma transação. Não herda de `BaseEntity`.

---

## SystemParameter (Parâmetro do Sistema)

Configurações globais editáveis em runtime (via `/api/admin/config`).

| Campo | Tipo | Descrição |
|---|---|---|
| Id | int | PK |
| Key | string | Chave única (ex: `Auth:PasswordMinLength`) |
| Value | string? | Valor atual |
| Type | string | `String` \| `Int` \| `Bool` \| `JsonList` |
| Group | string? | Categoria (Auth, Business, Media, Replies, Occurrence) |
| Description | string? | Descrição |
| IsRequired | bool | Se o valor é obrigatório |
| UpdatedByUserId | int? | FK → User (última alteração) |

Seed inicial em `SystemConfigService.SeedDefaultParametersAsync()`.

## Device (Dispositivo Conectado)

Informações sobre o dispositivo WhatsApp conectado.

| Campo | Tipo | Descrição |
|---|---|---|
| Id | int | PK |
| Jid | string? | JID do dispositivo |
| PhoneNumber | string? | Número conectado |
| PushName | string? | Nome do dispositivo |
| Platform | string? | Plataforma (Android, iOS, Web) |
| LastSeen | DateTime? | Último visto |

**Nota:** Não herda de `BaseEntity`. Singleton — apenas um registro ativo.

---

## AuditLog (Log de Auditoria)

Registro de todas as alterações feitas no sistema.

| Campo | Tipo | Descrição |
|---|---|---|
| Id | int | PK |
| UserId | int? | ID do usuário |
| UserName | string? | Nome do usuário |
| UserRole | string? | Role do usuário |
| EntityType | string | Nome da entidade (`"Contact"`, `"Message"`) |
| EntityId | int? | ID da entidade |
| Action | string | `Created` \| `Updated` \| `Deleted` |
| Description | string | Descrição legível |
| OldValues | string? | JSON com valores antigos |
| NewValues | string? | JSON com valores novos |
| Timestamp | DateTime | Data/hora da operação |

**Nota:** Não herda de `BaseEntity`. Criado automaticamente pelo `AuditService`.

**Índices:** INDEX em `Timestamp`, `EntityType`.

---

## Migrations

| Migration | Data | Descrição |
|---|---|---|
| `20260730170246_InitialCreate` | 2026-07-30 | Schema completo inicial (PostgreSQL) |
| `20260730201519_AddUniqueIndexOnWhatsAppMessageId` | 2026-07-30 | Índice único em `Message.WhatssAppMessageId` (dedup) |
| `20260730203228_FIXDUPLICATEDMESSAGES` | 2026-07-30 | Correção de duplicação de mensagens |
| `20260731010540_AddSystemParameters` | 2026-07-31 | Entidade `SystemParameter` |
| `20260731011146_AddSettingsToBD` | 2026-07-31 | Seed dos parâmetros padrão |
| `20260731133730_AddMessageSourceAndFromMe` | 2026-07-31 | Campos `Source` e `FromMe` em Message |
