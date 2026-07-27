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

| Valor | Descrição |
|---|---|
| `Pending` | Aguardando envio |
| `Sent` | Enviada |
| `Delivered` | Entregue ao destinatário |
| `Read` | Lida pelo destinatário |
| `Failed` | Falha no envio |

**Uso:** `Message.DeliveryStatus`

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
