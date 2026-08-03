# WebSocket (SignalR)

## Visão Geral

A comunicação em tempo real entre frontend e backend é feita via **SignalR** (`@microsoft/signalr`), que estabelece uma conexão WebSocket com fallback automático para Server-Sent Events e Long Polling.

O hub está mapeado em `/whatsappHub` no backend.

## Cliente (`src/services/websocket.ts`)

### Instância Singleton

```ts
import { ws } from "../services/websocket"
```

O `WsClient` é exportado como singleton (`ws`). Todos os consumidores compartilham a mesma conexão.

### Estados da Conexão

```ts
type WsConnectionState = "disconnected" | "connecting" | "connected" | "reconnecting"
```

Acompanhe o estado atual via:

```ts
ws.state                    // valor atual
ws.onStateChange((state) => {
  // reage a mudanças de estado
})
```

### Eventos Tipados

| Evento | Payload | Descrição |
|---|---|---|
| `message:received` | `MessageResponse` | Mensagem recebida via webhook |
| `message:sent` | `MessageResponse` | Mensagem enviada pelo usuário |
| `message:delivery-status` | `MessageResponse` | Status de entrega atualizado (Pending/Sent/Delivered/Read/Failed) |

### Inscrição em Eventos

```ts
const unsubscribe = ws.on("message:received", (msg) => {
  console.log("Nova mensagem:", msg)
})
// cancelar inscrição:
unsubscribe()
```

O método `on()` inicia a conexão automaticamente se ainda não estiver ativa.

### Ciclo de Vida

1. `on(evento, callback)` é chamado por um consumidor
2. Se não houver conexão ativa, `start()` é chamado (com deduplicação de chamadas concorrentes)
3. O SignalR negocia a conexão enviando o JWT via `accessTokenFactory`
4. Handlers do SignalR são registrados uma única vez (`handlersRegistered`)
5. Em caso de queda, o SignalR tenta reconexão automática nos intervalos `[0, 2000, 5000, 10000, 30000]` ms
6. Ao desconectar permanentemente, o estado volta para `disconnected` e uma nova chamada a `on()` recria a conexão

### Gerenciamento de Token

O token JWT é lido de `localStorage.getItem("token")` e enviado na negociação da conexão.

Para forçar renovação da conexão (ex: após login/logout):

```ts
ws.refreshToken()  // para a conexão atual
```

### Parada Manual

```ts
await ws.stop()
```

## Consumidores

### `use-chat-messaging.ts`

Hook que gerencia mensagens de um chat específico.

- Escuta `message:received` e `message:sent`
- Filtra mensagens pelo `chatId` ativo
- Adiciona a mensagem ao estado local e invalida o cache

### `chat-area.logic.tsx`

- Escuta `message:received`, `message:sent` e `message:delivery-status`
- Em `message:delivery-status`, atualiza a mensagem existente pelo `id` (preservando a posição) em vez de adicionar nova
- Atualiza o indicador de entrega na bolha (✓/✓✓/✓✓ azul)

### `chat-sidebar.logic.tsx`

Hook que gerencia a lista de chats na sidebar.

- Escuta `message:received`, `message:sent` e `message:delivery-status`
- Re-carrega a lista completa de chats via REST API
- Mantém cache local (`cachedChats`) para renderização inicial otimizada

## Backend

### Hub (`WhatsappHub.cs`)

Mapeado em `/whatsappHub`. Não possui métodos invocáveis pelo cliente — apenas envia eventos.

### Emissores

| Arquivo | Evento SignalR | Gatilho |
|---|---|---|
| `SendMessageUseCase.cs` | `MessageSent` | Após envio de mensagem |
| `SaveIncomingMessageUseCase.cs` | `MessageReceived` | Após recebimento via webhook |
| `UpdateMessageDeliveryStatusUseCase.cs` | `MessageDeliveryStatusChanged` | Após ACK do WhatsApp (webhook de status) |
| `UseCaseLogger.cs` | `LogReceived` | (não consumido no frontend) |

### Fluxo de Dados

```
[WhatsApp] → [Node.js messageria] → POST /api/webhook/whatsapp
                                           ↓
                                  WebhookController
                                           ↓
                                  SaveIncomingMessageUseCase
                                           ↓
                                  IHubContext<WhatsappHub>
                                           ↓
                                  SignalR → WsClient.on("message:received")
                                           ↓
                                  useChatMessaging / chatSidebar
```

```
[WhatsApp] muda ACK → [Node.js] 'message_ack' → POST /api/webhook/status
                                           ↓
                                  WebhookController
                                           ↓
                                  UpdateMessageDeliveryStatusUseCase
                                           ↓
                                  IHubContext<WhatsappHub> → 'MessageDeliveryStatusChanged'
                                           ↓
                                  WsClient.on("message:delivery-status")
                                           ↓
                                  chat-area.logic.tsx (atualiza bolha)
```

## Configuração

| Parâmetro | Valor |
|---|---|
| Hub URL | `{NEXT_PUBLIC_API_URL}/whatsappHub` |
| Fallback | `http://localhost:5261/whatsappHub` |
| Log Level | `Warning` |
| Reconexão | `[0, 2000, 5000, 10000, 30000]` ms |
| Autenticação | JWT via `accessTokenFactory` |
