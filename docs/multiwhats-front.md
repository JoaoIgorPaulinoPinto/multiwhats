# Documentação Completa - MultiWhats Front-end

## Índice
1. [Visão Geral](#visão-geral)
2. [Tecnologias Utilizadas](#tecnologias-utilizadas)
3. [Arquitetura](#arquitetura)
4. [Estrutura de Diretórios](#estrutura-de-diretórios)
5. [Configuração de Tema (Light/Dark)](#configuração-de-tema-lightdark)
6. [Estado Global (Zustand)](#estado-global-zustand)
7. [Serviços de API](#serviços-de-api)
8. [WebSocket / SignalR (Tempo Real)](#websocket--signalr-tempo-real)
9. [Sistema de Roteamento](#sistema-de-roteamento)
10. [Tela de Login](#tela-de-login)
11. [Layout Autenticado](#layout-autenticado)
12. [NavBar (Navegação Lateral)](#navbar-navegação-lateral)
13. [Tela de Chats (Conversas)](#tela-de-chats-conversas)
14. [Tela de Contatos](#tela-de-contatos)
15. [Tela de Empresas (Companies)](#tela-de-empresas-companies)
16. [Tela de Kanban](#tela-de-kanban)
17. [Componentes Compartilhados](#componentes-compartilhados)
18. [Fluxos de Dados Completos](#fluxos-de-dados-completos)
19. [Configuração e Execução](#configuração-e-execução)
20. [Padrões e Convenções de Código](#padrões-e-convenções-de-código)

---

## Visão Geral

O **MultiWhats Front-end** é uma aplicação web moderna construída com **Next.js 16** que serve como interface de usuário para o sistema MultiWhats - um CRM integrado ao WhatsApp para gestão multi-empresas.

A interface permite:
- **Visualizar e gerenciar conversas do WhatsApp** em tempo real (receber e enviar mensagens)
- **Gerenciar contatos** (associar a empresas, editar, excluir)
- **Gerenciar empresas/clientes** (CRUD completo com contatos vinculados)
- **Visualizar quadro Kanban** de tarefas e ocorrências
- **Autenticação** com login/registro via JWT
- **Tema claro/escuro** com persistência em localStorage

---

## Tecnologias Utilizadas

| Tecnologia | Versão | Propósito |
|---|---|---|
| Next.js | 16.2.10 | Framework React com App Router |
| React | 19.2.4 | Biblioteca UI |
| TypeScript | 5.x | Tipagem estática |
| Zustand | 5.0.14 | Gerenciamento de estado global |
| @microsoft/signalr | 10.0.0 | WebSocket para tempo real |
| lucide-react | 1.25.0 | Biblioteca de ícones SVG |
| CSS Modules | - | Estilização encapsulada por componente |
| ESLint | 9.x | Linting |

---

## Arquitetura

### Padrão Arquitetural

O front-end utiliza uma **arquitetura baseada em componentes com separação de responsabilidades** inspirada no padrão **Feature-Sliced**:

```
┌────────────────────────────────────────────────────┐
│                 Next.js App Router                  │
│  (pages.tsx na pasta app/ definem as rotas)         │
├────────────────────────────────────────────────────┤
│               Screen Components                     │
│  (screens/chats, screens/contacts, etc.)            │
│  → Coordenam sub-componentes para formar uma tela   │
├────────────────────────────────────────────────────┤
│             Feature Components                       │
│  (chat-area, chat-sidebar, nav-bar, etc.)           │
│  → São divididos em: view + logic + module.css      │
├────────────────────────────────────────────────────┤
│               Services Layer                         │
│  (api.ts, auth.service.ts, chats.service.ts, etc.)  │
│  → Comunicação com API REST + WebSocket             │
├────────────────────────────────────────────────────┤
│              State Management                        │
│  (stores/auth-store.ts - Zustand)                   │
│  → Estado global de autenticação                    │
└────────────────────────────────────────────────────┘
```

### Separação View / Logic

Cada componente funcional complexo segue um padrão de **separação de concerns** em 3 arquivos:

```
componente/
├── componente.view.tsx      ← Renderização / JSX puro
├── componente.logic.tsx     ← Hooks personalizados com estado e lógica
└── componente.module.css    ← Estilos encapsulados
```

**view.tsx** - Contém apenas JSX, recebe props e chama callbacks. Nunca contém estado ou lógica de negócio.

**logic.tsx** - Exporta um hook personalizado (ex: `useChatArea()`, `useChatSidebar()`) que encapsula:
- Estado local (`useState`)
- Efeitos colaterais (`useEffect`)
- Chamadas de API
- Manipulação de eventos WebSocket
- Lógica de formulário

**Exemplo de uso no view:**
```tsx
function ChatAreaView(props: ChatAreaViewProps) {
    const { messages, sendMessage, ... } = useChatArea(props);
    return (
        <div>
            {messages.map(msg => <MessageBubble key={msg.id} ... />)}
            <InputArea onSend={sendMessage} />
        </div>
    );
}
```

---

## Estrutura de Diretórios

```
multiwhats-front/
├── public/                          ← Arquivos estáticos (SVG, favicon)
├── src/
│   ├── app/                         ← Next.js App Router
│   │   ├── globals.css              ← Estilos globais + variáveis CSS (tema)
│   │   ├── hydrator.tsx             ← Componente de hidratação do auth store
│   │   ├── layout.tsx               ← Root layout (fontes, tema, hydrator)
│   │   ├── page.tsx                 ← Home page (redirect condicional)
│   │   ├── login/
│   │   │   └── page.tsx             ← Rota /login
│   │   └── (authenticated)/         ← Route group (layout compartilhado)
│   │       ├── layout.tsx           ← Layout autenticado (NavBar + content)
│   │       ├── chats/
│   │       │   └── page.tsx         ← Rota /chats
│   │       ├── contacts/
│   │       │   └── page.tsx         ← Rota /contacts
│   │       ├── companies/
│   │       │   └── page.tsx         ← Rota /companies
│   │       └── kanban/
│   │           └── page.tsx         ← Rota /kanban
│   ├── components/                  ← Componentes React
│   │   ├── auth/
│   │   │   ├── login.view.tsx
│   │   │   ├── login.logic.tsx
│   │   │   └── login.module.css
│   │   ├── avatar/
│   │   │   ├── avatar.view.tsx
│   │   │   └── avatar.module.css
│   │   ├── chat-area/
│   │   │   ├── chat-area.view.tsx
│   │   │   ├── chat-area.logic.tsx
│   │   │   └── chat-area.module.css
│   │   ├── chat-sidebar/
│   │   │   ├── chat-sidebar.view.tsx
│   │   │   ├── chat-sidebar.logic.tsx
│   │   │   └── chat-sidebar.module.css
│   │   ├── nav-bar/
│   │   │   ├── nav-bar.view.tsx
│   │   │   └── nav-bar.module.css
│   │   ├── profile-popover/
│   │   │   ├── profile-popover.view.tsx
│   │   │   ├── profile-popover.logic.tsx
│   │   │   └── profile-popover.module.css
│   │   ├── theme-toggle/
│   │   │   ├── theme-toggle.view.tsx
│   │   │   ├── theme-toggle.logic.tsx
│   │   │   └── theme-toggle.module.css
│   │   └── screens/
│   │       ├── chats/
│   │       │   ├── chats.view.tsx
│   │       │   └── chats.module.css
│   │       ├── contacts/
│   │       │   ├── contacts.view.tsx
│   │       │   ├── contacts.logic.tsx
│   │       │   └── contacts.module.css
│   │       ├── companies/
│   │       │   ├── companies.view.tsx
│   │       │   ├── companies.logic.tsx
│   │       │   └── companies.module.css
│   │       └── kanban/
│   │           ├── kanban.view.tsx
│   │           ├── kanban.logic.tsx
│   │           └── kanban.module.css
│   ├── data/
│   │   └── mock-data.ts            ← Dados mockados (não utilizados atualmente)
│   ├── services/
│   │   ├── api.ts                   ← Cliente HTTP genérico (ApiClient)
│   │   ├── auth.service.ts          ← Serviço de autenticação
│   │   ├── chats.service.ts         ← Serviço de chats/mensagens
│   │   ├── companies.service.ts     ← Serviço de empresas (clients)
│   │   ├── contacts.service.ts      ← Serviço de contatos
│   │   ├── kanban.service.ts        ← Serviço de tarefas/ocorrências
│   │   ├── paginated.response.ts    ← Interface de resposta paginada
│   │   └── websocket.ts             ← Cliente SignalR (WebSocket)
│   └── stores/
│       └── auth-store.ts            ← Zustand store (autenticação)
├── .env                             ← Variáveis de ambiente
├── next.config.ts                   ← Configuração Next.js
├── tsconfig.json                    ← Configuração TypeScript
├── eslint.config.mjs                ← Configuração ESLint
└── package.json                     ← Dependências e scripts
```

---

## Configuração de Tema (Light/Dark)

### Sistema de Temas

O tema é controlado via **CSS Custom Properties** (variáveis CSS) no arquivo `globals.css`. Existem 3 estados:

1. **Tema Claro (padrão)**: Definido em `:root`
2. **Tema Escuro (manual)**: Definido em `[data-theme="dark"]`
3. **Tema Escuro (sistema)**: Definido em `@media (prefers-color-scheme: dark) { :root:not([data-theme]) }` - ativa se o sistema do usuário estiver em modo escuro e nenhum tema manual foi definido

### Variáveis CSS

```css
:root {
    --background: #f0f2f5;
    --surface: #ffffff;
    --surface-hover: #f5f6f8;
    --sidebar: #ffffff;
    --chat: #efeae2;
    --panel: #f0f2f5;
    --card: #ffffff;
    --card-hover: #f5f5f5;
    --border: #e0e0e0;
    --text: #111b21;
    --text-secondary: #667781;
    --text-muted: #8696a0;
    --input: #f0f2f5;
    --primary: #25d366;
    --primary-hover: #22c35e;
    --message-received: #ffffff;
    --message-sent: #d9fdd3;
}
```

O tema escuro substitui todas essas variáveis com cores escuras:
```css
[data-theme="dark"] {
    --background: #111b21;
    --surface: #1f2c33;
    --chat: #0b141a;
    --message-sent: #005c4b;
    --primary: #00a884;
    ...
}
```

### Injeção do Tema (layout.tsx)

Para evitar **flash de conteúdo** (FOUC) ao carregar a página, um script é injetado antes da hidratação do React:

```tsx
<script
    dangerouslySetInnerHTML={{
        __html: `
            (function() {
                var theme = localStorage.getItem('theme');
                if (theme === 'dark' || (!theme && window.matchMedia('(prefers-color-scheme: dark)').matches)) {
                    document.documentElement.setAttribute('data-theme', 'dark');
                }
            })();
        `
    }}
/>
```

Este script executa **antes** do React renderizar, garantindo que o tema correto seja aplicado imediatamente.

### ThemeToggle

O `theme-toggle` permite alternar manualmente entre claro/escuro:
1. Hook `useThemeToggle()`: lê `localStorage.getItem('theme')`
2. `toggle()`: alterna o valor, atualiza `data-theme` no `<html>`, persiste no `localStorage`

---

## Estado Global (Zustand)

### Auth Store (`stores/auth-store.ts`)

O único store global do sistema gerencia **autenticação**:

```typescript
interface AuthState {
    user: UserResponse | null;
    token: string | null;
    loading: boolean;
    error: string | null;
    
    login: (name: string, password: string) => Promise<void>;
    register: (name: string, password: string) => Promise<void>;
    logout: () => Promise<void>;
    hydrate: () => void;
}
```

### Fluxo de Login
1. Chama `authService.login(name, password)` → recebe `{ token, user }`
2. Salva `token` e `user` (JSON) no `localStorage`
3. Atualiza estado global (`set({ user, token, loading: false })`)

### Fluxo de Logout
1. Chama `authService.logout()` → revoga token no backend
2. Remove `token` e `user` do `localStorage`
3. Limpa estado global (`set({ user: null, token: null })`)

### Hidratação (`hydrator.tsx`)
No mount do app, `hydrate()` é chamada para restaurar `user` e `token` do `localStorage`, mantendo a sessão entre recarregamentos de página.

---

## Serviços de API

### ApiClient (`services/api.ts`)

Cliente HTTP genérico que encapsula `fetch`:

```typescript
class ApiClient {
    private baseUrl: string;
    
    constructor(baseUrl: string) {
        // usa NEXT_PUBLIC_API_URL ou fallback para localhost:5261
    }
    
    private getToken(): string | null {
        // lê token do localStorage
    }
    
    private async request<T>(method, path, body?): Promise<T> {
        // Adiciona headers: Content-Type, Authorization (Bearer token)
        // Serializa body como JSON
        // Converte erro em ApiError com status code
    }
    
    get<T>(path) { return this.request<T>('GET', path); }
    post<T>(path, body) { return this.request<T>('POST', path, body); }
    put<T>(path, body) { return this.request<T>('PUT', path, body); }
    patch<T>(path, body) { return this.request<T>('PATCH', path, body); }
    delete<T>(path) { return this.request<T>('DELETE', path); }
}
```

**Características:**
- Todas as requests incluem `Authorization: Bearer <token>` automaticamente
- Tratamento de erros: respostas não-OK são convertidas em `ApiError` com código de status
- `baseUrl` configurável via `NEXT_PUBLIC_API_URL` no `.env`

### Auth Service (`services/auth.service.ts`)

```typescript
export const authService = {
    login(name: string, password: string)  → POST /api/auth/login
    register(name: string, password: string) → POST /api/auth/register
    logout()  → POST /api/auth/logout
    me()  → GET /api/auth/me
};
```

**Retornos:**
- `login`: `LoginResponse` = `{ token: string, user: UserResponse }`
- `register`: `UserResponse` = `{ id, name, role, isActive, createdAt }`

### Chats Service (`services/chats.service.ts`)

```typescript
export const chatsService = {
    listChats(page?: number, pageSize?: number)  → GET /api/chats
    getChat(id: number)  → GET /api/chats/:id
    getMessages(chatId: number, page?: number, pageSize?: number)  → GET /api/chats/:id/messages
    getOccurrences(chatId: number)  → GET /api/chats/:id/occurrences
    sendMessage(jid: string, text: string)  → POST /api/messages/send
};
```

**Interfaces:**
```typescript
interface ChatResponse {
    id: number; jid: string; phoneNumber: string; name: string;
    contactId: number | null; contactName: string | null;
    clientId: number | null; clientName: string | null;
    lastMessageAt: string; lastMessageBody: string | null;
    assignedToUserId: number | null; assignedToUserName: string | null;
    unreadCount: number; createdAt: string; lastUpdate: string;
}

interface MessageResponse {
    id: number; messageId: string; fromJid: string; toJid: string;
    phoneNumber: string; body: string; direction: MessageDirection;
    type: MessageType; timestamp: number | null; sentAt: string | null;
    notifyName: string | null; hasMedia: boolean; mediaUrl: string | null;
    mediaMimeType: string | null; mediaFilename: string | null;
    mediaSize: number | null; mediaCaption: string | null;
    deliveryStatus: DeliveryStatus; isForwarded: boolean;
    chatId: number; userId: number | null; occurrenceId: number | null;
    replyToId: number | null; createdAt: string;
}

type MessageDirection = 0 | 1;  // 0 = Incoming, 1 = Outgoing
type MessageType = "Text" | "Image" | "Audio" | "Video" | "Document" | "Sticker" | "Contact" | "Location" | "Unknown";
type DeliveryStatus = "Pending" | "Sent" | "Delivered" | "Read" | "Failed";
```

### Contacts Service (`services/contacts.service.ts`)

```typescript
export const contactsService = {
    list()  → GET /api/contacts
    getById(id: number)  → GET /api/contacts/:id
    create(data: CreateContactRequest)  → POST /api/contacts
    update(id: number, data: UpdateContactRequest)  → PUT /api/contacts/:id
    delete(id: number)  → DELETE /api/contacts/:id
    assign(id: number, clientId: number)  → PATCH /api/contacts/:id/assign
    unassign(id: number)  → PATCH /api/contacts/:id/unassign
};
```

### Companies Service (`services/companies.service.ts`)

```typescript
export const companiesService = {
    list()  → GET /api/clients
    getById(id: number)  → GET /api/clients/:id
    create(data: CreateClientRequest)  → POST /api/clients
    update(id: number, data: UpdateClientRequest)  → PUT /api/clients/:id
    delete(id: number)  → DELETE /api/clients/:id
    listContacts(clientId: number)  → GET /api/clients/:id/contacts
    // unassignContact usa contactsService.unassign internamente
};
```

### Kanban Service (`services/kanban.service.ts`)

```typescript
export const kanbanService = {
    // Tarefas
    listTasks()  → GET /api/tasks
    getTask(id: number)  → GET /api/tasks/:id
    createTask(data: CreateTaskRequest)  → POST /api/tasks
    updateTask(id: number, data: UpdateTaskRequest)  → PUT /api/tasks/:id
    updateTaskStatus(id: number, status: number)  → PATCH /api/tasks/:id/status
    deleteTask(id: number)  → DELETE /api/tasks/:id
    
    // Ocorrências
    listOccurrences()  → GET /api/occurrences
    getOccurrence(id: number)  → GET /api/occurrences/:id
    createOccurrence(data: CreateOccurrenceRequest)  → POST /api/occurrences
    updateOccurrence(id: number, data: UpdateOccurrenceRequest)  → PUT /api/occurrences/:id
    deleteOccurrence(id: number)  → DELETE /api/occurrences/:id
};
```

### Paginated Response (`services/paginated.response.ts`)

```typescript
interface PaginatedResponse<T> {
    items: T[];
    totalCount: number;
    page: number;
    pageSize: number;
    totalPages: number;
    hasPrevious: boolean;
    hasNext: boolean;
}
```

---

## WebSocket / SignalR (Tempo Real)

### Cliente WebSocket (`services/websocket.ts`)

Cliente SignalR personalizado que gerencia conexão, autenticação e eventos:

```typescript
class WsClient {
    private connection: signalR.HubConnection | null = null;
    private listeners = Map<string, Set<(data: unknown) => void>>();
    private started = false;
    
    async start(): Promise<void>;
    on(event: string, callback: (data: any) => void): () => void;  // retorna unsubscribe
    async stop(): Promise<void>;
    
    private getToken(): string | null;
    private async ensureConnection(): Promise<void>;
    private emit(event: string, data: unknown): void;
}
```

### Configuração da Conexão

```typescript
this.connection = new signalR.HubConnectionBuilder()
    .withUrl(`${baseUrl}/whatsappHub`, {
        accessTokenFactory: () => this.getToken(),
        transport: signalR.HttpTransportType.WebSockets,
    })
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
    .build();
```

**Reconnect automático:** Tenta reconectar em intervalos progressivos: 0s, 2s, 5s, 10s, 30s.

### Eventos Suportados

| Evento | Tipo | Disparado Por | Payload |
|---|---|---|---|
| `message:raw` | ReceberNovaMensagem | API (webhook) | MessageResponse |
| `message:received` | MessageReceived | API (webhook) | MessageResponse |
| `message:sent` | MessageSent | API (envio) | MessageResponse |

### Padrão de Uso nos Componentes

```typescript
useEffect(() => {
    const unsub = ws.on('message:received', (data: any) => {
        setMessages(prev => [...prev, data as MessageResponse]);
    });
    return () => unsub();  // cleanup no unmount
}, []);
```

### Lazy Start

A conexão só é estabelecida quando o primeiro listener é registrado:

```typescript
on(event: string, callback) {
    if (!this.started) {
        this.started = true;
        this.ensureConnection();
    }
    // registra callback
}
```

---

## Sistema de Roteamento

### Estrutura App Router

```
src/app/
├── layout.tsx                    ← RootLayout (fontes, tema, hydrator)
├── page.tsx                      ← / (redireciona condicionalmente)
├── login/
│   └── page.tsx                  ← /login
└── (authenticated)/              ← Route group
    ├── layout.tsx                ← Layout compartilhado (navbar + content)
    ├── chats/
    │   └── page.tsx              ← /chats
    ├── contacts/
    │   └── page.tsx              ← /contacts
    ├── companies/
    │   └── page.tsx              ← /companies
    └── kanban/
        └── page.tsx              ← /kanban
```

### Guardas de Autenticação

**Home (`/`):**
```typescript
const user = useAuthStore(s => s.user);
const loading = useAuthStore(s => s.loading);
useEffect(() => {
    if (!loading) {
        router.replace(user ? '/chats' : '/login');
    }
}, [user, loading]);
```

**Login (`/login`):**
```typescript
const user = useAuthStore(s => s.user);
useEffect(() => {
    if (user) router.replace('/chats');
}, [user]);
```

**Authenticated Layout (`(authenticated)/layout.tsx`):**
```typescript
const user = useAuthStore(s => s.user);
useEffect(() => {
    if (!user) router.replace('/login');
}, [user]);
```

### Navegação

A navegação é feita exclusivamente via `useRouter().push()` (não usa `<Link>` do Next.js).

---

## Tela de Login

### `/login` → `auth/login.view.tsx`

**Funcionalidades:**
- Alternância entre modo **Login** e **Registro**
- Campos: Nome de usuário, Senha
- Botão de submit com ícone dinâmico (LogIn ou UserPlus)
- Indicador de loading
- Exibição de erros (credenciais inválidas, usuário já existe, etc.)

### Hook `useLogin()` (`login.logic.tsx`)

```typescript
function useLogin() {
    const [mode, setMode] = useState<'login' | 'register'>('login');
    const [name, setName] = useState('');
    const [password, setPassword] = useState('');
    const [error, setError] = useState<string | null>(null);
    
    const handleSubmit = async () => {
        if (mode === 'register') {
            await authStore.register(name, password);
        }
        await authStore.login(name, password);
    };
    
    const toggleMode = () => {
        setMode(m => m === 'login' ? 'register' : 'login');
        setError(null);
    };
    
    return { mode, name, setName, password, setPassword, error, handleSubmit, toggleMode };
}
```

### Layout Visual

- Cartão centralizado com 400px de largura
- Ícone verde (WhatsApp) no topo
- Inputs estilizados com ícones internos (User, Lock)
- Botão verde com espaçamento adequado
- Link para alternar entre login/registro

---

## Layout Autenticado

### `(authenticated)/layout.tsx`

```tsx
function AuthenticatedLayout({ children }: { children: React.ReactNode }) {
    const user = useAuthStore(s => s.user);
    const navigate = useRouter();
    
    useEffect(() => {
        if (!user) navigate.replace('/login');
    }, [user]);
    
    if (!user) return <div className={styles.spinner} />;
    
    return (
        <div className={styles.layout}>
            <NavBarView />
            {children}
        </div>
    );
}
```

**Estrutura:**
- `display: flex; height: 100vh`
- NavBar (60px largura) à esquerda
- Conteúdo da rota à direita (flex: 1)

---

## NavBar (Navegação Lateral)

### `nav-bar.view.tsx`

Barra de navegação vertical fixa com 60px de largura:

| Ícone | Rota | Descrição |
|---|---|---|
| MessageSquare | `/chats` | Conversas WhatsApp |
| Users | `/contacts` | Contatos |
| LayoutDashboard | `/kanban` | Kanban |
| Building2 | `/companies` | Empresas |

**Recursos:**
- Destaque do item ativo via `usePathname()`
- Efeitos hover e active nos botões
- Avatar do usuário no rodapé (abre profile popover)

### Profile Popover

Ao clicar no avatar, abre um popover com:
- **Avatar** com iniciais do usuário
- **Nome** e **Role** do usuário
- **ThemeToggle** (alternar modo claro/escuro)
- **Botão de Logout** (vermelho no hover)

---

## Tela de Chats (Conversas)

### `/chats` → `screens/chats/chats.view.tsx`

A tela principal de conversas é dividida em **duas colunas**:

```
┌──────────────────────────────────────────────────┐
│  Sidebar (450px)        │  Chat Area (flex: 1)   │
│                         │                        │
│  ┌─────────────────┐    │  ┌──────────────────┐  │
│  │ Buscar conversa │    │  │ <header>         │  │
│  ├─────────────────┤    │  │ Avatar + Nome    │  │
│  │ Chat 1          │    │  │ Status           │  │
│  │ Chat 2          │    │  ├──────────────────┤  │
│  │ Chat 3          │    │  │ Mensagens        │  │
│  │ ...             │    │  │ (bolhas)         │  │
│  └─────────────────┘    │  │                  │  │
│                         │  ├──────────────────┤  │
│                         │  │ Input de texto   │  │
│                         │  └──────────────────┘  │
└──────────────────────────────────────────────────┘
```

### ChatSidebar (`chat-sidebar/`)

**Hook `useChatSidebar()`:**
- **Polling**: Busca chats a cada **5 segundos** via `chatsService.listChats()`
- **WebSocket**: Atualiza a lista imediatamente quando recebe `message:received` ou `message:sent`
- **Filtro**: Campo de busca filtra no cliente-side por nome ou telefone
- **Seleção**: `onSelect(id, name, phone, jid, contactId)` é chamado ao clicar em um chat

```typescript
function useChatSidebar() {
    const [search, setSearch] = useState('');
    const [chats, setChats] = useState<ChatResponse[]>([]);
    const [loading, setLoading] = useState(true);
    
    // Polling a cada 5s
    useEffect(() => {
        const fetch = () => chatsService.listChats().then(r => setChats(r.items));
        fetch();
        const interval = setInterval(fetch, 5000);
        return () => clearInterval(interval);
    }, []);
    
    // WebSocket refresh
    useEffect(() => {
        const refresh = () => chatsService.listChats().then(r => setChats(r.items));
        const unsub1 = ws.on('message:received', refresh);
        const unsub2 = ws.on('message:sent', refresh);
        return () => { unsub1(); unsub2(); };
    }, []);
    
    // Filtro client-side
    const filtered = chats.filter(c =>
        c.name?.toLowerCase().includes(search.toLowerCase()) ||
        c.phoneNumber?.includes(search)
    );
    
    return { search, setSearch, chats: filtered, loading };
}
```

### ChatArea (`chat-area/`)

**Hook `useChatArea()`:**

```typescript
function useChatArea(chatId: number, jid: string) {
    const [messages, setMessages] = useState<MessageResponse[]>([]);
    const [loading, setLoading] = useState(true);
    
    // Cache de mensagens por chatId
    const cache = new Map<number, MessageResponse[]>();
    
    // Carrega mensagens quando chatId muda
    useEffect(() => {
        if (cache.has(chatId)) {
            setMessages(cache.get(chatId)!);
        } else {
            chatsService.getMessages(chatId).then(r => {
                cache.set(chatId, r.items);
                setMessages(r.items);
            });
        }
    }, [chatId]);
    
    // Escuta novas mensagens via WebSocket
    useEffect(() => {
        const onReceived = (data: any) => {
            const msg = data as MessageResponse;
            if (msg.chatId === chatId) {
                setMessages(prev => [...prev, msg]);
                cache.get(chatId)?.push(msg);
            }
        };
        const unsub1 = ws.on('message:received', onReceived);
        const unsub2 = ws.on('message:sent', onReceived);
        return () => { unsub1(); unsub2(); };
    }, [chatId]);
    
    // Envia mensagem
    const sendMessage = async (text: string) => {
        await chatsService.sendMessage(jid, text);
        cache.delete(chatId); // limpa cache para recarregar
    };
    
    return { messages, loading, sendMessage };
}
```

**Recursos do ChatArea:**
- **Bolhas de mensagem**: Estilo diferente para mensagens recebidas (esquerda, fundo branco) e enviadas (direita, fundo verde)
- **Avatar**: Exibe iniciais do contato no topo
- **Input**: Campo de texto com botão de enviar verde
- **Salvar contato**: Se o contato não estiver associado a nenhum cliente, exibe botão "Salvar nos contatos" que abre modal com formulário
- **Modal de Salvar Contato**: Campos JID (readonly), Telefone, Nome, PushName (readonly), Empresa (select)

---

## Tela de Contatos

### `/contacts` → `screens/contacts/contacts.view.tsx`

**Hook `useContacts()`:**

```typescript
function useContacts() {
    // Carrega contatos + empresas no mount
    const [contacts, setContacts] = useState<ContactResponse[]>([]);
    const [companies, setCompanies] = useState<ClientResponse[]>([]);
    const [search, setSearch] = useState('');
    const [editingContact, setEditingContact] = useState<ContactResponse | null>(null);
    
    // CRUD
    const startEdit = (contact: ContactResponse) => setEditingContact(contact);
    const cancelEdit = () => setEditingContact(null);
    const saveEdit = async () => {
        await contactsService.update(editingContact.id, { ... });
        if (selectedCompany) {
            await contactsService.assign(editingContact.id, selectedCompany);
        }
        // recarrega lista
    };
    const handleDelete = async (id: number) => {
        await contactsService.delete(id);
        setContacts(prev => prev.filter(c => c.id !== id));
    };
    
    return { contacts: filtered, companies, search, setSearch, startEdit, cancelEdit, saveEdit, ... };
}
```

**Layout:**
- Sidebar (380px): Lista de contatos com busca, botão "Novo" (desabilitado - mensagem "Salve um contato a partir do chat")
- Cada item: avatar, nome, empresa (se associado), botões editar/excluir
- Main panel: Estado vazio (ícone Phone, mensagem "Selecione um contato")
- Modal de edição: Nome (editável), PushName (readonly), Empresa (select)

---

## Tela de Empresas (Companies)

### `/companies` → `screens/companies/companies.view.tsx`

**Hook `useCompanies()`:**

```typescript
function useCompanies() {
    const [companies, setCompanies] = useState<ClientResponse[]>([]);
    const [allContacts, setAllContacts] = useState<ContactResponse[]>([]);
    const [editingCompany, setEditingCompany] = useState<ClientResponse | null>(null);
    
    // CRUD
    const createNewCompany = async (data) => {
        const newCompany = await companiesService.create(data);
        setCompanies(prev => [...prev, newCompany]);
    };
    const saveEdit = async (data) => {
        const updated = await companiesService.update(editingCompany.id, data);
        setCompanies(prev => prev.map(c => c.id === updated.id ? updated : c));
    };
    const handleDelete = async (id: number) => {
        await companiesService.delete(id);
        setCompanies(prev => prev.filter(c => c.id !== id));
    };
    const handleUnassignContact = async (contactId: number) => {
        await contactsService.unassign(contactId);
        // recarrega contatos
    };
    
    // Filtra contatos vinculados a uma empresa
    const companyContacts = (clientId: number) =>
        allContacts.filter(c => c.clientId === clientId);
    
    return { companies, allContacts, createNewCompany, saveEdit, handleDelete, ... };
}
```

**Layout:**
- Cards de empresa: Avatar (quadrado), Nome, Telefone, Contagem de contatos, Badge de status (Active = verde, Inactive = cinza)
- Botão de editar/excluir em cada card
- Botão "Nova empresa" no cabeçalho
- Modal de criação/edição: Nome, Telefone, Status (select)
- Na edição: lista de contatos vinculados com botão "Remover" para cada

---

## Tela de Kanban

### `/kanban` → `screens/kanban/kanban.view.tsx`

**Hook `useKanban()`:**

```typescript
interface KanbanCard {
    id: number;
    title: string;
    subtitle: string;  // nome do responsável ou cliente
    type: 'task' | 'occurrence';
    status: string;
}

interface KanbanColumn {
    id: string;
    title: string;
    cards: KanbanCard[];
}

function useKanban() {
    const [tasks, setTasks] = useState<TaskResponse[]>([]);
    const [occurrences, setOccurrences] = useState<OccurrenceResponse[]>([]);
    
    // Mapeamento de status para colunas
    const buildColumns = (): KanbanColumn[] => {
        const todoCards: KanbanCard[] = [];
        const progressCards: KanbanCard[] = [];
        const doneCards: KanbanCard[] = [];
        
        // Tasks: Open → todo, InProgress → progress, Completed/Cancelled → done
        tasks.forEach(t => {
            const card = { id: t.id, title: t.title, subtitle: t.assignedToName ?? t.clientName, type: 'task', status: t.status };
            if (t.status === 'Open') todoCards.push(card);
            else if (t.status === 'InProgress') progressCards.push(card);
            else if (t.status === 'Completed' || t.status === 'Cancelled') doneCards.push(card);
        });
        
        // Occurrences: Open → todo, InProgress → progress, Resolved/Closed → done
        occurrences.forEach(o => {
            const card = { id: o.id, title: o.title, subtitle: o.assignedToName ?? o.chatName, type: 'occurrence', status: o.status };
            if (o.status === 'Open') todoCards.push(card);
            else if (o.status === 'InProgress') progressCards.push(card);
            else if (o.status === 'Resolved' || o.status === 'Closed') doneCards.push(card);
        });
        
        return [
            { id: 'todo', title: 'A fazer', cards: todoCards },
            { id: 'progress', title: 'Em andamento', cards: progressCards },
            { id: 'done', title: 'Concluído', cards: doneCards },
        ];
    };
    
    return { columns: buildColumns() };
}
```

**Layout:**
- Título "Kanban"
- 3 colunas lado a lado (scroll horizontal se necessário):
  - **A fazer**: Cards laranja/neutro
  - **Em andamento**: Cards azul/neutro  
  - **Concluído**: Cards verde/neutro
- Cada card: Título, Subtítulo (responsável/cliente), Badge de tipo ("Tarefa" ou "Ocorrência")
- Cards de ocorrência têm borda esquerda verde (3px solid var(--primary))
- Botão "Adicionar" placeholder (sem funcionalidade ainda)

---

## Componentes Compartilhados

### Avatar (`avatar/`)

Gera um avatar com a **primeira inicial** do nome sobre um **fundo gradiente**:

```typescript
type AvatarProps = {
    name: string;
    size?: number;      // default 44
    fontSize?: number;  // calculado automaticamente (size * 0.42)
    square?: boolean;   // false = circle, true = rounded square
};
```

**Gradientes:** 16 pares de cores pré-definidos, selecionados por hash do nome:
```typescript
const gradients = [
    ['#ff6b6b', '#ee5a24'],  // vermelho
    ['#48dbfb', '#0abde3'],  // azul claro
    ['#1dd1a1', '#10ac84'],  // verde
    ['#feca57', '#ff9f43'],  // laranja
    ['#5f27cd', '#341f97'],  // roxo
    ['#ff9ff3', '#f368e0'],  // rosa
    ['#54a0ff', '#2e86de'],  // azul
    ['#5f27cd', '#82589f'],  // magenta
    ['#ff6348', '#c44569'],  // salmão
    ['#7bed9f', '#2ed573'],  // verde claro
    ['#70a1ff', '#1e90ff'],  // azul royal
    ['#ffa502', '#e67e22'],  // laranja escuro
    ['#a29bfe', '#6c5ce7'],  // lavanda
    ['#fd79a8', '#e84393'],  // rosa escuro
    ['#00cec9', '#00b894'],  // turquesa
    ['#fab1a0', '#e17055'],  // pêssego
];
```

### ThemeToggle (`theme-toggle/`)

```typescript
function useThemeToggle() {
    const [theme, setTheme] = useState<'light' | 'dark'>('light');
    
    useEffect(() => {
        const saved = localStorage.getItem('theme');
        if (saved === 'dark' || (!saved && window.matchMedia('(prefers-color-scheme: dark)').matches)) {
            setTheme('dark');
            document.documentElement.setAttribute('data-theme', 'dark');
        }
    }, []);
    
    const toggle = () => {
        const newTheme = theme === 'dark' ? 'light' : 'dark';
        setTheme(newTheme);
        document.documentElement.setAttribute('data-theme', newTheme);
        localStorage.setItem('theme', newTheme);
    };
    
    return { theme, toggle };
}
```

---

## Fluxos de Dados Completos

### 1. Login Completo

```
Usuário digita nome/senha
    → login.logic.tsx: handleSubmit()
    → auth-store.ts: login(name, password)
    → auth.service.ts: POST /api/auth/login
    → API retorna { token, user }
    → Salva token + user no localStorage
    → Atualiza estado Zustand
    → layout.tsx detecta user ≠ null
    → Redireciona para /chats
```

### 2. Recebimento de Mensagem em Tempo Real

```
WhatsApp → Node.js → API .NET (webhook)
    → API salva mensagem no banco
    → API dispara SignalR 'MessageReceived'
    → websocket.ts recebe evento
    → chat-sidebar.logic.tsx: refresh da lista de chats
    → chat-area.logic.tsx: append da nova mensagem (se chat aberto)
    → UI atualiza em tempo real
```

### 3. Envio de Mensagem

```
Usuário digita texto e clica Enviar
    → chat-area.logic.tsx: sendMessage(text)
    → chats.service.ts: POST /api/messages/send { jid, text }
    → API envia para Node.js (whatsapp-web.js envia via WhatsApp)
    → API salva mensagem no banco
    → API dispara SignalR 'MessageSent'
    → chat-area recebe evento e adiciona à lista
    → Bolha azul aparece na UI
```

### 4. Gerenciamento de Contatos

```
Usuário abre modal "Salvar contato" no chat
    → Preenche nome + seleciona empresa
    → contacts.service.ts: POST /api/contacts
    → API associa contato ao chat (Chat.ContactId)
    → Botão "Salvar" desaparece do chat

OU

Usuário vai em Contatos
    → contacts.service.ts: GET /api/contacts
    → Lista exibe todos contatos
    → Edita: PUT /api/contacts/:id
    → Associa: PATCH /api/contacts/:id/assign
    → Exclui: DELETE /api/contacts/:id
```

### 5. Kanban

```
Usuário acessa Kanban
    → kanban.service.ts: GET /api/tasks + GET /api/occurrences
    → kanban.logic.tsx: buildColumns() organiza em 3 colunas
    → UI renderiza cards
```

---

## Configuração e Execução

### Pré-requisitos

- Node.js 18+
- A API MultiWhats rodando (http://localhost:5261)

### Variáveis de Ambiente (`.env`)

```env
NEXT_PUBLIC_API_URL=http://localhost:5261
```

### Scripts

```bash
npm run dev      # Inicia servidor de desenvolvimento
npm run build    # Build de produção
npm run start    # Inicia servidor de produção
npm run lint     # Executa ESLint
```

### Instalação

```bash
cd multiwhats-front
npm install
npm run dev
```

A aplicação estará disponível em `http://localhost:3000`.

---

## Padrões e Convenções de Código

### Separação View/Logic
- Todo componente funcional complexo tem **3 arquivos**: `.view.tsx`, `.logic.tsx`, `.module.css`
- Views são puras (sem estado, sem lógica)
- Logic exporta hooks personalizados

### Nomenclatura
- **Arquivos**: `kebab-case` (ex: `chat-area.view.tsx`, `auth-store.ts`)
- **Componentes**: `PascalCase` com sufixo (ex: `ChatAreaView`, `NavBarView`)
- **Hooks**: `camelCase` com prefixo `use` (ex: `useChatArea`, `useChatSidebar`)
- **Interfaces/Types**: `PascalCase` (ex: `ChatResponse`, `MessageResponse`)

### CSS Modules
- Estilos encapsulados por componente
- Nomes de classe em `camelCase` (ex: `.chatBubble`, `.sendButton`)
- Variáveis CSS globais para tema

### Estado
- **Estado global**: Zustand store (`auth-store.ts`)
- **Estado de componente**: hooks com `useState` + `useEffect` nos `.logic.tsx`
- **Cache**: Map manual para mensagens de chat (evita refetch desnecessário)

### WebSocket
- Cliente global singleton (`ws`)
- Método `on()` retorna função de unsubscribe
- Lazy connect (só conecta quando primeiro listener é registrado)

### Server vs Client Components
- Todos os componentes atuais são **Client Components** (`"use client"`)
- Nenhum Server Component é utilizado além do layout raiz
