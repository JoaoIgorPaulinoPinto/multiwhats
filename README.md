# MultiWhats API

Plataforma de gerenciamento multi-clientes para WhatsApp. Permite que múltiplos operadores atendam clientes via WhatsApp, com controle de conversas, contatos, chamados (ocorrências) e tarefas.

---

## Índice

- [O que é o MultiWhats?](#o-que-é-o-multiwhats)
- [Arquitetura do Sistema](#arquitetura-do-sistema)
- [Stack Tecnológica](#stack-tecnológica)
- [Entidades e Relacionamentos](#entidades-e-relacionamentos)
- [Rodando com Docker](#rodando-com-docker)
- [Rodando sem Docker (Desenvolvimento)](#rodando-sem-docker-desenvolvimento)
- [Endpoints da API](#endpoints-da-api)
- [Autenticação e Autorização](#autenticação-e-autorização)
- [Sistema de Auditoria](#sistema-de-auditoria)
- [Padrões de Código](#padrões-de-código)
- [Comandos Úteis](#comandos-úteis)

---

## O que é o MultiWhats?

O MultiWhats é um sistema composto por 3 partes que trabalham juntas:

```
┌──────────────┐     ┌──────────────┐     ┌──────────────┐
│   Frontend   │────▶│   Backend    │────▶│  Messageria  │
│  Next.js     │     │  ASP.NET Core│     │  Node.js     │
│  :3000       │     │  :5261       │     │  :3333       │
└──────────────┘     └──────┬───────┘     └──────┬───────┘
                            │                     │
                            ▼                     ▼
                      ┌──────────┐        WhatsApp Web.js
                      │  MySQL   │        (Puppeteer/Chromium)
                      │ (Railway)│
                      └──────────┘
```

1. **Frontend (Next.js)** - Interface visual que o operador usa no navegador. Roda na porta 3000.

2. **Backend (ASP.NET Core)** - "Cérebro" do sistema. Recebe pedidos do frontend, gerencia dados no banco MySQL, e envia/recebe mensagens do WhatsApp. Roda na porta 5261.

3. **Messageria (Node.js)** - Ponte com o WhatsApp. Usa Puppeteer (um navegador automatizado) para se conectar ao WhatsApp Web. Roda na porta 3333.

**Fluxo simplificado de uma mensagem:**
- Operador digita no Frontend → Frontend pede ao Backend → Backend salva no MySQL e pede à Messageria → Messageria envia pelo WhatsApp
- Cliente responde no WhatsApp → Messageria recebe → Envia para o Backend (webhook) → Backend salva e avisa o Frontend em tempo real (SignalR)

---

## Arquitetura do Sistema

### Arquitetura de Código (Clean Architecture)

O Backend segue o padrão **Use Case** (Casos de Uso), onde cada operação de negócio é uma classe separada. Isso mantém o código organizado e fácil de testar.

```
Controller (recebe HTTP)
    │
    ▼
Use Case (executa regra de negócio)
    │
    ▼
Repository (acessa o banco de dados)
    │
    ▼
MySQL (armazena os dados)
```

**Camadas:**
- **Controllers** (`src/controllers/`) - Recebem requisições HTTP e devolvem respostas. São "receptores" simples que delegam o trabalho.
- **Use Cases** (`src/usecases/`) - Contêm toda a lógica de negócio. Cada classe tem um único método `Execute()`.
- **Repositories** (`src/repositories/`) - Falam com o banco de dados. Abstraem todas as consultas SQL.
- **Entities** (`src/data/entities/`) - Representam as tabelas do banco. Cada entidade é uma tabela.
- **DTOs** (`src/data/dtos/`) - "Transfer objects" - formatos de dados que entram (Request) e saem (Response) da API.
- **Services** (`src/services/`) - Serviços auxiliares como autenticação JWT, auditoria, e comunicação em tempo real (SignalR).

### Padrões de Design Utilizados

1. **Use Case Pattern** - Cada operação (criar cliente, enviar mensagem, etc.) é uma classe separada. Isso evita "controllers gigantes" com tudo misturado.

2. **Repository Pattern** - Todo acesso ao banco é feito por interfaces. Se amanhã trocarmos o MySQL por PostgreSQL, só precisamos trocar a implementação do Repository, sem mudar os Use Cases.

3. **Strategy Pattern** - O envio de mensagens usa o padrão Strategy para tratar cada tipo de mensagem (texto, imagem, áudio, vídeo, documento, sticker) de forma diferente, sem condicionais gigantes.

4. **Soft Delete** - Nenhum dado é deletado de verdade. Quando algo é "deletado", o campo `IsDeleted` vira `true` e o registro some das consultas, mas continua no banco para auditoria.

5. **Auto-Audit** - Ao salvar qualquer coisa, o sistema automaticamente registra quem criou/modificou, quando, e os valores antigos/novos.

---

## Stack Tecnológica

| Componente | Tecnologia | Versão |
|------------|-----------|--------|
| Backend | ASP.NET Core (C#) | 10.0 |
| Banco de Dados | MySQL (via Pomelo) | 8.0+ |
| ORM | Entity Framework Core | 9.0 |
| Autenticação | JWT Bearer | - |
| Tempo Real | SignalR | - |
| Documentação | Swagger/OpenAPI | 7.3 |
| Messageria | Node.js + WhatsApp Web.js | - |
| Frontend | Next.js | - |
| Containerização | Docker + Docker Compose | - |

---

## Entidades e Relacionamentos

### Diagrama de Relacionamentos

```
User (Usuário do sistema)
 ├── Support / Dev / Admin
 │
 ├── Contacts[] (contatos que criou)
 ├── Messages[] (mensagens que enviou/recebeu)
 └── Assigned Occurrences[] (ocorrências atribuídas)

Client (Empresa/Cliente)
 ├── Name, MainPhoneNumber, Status (Active/Inactive)
 │
 ├── Contacts[] (1:N — contatos vinculados ao cliente)
 ├── Chats[] (1:N — conversas do cliente)
 └── ClientTasks[] (1:N — demandas da empresa)

Contact (Número de WhatsApp)
 ├── JID (ID único do WhatsApp: 5511999999999@c.us)
 ├── PhoneNumber, Name, PushName
 │
 ├── ClientId? (FK — pode ser vinculado a um Cliente)
 ├── GroupId? (FK — pode pertencer a um Grupo)
 ├── Chat (1:1 — uma conversa vinculada)
 ├── Messages[] (1:N)
 └── Occurrences[] (1:N)

Chat (Conversa do WhatsApp)
 ├── JID, PhoneNumber, Name
 ├── ContactId (1:1 com Contact)
 ├── ClientId? (FK)
 ├── AssignedToUserId? (operador responsável)
 │
 ├── Messages[] (1:N — mensagens da conversa)
 └── Occurrences[] (1:N — chamados dessa conversa)

Message (Mensagem)
 ├── Direction: Incoming | Outgoing
 ├── Type: Text | Image | Audio | Video | Document | Sticker
 ├── DeliveryStatus: Pending | Sent | Delivered | Read | Failed
 ├── ChatId (FK obrigatório — sempre pertence a uma conversa)
 ├── OccurrenceId? (FK — pode estar vinculada a um chamado)
 └── ReplyToId? (FK — pode ser resposta a outra mensagem)

Occurrence (Chamado/Incidente)
 ├── Title, Description, Status, Priority
 ├── ChatId (FK obrigatório)
 └── AssignedToUserId? (operador responsável)

ClientTask (Demanda da Empresa)
 ├── Title, Description, Status, Priority, DueDate
 └── ClientId (FK obrigatório)
```

### Tabela de Enums (Valores Permitidos)

| Enum | Valores | Usado em |
|------|---------|----------|
| `UserRole` | Support, Dev, Admin | Usuários |
| `ClientStatus` | Active, Inactive | Clientes |
| `OccurrenceStatus` | Open, InProgress, Resolved, Closed | Ocorrências |
| `ClientTaskStatus` | Open, InProgress, Completed, Cancelled | Tarefas |
| `Priority` | Low, Medium, High, Urgent | Ocorrências e Tarefas |
| `MessageType` | Text, Image, Audio, Video, Document, Sticker, Contact, Location, Unknown | Mensagens |
| `MessageDirection` | Incoming, Outgoing | Mensagens |
| `DeliveryStatus` | Pending, Sent, Delivered, Read, Failed | Mensagens |

---

## Rodando com Docker

### Pré-requisitos
- Docker e Docker Compose instalados

### 1. Criar arquivo `.env` na raiz do projeto

```bash
DB_CONNECTION_STRING=server=tokaido.proxy.rlwy.net;port=14481;database=railway;user=root;password=SUA_SENHA;AllowZeroDateTime=True;DateTimeKind=Utc
JWT_SECRET=SuaChaveSecretaSuperPoderosaEALeatoriaComMaisDe32Caracteres!
```

### 2. Subir todos os serviços

```bash
docker compose up --build
```

Isso vai construir e rodar:
- **Backend** (ASP.NET Core) → `http://localhost:5261`
- **Messageria** (Node.js + WhatsApp Web.js) → `http://localhost:3333`
- **Frontend** (Next.js) → `http://localhost:3000`

### 3. Atualizar após modificar código

```bash
# Reconstruir apenas o serviço alterado (ex: backend)
docker compose up --build backend

# Reconstruir tudo
docker compose up --build

# Rodar em background (detached)
docker compose up --build -d

# Ver logs
docker compose logs -f backend
docker compose logs -f messageria
docker compose logs -f frontend
```

### 4. Parar e limpar

```bash
# Parar todos os serviços
docker compose down

# Parar e remover imagens construídas
docker compose down --rmi local
```

---

## Rodando sem Docker (Desenvolvimento)

### Pré-requisitos
- .NET 10 SDK
- MySQL rodando localmente (ou remoto)
- Node.js 18+ (para a messageria)

### Backend (ASP.NET Core)

```bash
cd multiwhats-api/multiwhats-api/multiwhats-api

# Instalar dependências
dotnet restore

# Aplicar migrations no banco
dotnet ef database update

# Rodar
dotnet run
```

A API estará disponível em `http://localhost:5261`. O Swagger estará em `http://localhost:5261/swagger`.

### Messageria (Node.js)

```bash
cd messageria
npm install
npm start
```

O serviço de messageria estará em `http://localhost:3333`.

---

## Endpoints da API

### Autenticação

| Método | Endpoint | Descrição | Autorização |
|--------|----------|-----------|-------------|
| POST | `/api/auth/register` | Criar novo usuário | Pública |
| POST | `/api/auth/login` | Fazer login (retorna JWT) | Pública |
| POST | `/api/auth/logout` | Revogar token atual | Autenticado |

### Clientes

| Método | Endpoint | Descrição | Autorização |
|--------|----------|-----------|-------------|
| POST | `/api/clients` | Criar cliente | Autenticado |
| GET | `/api/clients` | Listar clientes | Autenticado |
| GET | `/api/clients/{id}` | Detalhar cliente | Autenticado |
| PUT | `/api/clients/{id}` | Atualizar cliente | Autenticado |
| DELETE | `/api/clients/{id}` | Deletar cliente (soft) | Autenticado |

### Contatos

| Método | Endpoint | Descrição | Autorização |
|--------|----------|-----------|-------------|
| POST | `/api/contacts` | Criar contato | Autenticado |
| GET | `/api/contacts` | Listar contatos | Autenticado |
| GET | `/api/contacts/{id}` | Detalhar contato | Autenticado |
| PUT | `/api/contacts/{id}` | Atualizar contato | Autenticado |
| DELETE | `/api/contacts/{id}` | Deletar contato (soft) | Autenticado |
| PATCH | `/api/contacts/{id}/assign` | Vincular contato a cliente | Autenticado |
| PATCH | `/api/contacts/{id}/unassign` | Desvincular contato de cliente | Autenticado |

### Conversas (Chats)

| Método | Endpoint | Descrição | Autorização |
|--------|----------|-----------|-------------|
| GET | `/api/chats` | Listar conversas (paginado) | Autenticado |
| GET | `/api/chats/{id}` | Detalhar conversa | Autenticado |
| GET | `/api/chats/{id}/messages` | Mensagens da conversa (paginado) | Autenticado |
| GET | `/api/chats/{id}/occurrences` | Ocorrências da conversa | Autenticado |

### Mensagens

| Método | Endpoint | Descrição | Autorização |
|--------|----------|-----------|-------------|
| POST | `/api/messages/send` | Enviar mensagem WhatsApp | Autenticado |
| GET | `/api/messages` | Listar mensagens | Autenticado |
| GET | `/api/messages/{id}` | Detalhar mensagem | Autenticado |
| GET | `/api/messages/phone/{phoneNumber}` | Mensagens por telefone | Autenticado |

### Ocorrências (Chamados)

| Método | Endpoint | Descrição | Autorização |
|--------|----------|-----------|-------------|
| POST | `/api/occurrences` | Criar ocorrência | Autenticado |
| GET | `/api/occurrences` | Listar ocorrências | Autenticado |
| GET | `/api/occurrences/{id}` | Detalhar ocorrência | Autenticado |
| PUT | `/api/occurrences/{id}` | Atualizar ocorrência | Autenticado |
| DELETE | `/api/occurrences/{id}` | Deletar ocorrência (soft) | Autenticado |

### Tarefas

| Método | Endpoint | Descrição | Autorização |
|--------|----------|-----------|-------------|
| POST | `/api/tasks` | Criar tarefa | Autenticado |
| GET | `/api/tasks` | Listar tarefas | Autenticado |
| GET | `/api/tasks/{id}` | Detalhar tarefa | Autenticado |
| PUT | `/api/tasks/{id}` | Atualizar tarefa | Autenticado |
| DELETE | `/api/tasks/{id}` | Deletar tarefa (soft) | Autenticado |
| PATCH | `/api/tasks/{id}/status` | Atualizar status | Admin/Dev |

### Dispositivo

| Método | Endpoint | Descrição | Autorização |
|--------|----------|-----------|-------------|
| POST | `/api/device` | Registrar dispositivo WhatsApp | Pública |
| GET | `/api/device` | Ver dispositivo conectado | Pública |

### Webhook

| Método | Endpoint | Descrição | Autorização |
|--------|----------|-----------|-------------|
| POST | `/api/webhook/whatsapp` | Receber mensagens do WhatsApp | Pública |

---

## Autenticação e Autorização

### Como funciona o Login

1. O operador envia `POST /api/auth/login` com nome e senha.
2. O Backend valida as credenciais e retorna um **token JWT**.
3. O Frontend salva esse token e envia em todas as requisições no header `Authorization: Bearer {token}`.

### Roles (Perfis de Acesso)

| Role | O que pode fazer |
|------|------------------|
| **Support** | Todas as operações, exceto mudar status de tarefas |
| **Dev** | Tudo, incluindo mudar status de tarefas |
| **Admin** | Tudo, incluindo mudar status de tarefas |

### Como proteger um endpoint

No Controller, basta adicionar o atributo `[Authorize]`:
```csharp
[Authorize]          // Qualquer usuário logado
[Authorize(Roles = "Admin,Dev")]  // Só Admin ou Dev
```

### Token Blacklist (Logout)

Quando o operador faz logout, o ID do token (`JTI`) é adicionado a uma lista negra em memória. Mesmo que o token ainda esteja válido (não expirou), ele será recusado.

---

## Sistema de Auditoria

Todo INSERT, UPDATE ou DELETE é registrado automaticamente.

### O que é auditado:
- **Quem** executou (ID, nome, role do usuário)
- **O quê** foi feito (Created, Updated, Deleted)
- **Quando** (timestamp UTC)
- **Valores antigos e novos** (em formato JSON)

### Como funciona internamente:

1. **Auto-Audit** (`AppDbContext.ApplyAudit()`): Ao salvar, o Entity Framework automaticamente preenche `CreatedAt`, `LastUpdate`, `CreatedByUserId`, `LastUpdatedByUserId`. Deletes são convertidos em soft delete (`IsDeleted = true`).

2. **UseCaseLogger**: Cada Use Case registra a ação explicitamente com detalhes (ex: "Created Contact #5 para o Client #2"). Os logs são enviados em tempo real para o Frontend via SignalR.

3. **Console Colorido**: Cada ação é impressa no terminal com cores:
   - Verde para criações
   - Amarelo para atualizações

### Exemplo de log no console:
```
[AUDIT] 12:34:56 | User "Joao" (Admin) | Created Contact #5 | Created Contact #5
[AUDIT] 12:35:10 | User "Maria" (Support) | Updated Occurrence #3 | Updated Occurrence #3: [Status: Open -> InProgress]
```

---

## Padrões de Código

### Estrutura de uma Entity

Todas as entidades herdam de `BaseEntity`, que fornece:
- `CreatedAt` - Data de criação (UTC)
- `LastUpdate` - Última atualização (UTC)
- `IsDeleted` - Flag de soft delete
- `CreatedByUserId` - Quem criou
- `LastUpdatedByUserId` - Quem modificou por último

### Estrutura de um Use Case

```csharp
public class CreateClientUseCase : ICreateClientUseCase
{
    private readonly IClientRepository _repository;
    private readonly UseCaseLogger _logger;

    public async Task<ClientDetailResponse> Execute(CreateClientRequest request, int userId)
    {
        // 1. Criar entidade
        var client = new Client(request.Name, request.MainPhoneNumber);
        
        // 2. Salvar no banco
        await _repository.AddAsync(client);
        
        // 3. Registrar auditoria
        await _logger.LogAsync(userId, "Client", client.Id, "CreateClient", $"Created Client #{client.Id}");
        
        // 4. Retornar resposta
        return MapToDetailResponse(client);
    }
}
```

### Estrutura de um Repository

```csharp
public class ClientRepository : IClientRepository
{
    private readonly AppDbContext _context;

    public async Task<Client?> GetByIdAsync(int id)
    {
        // AsNoTracking() = melhora performance (não precisa rastrear mudanças)
        return await _context.Clients
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
    }
}
```

### Strategy Pattern (Envio de Mensagens)

Cada tipo de mensagem (texto, imagem, áudio, etc.) tem sua própria classe Strategy:

```
IMessageStrategy (interface)
    ├── TextMessageStrategy    → envia texto puro
    ├── ImageMessageStrategy   → envia imagem (base64)
    ├── AudioMessageStrategy   → envia áudio
    ├── VideoMessageStrategy   → envia vídeo
    ├── DocumentMessageStrategy → envia documento
    └── StickerMessageStrategy → envia sticker
```

O `MessageStrategyFactory` escolhe a Strategy correta com base no tipo de mensagem. Isso evita um `switch/case` gigante com toda a lógica misturada.

---

## Webhook DTO (Node.js → ASP.NET)

Quando uma mensagem chega do WhatsApp, o Node.js envia este formato para o Backend:

```json
{
  "from": "5511999999999@c.us",     // JID completo do WhatsApp
  "phoneNumber": "5511999999999",    // Número limpo (só dígitos)
  "body": "mensagem",
  "timestamp": 1712345678,           // Unix timestamp
  "notifyName": "Nome do Contato",
  "messageType": "text",             // text, image, audio, video, document
  "hasMedia": false,
  "mediaUrl": null,                  // Base64 da mídia (se houver)
  "mediaMimeType": null,
  "mediaFilename": null,
  "mediaSize": null,
  "mediaCaption": null,
  "messageId": "true_...",           // ID único do WhatsApp (deduplicação)
  "isForwarded": false,
  "userId": 1                        // ID do usuário dono da conexão
}
```

---

## Exemplos de Requisições

### Registrar usuário
```http
POST http://localhost:5261/api/auth/register
Content-Type: application/json

{
  "name": "Joao",
  "password": "123123"
}
```

### Login
```http
POST http://localhost:5261/api/auth/login
Content-Type: application/json

{
  "name": "Joao",
  "password": "123123"
}

// Response:
{
  "token": "...",
  "user": {
    "id": 1,
    "name": "Joao",
    "role": "Support"
  }
}
```

### Criar cliente
```http
POST http://localhost:5261/api/clients
Authorization: Bearer {token}
Content-Type: application/json

{
  "name": "Timontec",
  "mainPhoneNumber": "5515999999999"
}
```

### Criar contato
```http
POST http://localhost:5261/api/contacts
Authorization: Bearer {token}
Content-Type: application/json

{
  "jid": "5515987654321@c.us",
  "phoneNumber": "5515987654321",
  "name": "Maria",
  "pushName": "Maria"
}
```

### Vincular contato a cliente
```http
PATCH http://localhost:5261/api/contacts/1/assign
Authorization: Bearer {token}
Content-Type: application/json

{
  "clientId": 1
}
```

### Enviar mensagem
```http
POST http://localhost:5261/api/messages/send
Authorization: Bearer {token}
Content-Type: application/json

{
  "jid": "5515996880359@c.us",
  "text": "olá"
}
```

### Criar ocorrência
```http
POST http://localhost:5261/api/occurrences
Authorization: Bearer {token}
Content-Type: application/json

{
  "title": "Problema boleto",
  "description": "Cliente relata que o boleto não foi gerado",
  "priority": "High",
  "chatId": 1
}
```

### Criar tarefa
```http
POST http://localhost:5261/api/tasks
Authorization: Bearer {token}
Content-Type: application/json

{
  "title": "Relatório custos",
  "description": "Gerar relatório mensal de custos",
  "priority": "Medium",
  "clientId": 1
}
```

### Atualizar status da tarefa (Admin/Dev)
```http
PATCH http://localhost:5261/api/tasks/1/status
Authorization: Bearer {token}
Content-Type: application/json

{
  "status": "InProgress"
}
```

---

## Comandos Úteis

```bash
# Sobe a API ASP.NET Core em http://localhost:5261
dotnet run

# Sobe o serviço Node.js em http://localhost:3000
cd messageria && npm start

# Cria uma nova migration do Entity Framework
dotnet ef migrations add NomeDaMigration

# Aplica as migrations pendentes no banco MySQL
dotnet ef database update

# Rodar com Docker
docker compose up --build

# Ver logs do backend com Docker
docker compose logs -f backend
```

---

## Estrutura de Pastas

```
multiwhats-api/
├── multiwhats-api/
│   ├── Migrations/                    # Migrations do Entity Framework
│   ├── src/
│   │   ├── controllers/               # Controllers HTTP (receptores de requisições)
│   │   │   ├── AuthController.cs
│   │   │   ├── ClientsController.cs
│   │   │   ├── ContactsController.cs
│   │   │   ├── ChatsController.cs
│   │   │   ├── MessagesController.cs
│   │   │   ├── OccurrencesController.cs
│   │   │   ├── TasksController.cs
│   │   │   ├── DeviceController.cs
│   │   │   └── WebhookController.cs
│   │   ├── data/
│   │   │   ├── db/                    # Banco de dados (EF Core DbContext)
│   │   │   ├── dtos/                  # Objetos de transferência de dados
│   │   │   │   ├── Requests/          # DTOs de entrada (requests)
│   │   │   │   ├── Responses/         # DTOs de saída (responses)
│   │   │   │   └── Webhook/           # DTO do webhook
│   │   │   ├── entities/              # Entidades de domínio (tabelas)
│   │   │   ├── enums/                 # Enums (valores permitidos)
│   │   │   └── strategies/            # Padrão Strategy (tipos de mensagem)
│   │   ├── helpers/                   # Utilitários
│   │   ├── repositories/              # Acesso ao banco de dados
│   │   │   ├── interfaces/            # Interfaces dos repositories
│   │   │   └── repositories/          # Implementações
│   │   ├── services/                  # Serviços auxiliares
│   │   │   ├── AuditService.cs        # Auditoria automática
│   │   │   ├── TokenService.cs        # Geração de JWT
│   │   │   ├── TokenBlacklistService.cs # Lista negra de tokens
│   │   │   ├── UseCaseLogger.cs       # Logger de ações
│   │   │   └── WhatsappHub.cs         # Hub SignalR (tempo real)
│   │   └── usecases/                  # Casos de uso (lógica de negócio)
│   │       ├── interfaces/            # Interfaces dos use cases
│   │       └── usecases/              # Implementações
│   ├── Program.cs                     # Ponto de entrada da aplicação
│   ├── appsettings.json               # Configurações
│   └── multiwhats-api.csproj          # Projeto .NET
├── messageria/                        # Serviço Node.js (WhatsApp bridge)
├── multiwhats-front/                  # Frontend Next.js
├── docker-compose.yml
├── README.md
└── .env
```
