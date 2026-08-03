# Enums

Todos os enums são armazenados no banco como **strings** (ex: `"Active"`, não `0`).

---

## MessageDirection

Direção da mensagem WhatsApp.

| Valor | Descrição |
|---|---|
| `Incoming` | Mensagem recebida do WhatsApp |
| `Outgoing` | Mensagem enviada via API |

**Uso:** `Message.Direction`

---

## MessageType

Tipo de conteúdo da mensagem.

| Valor | Descrição |
|---|---|
| `Text` | Mensagem de texto |
| `Image` | Imagem |
| `Audio` | Áudio |
| `Video` | Vídeo |
| `Document` | Documento (PDF, etc.) |
| `Sticker` | Figurinha |
| `Contact` | Contato compartilhado |
| `Location` | Localização |
| `Unknown` | Tipo desconhecido |

**Uso:** `Message.Type`, seleciona `IMessageStrategy` no `MessageStrategyFactory`.

---

## DeliveryStatus

Status de entrega da mensagem.

| Valor | Descrição | ACK whatsapp-web.js |
|---|---|---|
| `Pending` | Aguardando envio | `0` |
| `Sent` | Enviada | `1` |
| `Delivered` | Entregue ao destinatário | `2` |
| `Read` | Lida pelo destinatário | `3` (READ/PLAYED) |
| `Failed` | Falha no envio | `-1` |

**Uso:** `Message.DeliveryStatus` — atualizado via `POST /api/webhook/status` (evento `message_ack` do Node.js), sempre avançando (nunca regressa).

---

## MessageSource

Origem da mensagem.

| Valor | Descrição |
|---|---|
| `Phone` | Chegou pelo webhook do WhatsApp |
| `System` | Enviada pelo sistema via API (corrigido quando o `message_create` confirma o envio) |

**Uso:** `Message.Source` — a combinação com `FromMe` controla os eventos SignalR emitidos (`MessageReceived` vs `MessageSent`).

---

## OccurrenceStatus

Status de uma ocorrência/chamado.

| Valor | Descrição |
|---|---|
| `Open` | Aberto |
| `InProgress` | Em andamento |
| `Resolved` | Resolvido |
| `Closed` | Fechado |

**Uso:** `Occurrence.Status`

---

## ClientTaskStatus

Status de uma tarefa da empresa.

| Valor | Descrição |
|---|---|
| `Open` | Aberta |
| `InProgress` | Em andamento |
| `Completed` | Concluída |
| `Cancelled` | Cancelada |

**Uso:** `ClientTask.Status`. A alteração para `InProgress` ou `Completed` requer role `Admin` ou `Dev`.

---

## Priority

Nível de prioridade.

| Valor | Descrição |
|---|---|
| `Low` | Baixa |
| `Medium` | Média |
| `High` | Alta |
| `Urgent` | Urgente |

**Uso:** `Occurrence.Priority`, `ClientTask.Priority`

---

## ClientStatus

Status de um cliente.

| Valor | Descrição |
|---|---|
| `Active` | Ativo |
| `Inactive` | Inativo |

**Uso:** `Client.Status`

---

## UserRole

Função do usuário no sistema.

| Valor | Descrição | Permissões |
|---|---|---|
| `Support` | Suporte | Todos os endpoints exceto alteração de status de tarefas |
| `Dev` | Desenvolvedor | Todos os endpoints |
| `Admin` | Administrador | Todos os endpoints |

**Uso:** `User.Role`, claim no JWT, controla acesso via `[Authorize(Roles = "...")]`
