Aqui está a sua documentação totalmente formatada e estruturada em Markdown:MultiWhats API — Documentação da Arquitetura🛠 Stack TecnológicaBackend Principal: ASP.NET Core 10 (Web API + SignalR)Banco de Dados: MySQL (Hospedado no Railway via Pomelo Entity Framework Core)Serviço de Messageria: Node.js (messageria/) — WhatsApp Web.js + Express (Porta 3000)Autenticação: JWT Bearer Auth📐 Entidades e RelacionamentosClient (Empresa / Cliente)Id (PK)Name (Obrigatório)MainPhoneNumberStatus (ClientStatus: Active | Inactive)CreatedAt, LastUpdate, IsDeleted, CreatedByUserId, LastUpdatedByUserId└── ← Contacts[] (1:N — Um cliente possui vários números)└── ← ClientTasks[] (1:N — Demandas da empresa)Contact (Número de WhatsApp)Id (PK)Jid (5511999999999@c.us) [UNIQUE]PhoneNumber (5511999999999)Name (Nome salvo na agenda)PushName (Nome do perfil do WhatsApp)ProfilePicUrlIsBlocked, IsGroupLastMessageAtClientId? → Client (FK — Atrelável/desatrelável via PATCH)GroupId? → GroupCreatedByUserId → UserCreatedAt, LastUpdate, IsDeleted├── ← Messages[] (1:N)└── ← Occurrences[] (1:N — Chamados)Message (Mensagem)Id (PK)MessageId (ID serializado do WhatsApp, para deduplicação)FromJid, ToJid?, PhoneNumber, Body?Direction (Incoming | Outgoing)Type (Text | Image | Audio | Video | Document | Sticker | Contact | Location | Unknown)Timestamp (Unix) + SentAt (DateTime)NotifyName?HasMedia, MediaUrl?, MediaMimeType?, MediaFilename?, MediaSize?, MediaCaption?DeliveryStatus (Pending | Sent | Delivered | Read | Failed)IsForwardedContactId? → ContactUserId? → User (Quem enviou/recebeu)OccurrenceId? → Occurrence (Vínculo opcional com chamado)ReplyToId? → Message (Resposta a)CreatedAt, LastUpdate, IsDeletedOccurrence (Chamado do Número)Id (PK)Title, Description?Status (Open | InProgress | Resolved | Closed)Priority (Low | Medium | High | Urgent)ContactId → Contact (FK Obrigatório)AssignedToUserId? → UserCreatedByUserId → UserCreatedAt, LastUpdate, IsDeleted└── ← Messages[]ClientTask (Demanda da Empresa)Id (PK)Title, Description?Status (Open | InProgress | Completed | Cancelled)Priority (Low | Medium | High | Urgent)DueDate?ClientId → Client (FK)AssignedToUserId? → UserCreatedByUserId → UserCreatedAt, LastUpdate, IsDeletedGroupId (PK)Name, Description?WhatsAppGroupId?CreatedAt, LastUpdate, IsDeleted└── ← Members (Contact[])UserId (PK)Name [UNIQUE]PasswordRole (Support | Dev | Admin)IsActiveCreatedAt, LastUpdate, IsDeletedAuditLogId (PK)UserId?, UserName?, UserRole?EntityType (Ex: "Contact", "Message")EntityId?Action ("Created" | "Updated" | "Deleted")Description (Ex: "Created Contact #5 - Maria - 5511999999999")OldValues? (JSON)NewValues? (JSON)Timestamp🔢 EnumsMessageDirection: Incoming | OutgoingMessageType: Text | Image | Audio | Video | Document | Sticker | Contact | Location | UnknownDeliveryStatus: Pending | Sent | Delivered | Read | FailedOccurrenceStatus: Open | InProgress | Resolved | ClosedClientTaskStatus: Open | InProgress | Completed | CancelledPriority: Low | Medium | High | UrgentClientStatus: Active | InactiveUserRole: Support | Dev | Admin🌐 Rotas da APIAuthMétodoRotaAutenticaçãoDescriçãoPOST/api/auth/register[anon]Registrar usuárioPOST/api/auth/login[anon]Login (retorna JWT + user)POST/api/auth/logout[auth]Revoga tokenClientsMétodoRotaAutenticaçãoDescriçãoPOST/api/clients[auth]Criar clienteGET/api/clients[auth]Listar todosGET/api/clients/{id}[auth]Buscar por IDPUT/api/clients/{id}[auth]AtualizarDELETE/api/clients/{id}[auth]Deletar (soft delete)GET/api/clients/{id}/contacts[auth]Contatos do clienteContactsMétodoRotaAutenticaçãoDescriçãoPOST/api/contacts[auth]Criar contatoGET/api/contacts[auth]Listar todosGET/api/contacts/{id}[auth]Buscar por IDPUT/api/contacts/{id}[auth]Atualizar (nome, pushName, blocked)DELETE/api/contacts/{id}[auth]DeletarPATCH/api/contacts/{id}/assign[auth]Atrelar a um clientePATCH/api/contacts/{id}/unassign[auth]Desatrelar do clienteGET/api/contacts/{id}/messages[auth]Mensagens do contatoMessagesMétodoRotaAutenticaçãoDescriçãoPOST/api/messages/send[auth]Enviar mensagem WhatsAppOccurrencesMétodoRotaAutenticaçãoDescriçãoPOST/api/occurrences[auth]Criar ocorrênciaGET/api/occurrences[auth]Listar todasGET/api/occurrences/{id}[auth]Buscar por IDPUT/api/occurrences/{id}[auth]AtualizarDELETE/api/occurrences/{id}[auth]DeletarTasksMétodoRotaAutenticaçãoDescriçãoPOST/api/tasks[auth]Criar tarefaGET/api/tasks[auth]Listar todasGET/api/tasks/{id}[auth]Buscar por IDPUT/api/tasks/{id}[auth]AtualizarDELETE/api/tasks/{id}[auth]DeletarPATCH/api/tasks/{id}/status[Admin, Dev]Alterar statusWebhookMétodoRotaAutenticaçãoDescriçãoPOST/api/webhook/whatsapp[anon]Webhook do Node.js📦 Payloads DTOPOST /api/auth/registerJSON{
  "name": "Joao",
  "password": "123123"
}
POST /api/auth/loginJSON// Request
{
  "name": "Joao",
  "password": "123123"
}

