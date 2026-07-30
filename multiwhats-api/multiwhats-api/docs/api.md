# API — Referência Completa

Base URL: `http://localhost:5261`

---

## AUTH — `/api/auth`

### POST `/api/auth/register`

Registra um novo usuário.

- **Auth:** Anônimo
- **Request:**
```json
{
  "name": "Joao",
  "password": "123123"
}
```
- **Response 201:** `{ "id": 1, "name": "Joao", "role": "Support", "isActive": true }`

---

### POST `/api/auth/login`

Autentica e retorna JWT.

- **Auth:** Anônimo
- **Request:**
```json
{
  "name": "Joao",
  "password": "123123"
}
```
- **Response 200:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "user": {
    "id": 1,
    "name": "Joao",
    "role": "Support",
    "isActive": true
  }
}
```

---

### POST `/api/auth/logout`

Revoga o token atual (blacklist).

- **Auth:** Requer token
- **Response 200:** `{ "message": "Logout realizado com sucesso" }`

---

## CLIENTS — `/api/clients`

### POST `/api/clients`

Cria um novo cliente.

- **Auth:** Requer token
- **Request:**
```json
{
  "name": "Timontec",
  "mainPhoneNumber": "5515999999999"
}
```

---

### GET `/api/clients`

Lista todos os clientes.

- **Auth:** Requer token
- **Response:** Array de clientes

---

### GET `/api/clients/{id}`

Busca cliente por ID.

- **Auth:** Requer token

---

### PUT `/api/clients/{id}`

Atualiza um cliente.

- **Auth:** Requer token
- **Request:**
```json
{
  "name": "Timontec Atualizado",
  "mainPhoneNumber": "5515999999999",
  "status": "Active"
}
```

---

### DELETE `/api/clients/{id}`

Soft delete de um cliente.

- **Auth:** Requer token

---

### GET `/api/clients/{id}/contacts`

Lista contatos vinculados a um cliente.

- **Auth:** Requer token

---

## CONTACTS — `/api/contacts`

### POST `/api/contacts`

Cria um novo contato.

- **Auth:** Requer token
- **Request:**
```json
{
  "jid": "5515987654321@c.us",
  "phoneNumber": "5515987654321",
  "name": "Maria",
  "pushName": "Maria"
}
```

---

### GET `/api/contacts`

Lista todos os contatos.

- **Auth:** Requer token

---

### GET `/api/contacts/{id}`

Busca contato por ID.

- **Auth:** Requer token

---

### PUT `/api/contacts/{id}`

Atualiza um contato.

- **Auth:** Requer token
- **Request:**
```json
{
  "name": "Maria Atualizada",
  "pushName": "Maria",
  "isBlocked": false
}
```

---

### DELETE `/api/contacts/{id}`

Soft delete de um contato.

- **Auth:** Requer token

---

### PATCH `/api/contacts/{id}/assign`

Vincula um contato a um cliente.

- **Auth:** Requer token
- **Request:**
```json
{
  "clientId": 1
}
```

---

### PATCH `/api/contacts/{id}/unassign`

Desvincula um contato do cliente.

- **Auth:** Requer token

---

## CHATS — `/api/chats`

### GET `/api/chats`

Lista conversas com paginação.

- **Auth:** Requer token
- **Query params:** `page` (default: 1), `pageSize` (default: 20)

---

### GET `/api/chats/{id}`

Detalhe de uma conversa com ocorrências.

- **Auth:** Requer token

---

### GET `/api/params/{id}/messages`

Lista mensagens de uma conversa com paginação.

- **Auth:** Requer token
- **Query params:** `page` (default: 1), `pageSize` (default: 50)

---

### GET `/api/chats/{id}/occurrences`

Lista ocorrências de uma conversa.

- **Auth:** Requer token

---

## MESSAGES — `/api/messages`

### POST `/api/messages/send`

Envia uma mensagem WhatsApp.

- **Auth:** Requer token
- **Body limit:** 100MB (para mídia)
- **Request:**
```json
{
  "phoneNumber": "5515996880359",
  "text": "Olá!"
}
```

---

### GET `/api/messages`

Lista todas as mensagens.

- **Auth:** Requer token

---

### GET `/api/messages/{id}`

Busca mensagem por ID.

- **Auth:** Requer token

---

### GET `/api/messages/phone/{phoneNumber}`

Busca mensagens por número de telefone.

- **Auth:** Requer token

---

## OCCURRENCES — `/api/occurrences`

### POST `/api/occurrences`

Cria uma ocorrência/chamado.

- **Auth:** Requer token
- **Request:**
```json
{
  "title": "Problema no boleto",
  "description": "Cliente relata erro ao gerar boleto",
  "priority": "High",
  "contactId": 1
}
```

---

### GET `/api/occurrences`

Lista todas as ocorrências.

- **Auth:** Requer token

---

### GET `/api/occurrences/{id}`

Busca ocorrência por ID.

- **Auth:** Requer token

---

### PUT `/api/occurrences/{id}`

Atualiza uma ocorrência.

- **Auth:** Requer token

---

### DELETE `/api/occurrences/{id}`

Soft delete de uma ocorrência.

- **Auth:** Requer token

---

## TASKS — `/api/tasks`

### POST `/api/tasks`

Cria uma tarefa.

- **Auth:** Requer token
- **Request:**
```json
{
  "title": "Relatório de custos",
  "description": "Gerar relatório mensal",
  "priority": "Medium",
  "clientId": 1
}
```

---

### GET `/api/tasks`

Lista todas as tarefas.

- **Auth:** Requer token

---

### GET `/api/tasks/{id}`

Busca tarefa por ID.

- **Auth:** Requer token

---

### PUT `/api/tasks/{id}`

Atualiza uma tarefa.

- **Auth:** Requer token

---

### DELETE `/api/tasks/{id}`

Soft delete de uma tarefa.

- **Auth:** Requer token

---

### PATCH `/api/tasks/{id}/status`

Altera o status de uma tarefa.

- **Auth:** Requer token **Admin** ou **Dev**
- **Request:**
```json
{
  "status": "InProgress"
}
```

---

## WEBHOOK — `/api/webhook`

### POST `/api/webhook/whatsapp`

Recebe mensagens do Node.js (WhatsApp).

- **Auth:** Anônimo
- **Request:** Ver [Webhook DTO](webhook.md)

---

## DEVICE — `/api/device`

### POST `/api/device`

Salva/atualiza informações do dispositivo conectado.

- **Auth:** Anônimo

---

### GET `/api/device`

Retorna o dispositivo conectado.

- **Auth:** Anônimo

---

## SIGNALR HUB

### `/whatsappHub`

Endpoint WebSocket para notificações em tempo real.

- **Protocolo:** SignalR (WebSocket)
- **Uso:** Frontend se conecta para receber atualizações de mensagens
