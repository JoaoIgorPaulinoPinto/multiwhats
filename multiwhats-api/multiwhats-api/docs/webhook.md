# Webhook — Integração Node.js ↔ ASP.NET

## Fluxo Geral

```
WhatsApp Web (Puppeteer)
        │
        ▼
Node.js (messageria/ — porta 3000)
        │
        │  POST /api/webhook/whatsapp
        ▼
ASP.NET Core API (porta 5261)
        │
        ├── SaveIncomingMessageUseCase
        │      ├── Salva Message no banco (Direction: Incoming)
        │      ├── Atualiza Contact.LastMessageAt
        │      └── Atualiza Chat.LastMessage
        │
        └── SignalR Hub (/whatsappHub)
               └── Notifica frontend em tempo real
```

## DTO — Webhook Payload

O Node.js envia o seguinte payload para o ASP.NET:

```json
{
  "from": "5511999999999@c.us",
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
  "userId": 1
}
```

### Campos

| Campo | Tipo | Obrigatório | Descrição |
|---|---|---|---|
| `from` | string | ✅ | JID completo do remetente (`5511999999999@c.us`) |
| `phoneNumber` | string | ✅ | Número limpo (`5511999999999`) |
| `body` | string | ❌ | Texto da mensagem |
| `timestamp` | long | ✅ | Unix timestamp da mensagem |
| `notifyName` | string | ❌ | Nome de notificação do contato |
| `messageType` | string | ✅ | Tipo: `text`, `image`, `audio`, `video`, `document` |
| `hasMedia` | bool | ✅ | Se a mensagem possui mídia |
| `mediaUrl` | string | ❌ | Base64 da mídia (quando `hasMedia = true`) |
| `mediaMimeType` | string | ❌ | Tipo MIME da mídia |
| `mediaFilename` | string | ❌ | Nome do arquivo |
| `mediaSize` | long | ❌ | Tamanho em bytes |
| `messageId` | string | ✅ | ID único do WhatsApp (dedup) |
| `isForwarded` | bool | ✅ | Se a mensagem foi encaminhada |
| `userId` | int | ✅ | ID do usuário dono da conexão WhatsApp |

## Mapeamento para Entity

O `SaveIncomingMessageUseCase` mapeia o DTO para a entidade `Message`:

| DTO Field | Entity Field | Observação |
|---|---|---|
| `from` | `FromJid` | JID do remetente |
| `phoneNumber` | `PhoneNumber` | Número limpo |
| `body` | `Body` | Texto |
| `timestamp` | `Timestamp` | Unix timestamp |
| `notifyName` | `NotifyName` | Nome de notificação |
| `messageType` | `Type` | Convertido para enum `MessageType` |
| `hasMedia` | `HasMedia` | Flag de mídia |
| `mediaUrl` | `MediaUrl` | Base64 (LONGTEXT no banco) |
| `mediaMimeType` | `MediaMimeType` | Tipo MIME |
| `mediaFilename` | `MediaFilename` | Nome do arquivo |
| `mediaSize` | `MediaSize` | Tamanho |
| `messageId` | `MessageId` | ID do WhatsApp |
| `isForwarded` | `IsForwarded` | Flag de encaminhamento |
| `userId` | `UserId` | FK → User |
| — | `Direction` | `Incoming` (fixo) |
| — | `SentAt` | Convertido de `timestamp` para DateTime |
| — | `DeliveryStatus` | `Delivered` (fixo para incoming) |

## Envio de Mensagens (ASP.NET → Node.js)

O `SendMessageUseCase` envia mensagens outbound:

```
POST http://localhost:3000/api/enviar
Content-Type: application/json

{
  "phoneNumber": "5515996880359",
  "text": "Olá!",
  "type": "text",
  ... // payload montado pela IMessageStrategy
}
```

O Node.js repassa para o WhatsApp Web via Puppeteer.

## Telecomandos do WhatsApp

O Node.js também processa comandos especiais no `body` da mensagem:

| Comando | Ação |
|---|---|
| `#ajuda` | Lista comandos disponíveis |
| `#status` | Mostra status do bot |
| `#grupos` | Lista grupos gerenciados |
| `#sair` | Sai do grupo atual |

Esses comandos são interceptados pelo Node.js e não chegam ao ASP.NET.
