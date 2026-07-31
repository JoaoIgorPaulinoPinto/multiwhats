# MultiWhats — Versão 1.0

Documentação oficial de implantação, configuração e execução do sistema MultiWhats (plataforma de atendimento multi-clientes via WhatsApp).

---

## 1. Visão Geral da Arquitetura

O sistema é composto por **4 serviços** que trabalham juntos:

```
┌──────────────┐   HTTP   ┌──────────────┐   HTTP   ┌──────────────┐
│   Frontend   │ ───────▶ │   Backend    │ ───────▶ │  Messageria  │
│   Next.js    │  :3000   │ ASP.NET Core │  :5261   │  Node.js     │
└──────────────┘          └──────┬───────┘          │   :3333      │
                                 │                   └──────┬───────┘
                                 ▼                          ▼
                          ┌────────────┐            WhatsApp Web.js
                          │ PostgreSQL │          (Puppeteer/Chromium)
                          │  :5432     │
                          └────────────┘
```

| # | Serviço | Tecnologia | Porta | Pasta |
|---|---------|-----------|-------|-------|
| 1 | **Frontend** | Next.js 16 + React 19 + Zustand + SignalR | 3000 | `multiwhats-front/` |
| 2 | **Backend (API)** | ASP.NET Core 10 (C#) + EF Core 10 + Npgsql | 5261 | `multiwhats-api/multiwhats-api/` |
| 3 | **Messageria** | Node.js + whatsapp-web.js + Puppeteer | 3333 | `multiwhats-api/messageria/` |
| 4 | **LegacyDB Adapter** *(opcional)* | Node.js + Express + mysql2 (banco legado MySQL) | 3001 | `multiwhats-api/legacydatabaseadapter/` |

**Observação:** o banco principal é **PostgreSQL**. A connection string `Xampp` (MySQL) no `appsettings.json` é apenas para o sync legado, que está desativado no código.

---

## 2. O que precisa estar instalado no computador

| Requisito | Versão | Motivo |
|-----------|--------|--------|
| **.NET SDK** | **10.x** | Backend ASP.NET Core (`net10.0`) |
| **Node.js** | 18+ (recomendado 20 LTS) | Messageria e LegacyDB Adapter |
| **PostgreSQL** | 14+ (testado com 18) | Banco de dados principal |
| **Google Chrome** | versão atual | Execução do WhatsApp Web via Puppeteer (não-headless exige o Chrome instalado) |
| **npm** | 9+ | Instalação das dependências Node |

**Recomendado (facultativo):**
- **Docker Desktop** — para subir todos os serviços com um comando (seção 5)
- **pgAdmin** ou outro cliente PostgreSQL — inspeção do banco

**Importante:** o WhatsApp Web.js exige **Chrome instalado no caminho padrão**:
- Windows: `C:\Program Files\Google\Chrome\Application\chrome.exe`
- Docker/Linux: `/usr/bin/chromium` (já configurado no `docker-compose.yml`)

---

## 3. Configurações necessárias

### 3.1 Backend — `multiwhats-api/multiwhats-api/appsettings.json`

```jsonc
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=multiwhats_db;Username=postgres;Password=12345678;",
    "Xampp": "Server=localhost;Port=3306;Database=multiwhats;User=root;Password=12345678;" // legado (desativado)
  },
  "JwtSettings": {
    "Secret": "SuaChaveSecretaSuperPoderosaEALeatoriaComMaisDe32Caracteres!",
    "Issuer": "MinhaApiEmissor",
    "Audience": "MeuAppCliente",
    "ExpiryInMinutes": 60
  },
  "Auth":     { "PasswordMinLength": 6, "RequireRegistrationCode": false, "RegistrationCodeExpiryHours": 48, "DefaultUserRole": "Support" },
  "Occurrence": { "StatusFlow": ["Open", "InProgress", "Resolved", "Closed"] },
  "Media":    { "AllowedTypes": ["Image", "Audio", "Video", "Document", "Sticker"], "MaxSizeMB": 50 },
  "Messageria": { "BaseUrl": "http://localhost:3333" },
  "LegacyDb": { "BaseUrl": "http://localhost:3001" }
}
```

| Variável | Descrição |
|----------|-----------|
| `DefaultConnection` | Conexão com o PostgreSQL (banco principal) |
| `JwtSettings:Secret` | Chave de assinatura do JWT (troque em produção!) |
| `Messageria:BaseUrl` | URL da messageria. Em Docker use `http://messageria:3333` |
| `Auth:*`, `Occurrence:*`, `Media:*` | Valores **iniciais** — sobrescritos pelos parâmetros dinâmicos do banco (`system_parameters`) |

### 3.2 Parâmetros dinâmicos (banco `system_parameters`)

O sistema **semeia** os parâmetros no banco ao iniciar e os usa em tempo real. São editáveis na tela **Configurações → Parâmetros do sistema** (requer role Admin/Dev):

| Chave | Default | Grupo | Uso |
|-------|---------|-------|-----|
| `Auth:PasswordMinLength` | 6 | Auth | Tamanho mínimo de senha (register/update) |
| `Auth:RequireRegistrationCode` | true | Auth | Exige código de permissão no cadastro |
| `Auth:RegistrationCodeExpiryHours` | 48 | Auth | Validade do código de permissão |
| `Occurrence:StatusFlow` | Open→InProgress→Resolved→Closed | Ocorrência | Fluxo de status (avançar/retroceder) |
| `Media:AllowedTypes` | Image,Audio,Video,Document,Sticker | Mídia | Tipos de mídia aceitos (envio e recebimento) |
| `Media:UnsupportedMessage` | (texto padrão) | Mídia | Auto-resposta para mídia não suportada |
| `Business:Enabled` | false | Horário | Ativa auto-resposta fora do horário de funcionamento |
| `Business:OpenTime` / `CloseTime` | 08:00 / 18:00 | Horário | Horário de funcionamento |
| `Business:WorkingDays` | Seg–Sex | Horário | Dias de atendimento |
| `Business:OutsideHoursMessage` | (texto padrão) | Horário | Auto-resposta fora do horário (suporta `{open}`, `{close}`, `{days}`) |
| `Business:Timezone` | America/Sao_Paulo | Horário | Fuso para calcular horário comercial |
| `Replies:SenderName` | (vazio = nome do usuário) | Respostas automáticas | Nome exibido como remetente nas auto-respostas |

### 3.3 Frontend — variáveis de ambiente

| Variável | Valor | Onde é usada |
|----------|-------|--------------|
| `NEXT_PUBLIC_API_URL` | `http://localhost:5261` | Cliente HTTP (`src/services/api.ts`) e SignalR (`src/services/websocket.ts`). É **embutida no bundle no build** — em Docker deve ser passada via `--build-arg`/`ENV`. |

Não existem arquivos `.env` no frontend; o fallback em código já aponta para `http://localhost:5261`.

### 3.4 Messageria — variáveis de ambiente

| Variável | Default | Descrição |
|----------|---------|-----------|
| `PORT` | 3333 | Porta HTTP da messageria |
| `ASPNET_WEBHOOK_URL` | `http://localhost:5261/api/webhook/whatsapp` | Para onde envia mensagens recebidas |
| `ASPNET_DEVICE_URL` | `http://localhost:5261/api/device` | Para onde envia os dados do dispositivo conectado |
| `CHROME_PATH` | `C:\Program Files\Google\Chrome\Application\chrome.exe` | Caminho do Chrome (no Docker: `/usr/bin/chromium`) |

> **Atenção (importante):** o `package.json` da messageria fixa `"whatsapp-web.js": "1.34.6"` (sem `^`). A versão **1.34.7 quebra o `downloadMedia()`** e o acesso a `window.Store` — não atualize essa dependência. Os patches em `patches/` são aplicados no `postinstall` (`npm install`).

### 3.5 LegacyDB Adapter — variáveis de ambiente

| Variável | Default | Descrição |
|----------|---------|-----------|
| `PORT` | 3001 | Porta HTTP |
| `DB_HOST` | localhost | Host do MySQL legado |
| `DB_PORT` | 3306 | Porta do MySQL legado |
| `DB_USER` / `DB_PASSWORD` / `DB_DATABASE` | root / "" / multiwhats_db | Credenciais do MySQL legado |

---

## 4. Etapas para rodar (sem Docker — desenvolvimento local)

### Passo 1 — Banco de dados PostgreSQL

Crie o banco e o usuário (ajuste a senha conforme `appsettings.json`):

```sql
CREATE USER postgres WITH PASSWORD '12345678';
CREATE DATABASE multiwhats_db OWNER postgres;
```

> Se o PostgreSQL já existir com o banco `multiwhats_db`, basta garantir que a connection string do `appsettings.json` bate.

### Passo 2 — Backend (API)

```bash
cd multiwhats-api/multiwhats-api

# 1. Restaurar dependências
dotnet restore

# 2. Aplicar as migrations no banco (cria todas as tabelas)
dotnet ef database update

# 3. Rodar a API (porta 5261)
dotnet run
```

Verificações:
- Swagger em `http://localhost:5261/swagger`
- Ao iniciar, o sistema **semeia** os parâmetros de configuração (`system_parameters`) e carrega o cache.

> Se o `dotnet ef database update` falhar pedindo o comando EF, instale a ferramenta:
> ```bash
> dotnet tool install --global dotnet-ef
> ```

### Passo 3 — Messageria (WhatsApp)

```bash
cd multiwhats-api/messageria

# 1. Instalar dependências (executa postinstall que aplica os patches)
npm install

# 2. Rodar (porta 3333)
npm start
```

Ao iniciar, o terminal exibe um **QRCode** para parear o WhatsApp. Escaneie com o celular (WhatsApp → Aparelhos conectados → Conectar aparelho).

Depois de conectado (`ready`), a messageria registra o dispositivo no backend (`POST /api/device`).

### Passo 4 — Frontend

```bash
cd multiwhats-front

# 1. Instalar dependências
npm install

# 2. Rodar em desenvolvimento (porta 3000)
npm run dev
```

Acesse `http://localhost:3000`.

### Passo 5 — Primeiro usuário

O cadastro inicial pode exigir um **código de permissão** (se `Auth:RequireRegistrationCode` estiver `true`). Nesse caso:

1. No banco, ajuste temporariamente o parâmetro: `UPDATE system_parameters SET value = 'false' WHERE key = 'Auth:RequireRegistrationCode';` e rode `POST /api/admin/config/reload`.
2. Ou crie o usuário via `POST /api/auth/register` com um código gerado por um Admin/Dev (tela Configurações → "Gerar código de permissão").

> **Credencial de teste usada no desenvolvimento:** `Joao Igor` / `123123` (role Admin). Senhas são armazenadas com **hash BCrypt** (a partir da v1.0).

### Atalho — `iniciar.bat` (Windows)

A raiz do repositório tem `iniciar.bat`, que abre 4 janelas de terminal (API, Messageria, LegacyDB Adapter e Front). **Requisitos:** dependências já instaladas (`dotnet restore`, `npm install` nos 3 projetos Node) e PostgreSQL rodando.

---

## 5. Rodando com Docker

### Pré-requisitos
- Docker Desktop instalado
- PostgreSQL acessível (o compose **não** sobe o banco — ele é externo)

### Passo 1 — arquivo `.env` na raiz do projeto

Crie um arquivo `.env` na raiz com:

```env
DB_CONNECTION_STRING=Host=host.docker.internal;Port=5432;Database=multiwhats_db;Username=postgres;Password=12345678;
JWT_SECRET=UMA_CHAVE_SECRETA_MUITO_LONGA_COM_MAIS_DE_32_CARACTERES!
```

### Passo 2 — subir os serviços

```bash
docker compose up --build
```

| Serviço | URL |
|---------|-----|
| Frontend | `http://localhost:3000` |
| Backend (Swagger) | `http://localhost:5261/swagger` |
| Messageria | `http://localhost:3333` |
| LegacyDB Adapter | `http://localhost:3001` |

### Comandos úteis (Docker)

```bash
docker compose up --build -d   # em background
docker compose logs -f backend
docker compose logs -f messageria
docker compose down            # parar
docker compose down --rmi local
```

**Atenção Docker:** o backend envia mensagens para a messageria via `Messageria__BaseUrl=http://messageria:3333` (definido no compose). Em Docker, o QRCode de pareamento é exibido nos logs da messageria.

---

## 6. Fluxos e Lógicas Principais

### 6.1 Fluxo de uma mensagem

**Envio (operador → cliente):**
```
Frontend (tela de chat)
   → POST /api/messages/send (JWT Bearer)
   → SendMessageUseCase (strategy por tipo: texto/imagem/vídeo/áudio/documento/sticker)
   → POST Messageria /api/enviar
   → whatsapp-web.js envia via WhatsApp Web
   → salva Message (Outgoing) no PostgreSQL + notifica front via SignalR ("MessageSent")
```

**Recebimento (cliente → operador):**
```
whatsapp-web.js recebe a mensagem
   → POST /api/webhook/whatsapp (público)
   → SaveIncomingMessageUseCase:
      1. Dedup por messageId (índice único whatsapp_message_id)
      2. Detecta self-sent (mensagem enviada por este dispositivo)
      3. Cria/vincula Chat e Contact automaticamente
      4. Bloqueia mídia não suportada → envia auto-resposta (Media:UnsupportedMessage)
      5. Se fora do horário comercial (Business:*), envia auto-resposta
   → salva Message (Incoming) + broadcast SignalR ("MessageReceived")
```

### 6.2 Autenticação e Autorização

- Login: `POST /api/auth/login` → retorna **JWT** (válido por `JwtSettings:ExpiryInMinutes`, default 60 min).
- Senhas: **hash BCrypt** (work factor 12). Senhas legadas em texto puro são migradas automaticamente no primeiro login.
- O token é enviado no header `Authorization: Bearer {token}`.
- **Logout**: o JTI do token entra numa blacklist em memória (`TokenBlacklistService`) — o token deixa de ser aceito.
- **Roles**: `Support` (operações gerais), `Dev` e `Admin` (tudo). Tela de Configurações e edição de usuários exigem **Admin/Dev**.
- A cada request, o backend relê role/status do usuário no banco (mudanças valem sem novo login).

### 6.3 Soft Delete

Todas as entidades de negócio (Client, Chat, Contact, Message, Occurrence, ClientTask, Group, User) herdam `BaseEntity` com `IsDeleted`. O `AppDbContext.ApplyAudit()`:
- Converte `DELETE` em `UPDATE is_deleted = true`
- Aplica **global query filters** (`HasQueryFilter(e => !e.IsDeleted)`) — registros deletados somem de todas as consultas
- Preenche `CreatedAt`, `LastUpdate`, `CreatedByUserId`, `LastUpdatedByUserId`

### 6.4 Auditoria

- `AppDbContext.GenerateAuditLogs()` gera `AuditLog` com valores antigos/novos em JSON (o campo `Password` é excluído).
- `UseCaseLogger` registra ações de negócio com detalhes e transmite via SignalR.
- Logs coloridos no console da API.

### 6.5 Respostas automáticas

Disparadas no `SaveIncomingMessageUseCase` quando:
1. **Mídia não suportada** (áudio sempre bloqueado; demais tipos conforme `Media:AllowedTypes`)
2. **Fora do horário comercial** (se `Business:Enabled`)

Ambas usam o remetente configurável `Replies:SenderName` (vazio = nome do usuário logado). O nome é prefixado no texto como `_*{nome}*_`.

### 6.6 Configuração dinâmica (SystemConfigService)

- Parâmetros ficam na tabela `system_parameters` (editáveis na tela de Configurações).
- `SeedDefaultParametersAsync()` roda no startup; `LoadAsync()` preenche o cache (TTL 5 min).
- Toda alteração pela API (`PUT /api/admin/config/{key}`) chama `InvalidateCache()`, aplicando a mudança **sem reiniciar**.

---

## 7. Dependências dos projetos

### Backend (`multiwhats-api/multiwhats-api.csproj`)
| Pacote | Versão |
|--------|--------|
| Microsoft.AspNetCore.Authentication.JwtBearer | 10.0.10 |
| Microsoft.EntityFrameworkCore | 10.0.4 |
| Microsoft.EntityFrameworkCore.Tools | 10.0.4 |
| Microsoft.EntityFrameworkCore.Design | 10.0.4 |
| Microsoft.EntityFrameworkCore.Relational | 10.0.4 |
| Npgsql.EntityFrameworkCore.PostgreSQL | 10.0.3 |
| Swashbuckle.AspNetCore | 7.3.1 |
| **BCrypt.Net-Next** | **4.0.3** *(hash de senha)* |

### Frontend (`multiwhats-front/package.json`)
| Pacote | Versão |
|--------|--------|
| next | 16.2.10 |
| react / react-dom | 19.2.4 |
| @microsoft/signalr | ^10.0.0 |
| zustand | ^5.0.14 |
| lucide-react | ^1.25.0 |
| typescript | ^5 |
| eslint / eslint-config-next | ^9 / 16.2.10 |

### Messageria (`multiwhats-api/messageria/package.json`)
| Pacote | Versão |
|--------|--------|
| whatsapp-web.js | **1.34.6 (fixa, não usar 1.34.7)** |
| express | ^5.2.1 |
| axios | ^1.18.1 |
| qrcode-terminal | ^0.12.0 |
| puppeteer | 24.38.0 (bundled via whatsapp-web.js) |

### LegacyDB Adapter (`legacydatabaseadapter/package.json`)
| Pacote | Versão |
|--------|--------|
| express | ^5.1.0 |
| mysql2 | ^3.11.5 |
| dotenv | ^16.4.7 |
| tsx / typescript | ^4.19.0 / ^5.7.0 |

---

## 8. Comandos úteis

### Backend
```bash
dotnet restore                     # restaurar pacotes
dotnet build                       # compilar
dotnet run                         # rodar na porta 5261
dotnet ef migrations add Nome      # criar migration
dotnet ef database update          # aplicar migrations
dotnet ef migrations remove        # desfazer última migration
```

### Frontend
```bash
npm install        # instalar dependências
npm run dev        # dev (porta 3000)
npm run build      # build de produção
npm run lint       # lint (ESLint)
npx tsc --noEmit   # checagem de tipos
```

### Messageria
```bash
npm install        # instala e aplica patches (postinstall)
npm start          # rodar na porta 3333
```

---

## 9. Endpoints da API (resumo)

| Método | Endpoint | Descrição | Auth |
|--------|----------|-----------|------|
| POST | `/api/auth/register` | Cadastro de usuário | Pública |
| POST | `/api/auth/login` | Login → JWT | Pública |
| POST | `/api/auth/logout` | Revoga token | Autenticado |
| POST | `/api/auth/codes` | Gera código de permissão | Admin/Dev |
| GET/POST | `/api/clients` | Listar / criar clientes | Autenticado |
| GET/PUT/DELETE | `/api/clients/{id}` | Detalhar / editar / soft-deletar | Autenticado |
| GET | `/api/clients/{id}/contacts` | **Contatos do cliente** | Autenticado |
| GET/POST | `/api/contacts` | Listar / criar contatos | Autenticado |
| PATCH | `/api/contacts/{id}/assign` · `/unassign` | Vincular/desvincular cliente | Autenticado |
| GET | `/api/chats` · `/api/chats/{id}` | Listar / detalhar conversas | Autenticado |
| GET | `/api/chats/{id}/messages` | Mensagens da conversa (paginado) | Autenticado |
| GET | `/api/chats/{id}/full-info` | Informações completas do chat | Autenticado |
| GET | `/api/chats/{id}/occurrences` | Ocorrências da conversa | Autenticado |
| PATCH | `/api/chats/merge?mergeJid&toJid` | Mesclar conversas | Autenticado |
| POST | `/api/messages/send` | Enviar mensagem (texto/mídia) | Autenticado |
| GET | `/api/messages` · `/api/messages/{id}` · `/api/messages/phone/{phone}` | Consultar mensagens | Autenticado |
| CRUD | `/api/occurrences` | Ocorrências | Autenticado |
| PATCH | `/api/occurrences/{id}/status` | Avançar/retroceder status | Autenticado |
| CRUD | `/api/tasks` | Tarefas | Autenticado |
| PATCH | `/api/tasks/{id}/status` | Status de tarefa | Admin/Dev |
| GET/PUT | `/api/users` | Listar / editar usuários | GET: logado · PUT: Admin/Dev |
| GET/PUT/POST | `/api/admin/config` | Parâmetros dinâmicos | **Admin/Dev** |
| GET/POST | `/api/device` | Dispositivo WhatsApp conectado | Pública (usada pela messageria) |
| POST | `/api/webhook/whatsapp` | Recebe mensagens da messageria | Pública (usada pela messageria) |

---

## 10. Correções incluídas nesta versão (v1.0)

A varredura de segurança/lógica encontrou e corrigiu:

| # | Problema | Correção |
|---|----------|----------|
| 1 | `/api/admin/config` **público** (`[Authorize]` comentado) | Adicionado `[Authorize(Roles = "Admin,Dev")]` |
| 2 | Senhas em **texto puro** no banco | Hash **BCrypt** (register/update/login) + migração automática de senhas legadas no primeiro login |
| 3 | `GET /api/clients/{id}/contacts` retornava o **próprio cliente** em vez dos contatos | Agora retorna os contatos do cliente (`ExecuteContacts`) |
| 4 | `DeleteAsync` usava `FindAsync`, que **ignora o filtro global de soft-delete** | Troca por `FirstOrDefaultAsync` (respeita `IsDeleted`) em todos os repositórios |
| 5 | URL da messageria **hardcoded** (`http://localhost:3333`) | Configurável via `Messageria:BaseUrl` (funciona em Docker com `messageria:3333`) |
| 6 | `iniciar.bat` chamava `node server.ts` sem `tsx` (quebrava o LegacyDB Adapter) | Usa `npm start` (`node --import tsx src/index.ts`) |
| 7 | `whatsapp-web.js ^1.34.6` podia instalar a 1.34.7 (quebrada) | Versão fixada em `1.34.6` |

### Pendências conhecidas (documentadas, não corrigidas nesta versão)

- Sincronização com banco MySQL legado (`LegacyDbSyncService.PostAsync`) está **comentada/desativada**.
- Migrações `FIXDUPLICATEDMESSAGES` e `AddSettingsToBD` estão **vazias** (não alteram o banco).
- Token JWT fica no `localStorage` do frontend (risco XSS — ok para ambiente controlado; em produção, avaliar cookies httpOnly).
- Logout do frontend não desconecta o WebSocket SignalR.
- `JWT Secret` e senha do banco estão em texto plano no `appsettings.json` (em produção, usar variáveis de ambiente/User Secrets).

---

## 11. Troubleshooting rápido

| Problema | Causa provável | Solução |
|----------|---------------|---------|
| `dotnet ef` não reconhecido | Ferramenta EF não instalada | `dotnet tool install --global dotnet-ef` |
| 500 ao abrir chat com mídia | whatsapp-web.js 1.34.7 instalado | Reinstalar com versão fixa: `npm install whatsapp-web.js@1.34.6` e `npm install` (aplica patches) |
| Mensagem não sai (backend) | Messageria fora do ar | Subir `npm start` na pasta messageria; checar `Messageria:BaseUrl` |
| QRCode não aparece | Chrome não encontrado | Definir `CHROME_PATH` para o caminho do Chrome |
| 401 no login | Senha errada ou usuário inativo | Verificar credenciais; usuários novos exigem código se `RequireRegistrationCode=true` |
| 403 ao acessar Configurações | Role do usuário é Support | Entrar como Admin/Dev |
| Config não reflete | Cache do SystemConfigService | Clicar "Recarregar cache" na tela de Configurações ou reiniciar a API |
| WebSocket desconecta | CORS/URL errada | Frontend e API na mesma máquina (localhost); `NEXT_PUBLIC_API_URL` correto |