// Response
{
  "token": "...",
  "user": {
    "id": 1,
    "name": "Joao",
    "role": "Support"
  }
}
POST /api/clientsJSON{
  "name": "Timontec",
  "mainPhoneNumber": "5515999999999"
}
POST /api/contactsJSON{
  "jid": "5515987654321@c.us",
  "phoneNumber": "5515987654321",
  "name": "Maria",
  "pushName": "Maria"
}
PATCH /api/contacts/{id}/assignJSON{
  "clientId": 1
}
POST /api/messages/sendJSON{
  "phoneNumber": "5515996880359",
  "text": "olá"
}
POST /api/occurrencesJSON{
  "title": "Problema boleto",
  "description": "...",
  "priority": "High",
  "contactId": 1
}
POST /api/tasksJSON{
  "title": "Relatório custos",
  "description": "...",
  "priority": "Medium",
  "clientId": 1
}
PATCH /api/tasks/{id}/status (Exclusivo Admin / Dev)JSON{
  "status": "InProgress"
}
POST /api/webhook/whatsappJSON{
  "from": "551...@c.us",
  "phoneNumber": "551...",
  "body": "...",
  "timestamp": 1712345678,
  "notifyName": "Maria",
  "messageType": "text",
  "hasMedia": false,
  "messageId": "true_abc123",
  "isForwarded": false,
  "userId": 1
}
🔐 AutorizaçãoRolePATCH /api/tasks/{id}/statusDemais EndpointsAdmin✅ Sim✅ SimDev✅ Sim✅ SimSupport❌ Não✅ SimRegra no JWT: Claim role = "Admin" | "Dev" | "Support".Implementação: Atributo [Authorize(Roles = "Admin,Dev")].🛡 AuditoriaA auditoria é acionada automaticamente ao final de requisições POST, PUT, PATCH e DELETE.Interceptor (AppDbContext.ApplyAudit):Preenche CreatedByUserId / LastUpdatedByUserId nas entidades.Converte deleção direta em Soft Delete (IsDeleted = true).AuditService (Middleware em Program.cs):Gera logs completos: quem executou, a ação realizada, e o estado anterior/novo em formato JSON.Salva as informações na tabela AuditLogs.Escreve no console com destaque visual:Plaintext[AUDIT] 12:34:56 | User "Joao" (Admin) | Created Contact #5 | ...
[AUDIT] 12:35:10 | User "Maria" (Support) | Updated Occurrence #3 | ...
🔄 Arquitetura (Fluxo de Dados)PlaintextFrontend / .http
       │
       ▼
ASP.NET Core API  (localhost:5261)
       │
       ├── Controllers ──> Use Cases ──> Repositories ──> DbContext ──> MySQL
       │
       ├── SignalR Hub (/whatsappHub) ──> Notifica web em tempo real
       │
       └── POST http://localhost:3000/api/enviar ──> Node.js (messageria)
                                                           │
                                                           ▼
                                               WhatsApp Web (Puppeteer)
Mensagens recebidas do WhatsApp:Node.js ➔ POST /api/webhook/whatsapp ➔ ASP.NET (Salva no BD + Emite via SignalR)💻 Comandos ÚteisBash# Sobe a API ASP.NET Core em http://localhost:5261
dotnet run

# Sobe o serviço Node.js em http://localhost:3000
cd messageria && npm start

# Cria uma nova migration do Entity Framework
dotnet ef migrations add NomeDaMigration

# Aplica as migrations pendentes no banco MySQL
dotnet ef database update
📋 Webhook DTO (Node.js ➔ ASP.NET)JSON{
  "from": "5511999999999@c.us",     // JID completo
  "phoneNumber": "5511999999999",    // Número limpo
  "body": "mensagem",
  "timestamp": 1712345678,
  "notifyName": "Nome do Contato",
  "messageType": "text",             // text, image, audio, video, document
  "hasMedia": false,
  "mediaUrl": null,                  // Base64 da mídia
  "mediaMimeType": null,
  "mediaFilename": null,
  "mediaSize": null,
  "messageId": "true_...",           // ID único do WhatsApp
  "isForwarded": false,
  "userId": 1                        // ID do usuário dono da conexão
}
