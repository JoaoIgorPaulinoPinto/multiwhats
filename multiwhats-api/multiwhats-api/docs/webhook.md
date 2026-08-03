# Webhook — Integração Node.js ↔ ASP.NET

## Fluxo Geral

```
WhatsApp Web (Puppeteer)
        │
        ▼
Node.js (messageria/ — porta 3333)
        │
        │  POST /api/webhook/whatsapp
        ▼
ASP.NET Core API (porta 5261)
        │
        ├── SaveIncomingMessageUseCase
        │      ├── Dedup por WhatssAppMessageId
        │      ├── Salva Message no banco (Incoming/Outgoing)
        │      ├── Atualiza Contact/Chat (LastMessageAt)
        │      └── Emite SignalR 'MessageReceived'/'MessageSent'
        │
        └── SignalR Hub (/whatsappHub)
               └── Notifica frontend em tempo real

Status de entrega (ACK):
WhatsApp muda ACK → Node.js 'message_ack'
        │
        │  POST /api/webhook/status  { messageId, deliveryStatus }
        ▼
UpdateMessageDeliveryStatusUseCase
        │
        └── SignalR 'MessageDeliveryStatusChanged'
```

## DTO — Webhook Payload

O Node.js envia o seguinte payload para o ASP.NET (`POST /api/webhook/whatsapp`):

```json
{
  "from": "5511999999999@c.us",
  "to": null,
  "phoneNumber": "5511999999999",
  "body": "Olá, preciso de ajuda",
  "timestamp": 1712345678,
  "notifyName": "Maria Silva",
  "messageType": "text",
  "hasMedia": false,
  "mediaUrl": null,
  "mediaMimeType": null,
  "mediaFilename": null,
  "mediaSize": null,
  "messageId": "true_abc123def456",
  "isForwarded": false,
  "fromMe": false,
  "userId": 1
}
```

### Campos

| Campo | Tipo | Obrigatório | Descrição |
|---|---|---|---|
| `from` | string | ✅ | JID completo do remetente (`5511999999999@c.us`) |
| `to` | string | ❌ | JID de destino (preenchido quando `fromMe = true`) |
| `phoneNumber` | string | ✅ | Número limpo (`5511999999999`) |
| `body` | string | ❌ | Texto da mensagem |
| `timestamp` | long | ✅ | Unix timestamp da mensagem |
| `notifyName` | string | ❌ | Nome de notificação do contato |
| `messageType` | string | ✅ | Tipo: `text`, `image`, `audio`, `video`, `document`, `sticker`, `vcard`, `location` |
| `hasMedia` | bool | ✅ | Se a mensagem possui mídia |
| `mediaUrl` | string | ❌ | Base64 da mídia (quando `hasMedia = true`) |
| `mediaMimeType` | string | ❌ | Tipo MIME da mídia |
| `mediaFilename` | string | ❌ | Nome do arquivo |
| `mediaSize` | long | ❌ | Tamanho em bytes |
| `messageId` | string | ✅ | ID real do WhatsApp (dedup) |
| `isForwarded` | bool | ✅ | Se a mensagem foi encaminhada |
| `fromMe` | bool | ✅ | Se a mensagem foi enviada pelo próprio número |
| `userId` | int | ✅ | ID do usuário dono da conexão WhatsApp |

## DTO — Webhook de Status

O Node.js envia status de entrega (`POST /api/webhook/status`) quando o WhatsApp confirma um ACK de mensagem enviada:

```json
{
  "messageId": "true_abc123def456",
  "deliveryStatus": "Sent"
}
```

| Campo | Tipo | Descrição |
|---|---|---|
| `messageId` | string | `WhatssAppMessageId` da mensagem (correlacionado ao `_serialized` do WhatsApp) |
| `deliveryStatus` | string | `Pending`, `Sent`, `Delivered`, `Read` ou `Failed` |

### Mapeamento ACK → Status

| ACK (whatsapp-web.js) | DeliveryStatus |
|---|---|
| `1` | Sent |
| `2` | Delivered |
| `3` (READ/PLAYED) | Read |
| `0` | Pending |
| `-1` | Failed |

O `UpdateMessageDeliveryStatusUseCase` localiza a mensagem por `WhatssAppMessageId`, atualiza o status (somente avanço — nunca regressa de `Read` para `Delivered`) e emite `MessageDeliveryStatusChanged`.

## Mapeamento para Entity

O `SaveIncomingMessageUseCase` mapeia o DTO para a entidade `Message`:

| DTO Field | Entity Field | Observação |
|---|---|---|
| `from` | `FromJid` | JID do remetente |
| `to` | `ToJid` | JID de destino |
| `phoneNumber` | `PhoneNumber` | Número limpo |
| `body` | `Body` | Texto |
| `timestamp` | `Timestamp` | Unix timestamp |
| `notifyName` | `NotifyName` | Nome de notificação |
| `messageType` | `Type` | Convertido para enum `MessageType` |
| `hasMedia` | `HasMedia` | Flag de mídia |
| `mediaUrl` | `MediaUrl` | Base64 |
| `mediaMimeType` | `MediaMimeType` | Tipo MIME |
| `mediaFilename` | `MediaFilename` | Nome do arquivo |
| `mediaSize` | `MediaSize` | Tamanho |
| `messageId` | `WhatssAppMessageId` | ID real do WhatsApp (índice único) |
| `isForwarded` | `IsForwarded` | Flag de encaminhamento |
| `userId` | `UserId` | FK → User |
| `fromMe` | `Direction` | `Incoming` (false) ou `Outgoing` (true) |
| — | `SentAt` | Convertido de `timestamp` para DateTime |
| — | `DeliveryStatus` | `Pending` (outgoing) ou `Delivered` (incoming) |

## Envio de Mensagens (ASP.NET → Node.js)

O `SendMessageUseCase` envia mensagens outbound:

```
POST http://localhost:3333/api/enviar
Content-Type: application/json

{
  "jid": "5515996880359@c.us",
  "mensagem": "Olá!",
  "type": "text",
  ... // payload montado pela IMessageStrategy
}
```

O Node.js repassa para o WhatsApp Web via Puppeteer.

> O Node.js também mantém uma fila (`sendsAguardandoId`) para correlacionar o envio com o `message_create` que o WhatsApp retorna, obtendo assim o `WhatssAppMessageId` real usado depois no webhook de status.
