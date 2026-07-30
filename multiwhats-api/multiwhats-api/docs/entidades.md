# Entidades

## Diagrama de Relacionamentos

```
Client ──1:N── Contact ──1:N── Message
  │                │
  │                └──1:N── Occurrence
  │
  └──1:N── ClientTask

Group ──1:N── Contact

User ──(criado/alterado por)── AuditLog
```

## BaseEntity

Todas as entidades (exceto `Device` e `AuditLog`) herdam de `BaseEntity`:

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

## Message (Mensagem)

Mensagem enviada ou recebida no WhatsApp.

| Campo | Tipo | Descrição |
|---|---|---|
| Id | int | PK |
| MessageId | string? | ID serializado WhatsApp (dedup) |
| FromJid | string | JID do remetente |
| ToJid | string? | JID do destinatário |
| PhoneNumber | string | Número limpo |
| Body | string? | Texto da mensagem |
| Direction | MessageDirection | `Incoming` \| `Outgoing` |
| Type | MessageType | `Text` \| `Image` \| `Audio` \| `Video` \| `Document` \| `Sticker` \| `Contact` \| `Location` \| `Unknown` |
| Timestamp | long | Unix timestamp |
| SentAt | DateTime | DateTime do envio/recebimento |
| NotifyName | string? | Nome de notificação |
| HasMedia | bool | Se possui mídia |
| MediaUrl | string? | Base64 ou URL da mídia (**LONGTEXT**) |
| MediaMimeType | string? | Tipo MIME |
| MediaFilename | string? | Nome do arquivo |
| MediaSize | long? | Tamanho em bytes |
| MediaCaption | string? | Legenda da mídia |
| DeliveryStatus | DeliveryStatus | `Pending` \| `Sent` \| `Delivered` \| `Read` \| `Failed` |
| IsForwarded | bool | Se foi encaminhada |
| ContactId | int? | FK → Contact |
| UserId | int? | FK → User (quem enviou) |
| OccurrenceId | int? | FK → Occurrence (vínculo opcional) |
| ReplyToId | int? | FK → Message (resposta a) |

**Relacionamentos:**
- `N:1` → Contact
- `N:1` → User
- `N:1` → Occurrence
- `N:1` → Message (ReplyTo — set null on delete)
- `1:N` → Chat (via Chat.LastMessage)

**Índices:** INDEX em `MessageId`, `PhoneNumber`, `Timestamp`.

**Cascade:** Delete → Chat (cascade), ReplyTo → Message (set null).

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
| Password | string | Senha (⚠️ armazenada em texto puro) |
| Role | UserRole | `Support` \| `Dev` \| `Admin` |
| IsActive | bool | Se a conta está ativa |

**Índices:** UNIQUE em `Name`.

---

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
| `Initial` | 2026-07-23 | Schema completo inicial |
| `AddLastMessageTypeToChat` | 2026-07-24 | Adiciona LastMessage navigation ao Chat |
