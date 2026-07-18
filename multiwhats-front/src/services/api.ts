function delay(ms = 300): Promise<void> {
  return new Promise((r) => setTimeout(r, ms))
}

export interface LoginResponse {
  token: string
  user: {
    id: number
    name: string
    role: string
  }
}

export interface Client {
  id: number
  name: string
  mainPhoneNumber: string | null
  status: string
  contacts?: Contact[]
  createdByUserId?: number
  createdAt?: string
  lastUpdate?: string
  isDeleted?: boolean
}

export interface Contact {
  id: number
  jid: string
  phoneNumber: string
  name: string | null
  pushName: string | null
  profilePicUrl: string | null
  isBlocked: boolean
  isGroup: boolean
  lastMessageAt: string | null
  clientId: number | null
  groupId: number | null
  createdByUserId?: number
  createdAt: string
  lastUpdate: string
}

export interface Occurrence {
  id: number
  title: string
  description: string | null
  status: string
  priority: string
  contactId: number
  assignedToUserId: number | null
  contact?: Contact
  createdAt: string
  lastUpdate: string
}

export interface ClientTask {
  id: number
  title: string
  description: string | null
  status: string
  priority: string
  dueDate: string | null
  clientId: number
  assignedToUserId: number | null
  client?: Client
  createdAt: string
  lastUpdate: string
}

export interface Message {
  id: number
  messageId: string | null
  fromJid: string
  toJid: string | null
  phoneNumber: string
  body: string | null
  direction: "Incoming" | "Outgoing"
  type: string
  timestamp: number
  sentAt: string
  notifyName: string | null
  hasMedia: boolean
  contactId: number | null
  createdAt: string
}

// ------------------------------------------------------------------ Mock data
let nextContactId = 100
let nextClientId = 10
let nextTaskId = 50
let nextOccId = 30
let nextMsgId = 500
let nextUserId = 2

let currentUser: LoginResponse["user"] | null = null

const mockClients: Client[] = [
  { id: 1, name: "Acme Corp", mainPhoneNumber: "(11) 3000-0001", status: "Active", createdAt: "2025-01-10T10:00:00Z", lastUpdate: "2025-06-15T12:00:00Z" },
  { id: 2, name: "Beta Ltda", mainPhoneNumber: "(11) 3000-0002", status: "Active", createdAt: "2025-02-20T10:00:00Z", lastUpdate: "2025-06-10T12:00:00Z" },
  { id: 3, name: "Gamma S/A", mainPhoneNumber: "(11) 3000-0003", status: "Inactive", createdAt: "2025-03-05T10:00:00Z", lastUpdate: "2025-05-01T12:00:00Z" },
  { id: 4, name: "Delta Tecnologia", mainPhoneNumber: "(11) 3000-0004", status: "Active", createdAt: "2025-03-15T10:00:00Z", lastUpdate: "2025-06-18T12:00:00Z" },
  { id: 5, name: "Eireli Comércio", mainPhoneNumber: "(11) 3000-0005", status: "Active", createdAt: "2025-04-01T10:00:00Z", lastUpdate: "2025-06-20T12:00:00Z" },
  { id: 6, name: "Ómega Serviços", mainPhoneNumber: null, status: "Active", createdAt: "2025-04-10T10:00:00Z", lastUpdate: "2025-06-01T12:00:00Z" },
]

const mockContacts: Contact[] = [
  { id: 1, jid: "5511999990001@s.whatsapp.net", phoneNumber: "(11) 99999-0001", name: "João Silva", pushName: "João", profilePicUrl: null, isBlocked: false, isGroup: false, lastMessageAt: "2025-06-20T14:30:00Z", clientId: 1, groupId: null, createdAt: "2025-01-15T10:00:00Z", lastUpdate: "2025-06-20T14:30:00Z" },
  { id: 2, jid: "5511999990002@s.whatsapp.net", phoneNumber: "(11) 99999-0002", name: "Maria Santos", pushName: "Maria", profilePicUrl: null, isBlocked: false, isGroup: false, lastMessageAt: "2025-06-19T09:15:00Z", clientId: 1, groupId: null, createdAt: "2025-01-20T10:00:00Z", lastUpdate: "2025-06-19T09:15:00Z" },
  { id: 3, jid: "5511999990003@s.whatsapp.net", phoneNumber: "(11) 99999-0003", name: "Carlos Oliveira", pushName: "Carlos", profilePicUrl: null, isBlocked: false, isGroup: false, lastMessageAt: "2025-06-18T16:45:00Z", clientId: 2, groupId: null, createdAt: "2025-02-01T10:00:00Z", lastUpdate: "2025-06-18T16:45:00Z" },
  { id: 4, jid: "5511999990004@s.whatsapp.net", phoneNumber: "(11) 99999-0004", name: "Ana Costa", pushName: "Ana", profilePicUrl: null, isBlocked: false, isGroup: false, lastMessageAt: "2025-06-17T11:20:00Z", clientId: null, groupId: null, createdAt: "2025-02-10T10:00:00Z", lastUpdate: "2025-06-17T11:20:00Z" },
  { id: 5, jid: "5511999990005@s.whatsapp.net", phoneNumber: "(11) 99999-0005", name: "Pedro Alves", pushName: "Pedro", profilePicUrl: null, isBlocked: false, isGroup: false, lastMessageAt: "2025-06-16T08:00:00Z", clientId: 2, groupId: null, createdAt: "2025-02-15T10:00:00Z", lastUpdate: "2025-06-16T08:00:00Z" },
  { id: 6, jid: "5511999990006@s.whatsapp.net", phoneNumber: "(11) 99999-0006", name: "Lucia Mendes", pushName: "Lucia", profilePicUrl: null, isBlocked: false, isGroup: false, lastMessageAt: "2025-06-15T14:00:00Z", clientId: 3, groupId: null, createdAt: "2025-03-01T10:00:00Z", lastUpdate: "2025-06-15T14:00:00Z" },
  { id: 7, jid: "5511999990007@s.whatsapp.net", phoneNumber: "(11) 99999-0007", name: "Roberto Lima", pushName: "Roberto", profilePicUrl: null, isBlocked: false, isGroup: false, lastMessageAt: "2025-06-14T10:30:00Z", clientId: null, groupId: null, createdAt: "2025-03-10T10:00:00Z", lastUpdate: "2025-06-14T10:30:00Z" },
  { id: 8, jid: "5511999990008@s.whatsapp.net", phoneNumber: "(11) 99999-0008", name: "Fernanda Rocha", pushName: "Fernanda", profilePicUrl: null, isBlocked: false, isGroup: false, lastMessageAt: "2025-06-13T17:00:00Z", clientId: 4, groupId: null, createdAt: "2025-03-20T10:00:00Z", lastUpdate: "2025-06-13T17:00:00Z" },
  { id: 9, jid: "5511999990009@s.whatsapp.net", phoneNumber: "(11) 99999-0009", name: "Gustavo Souza", pushName: "Gustavo", profilePicUrl: null, isBlocked: false, isGroup: false, lastMessageAt: "2025-06-12T12:00:00Z", clientId: 4, groupId: null, createdAt: "2025-04-01T10:00:00Z", lastUpdate: "2025-06-12T12:00:00Z" },
  { id: 10, jid: "5511999990010@s.whatsapp.net", phoneNumber: "(11) 99999-0010", name: "Helena Martins", pushName: "Helena", profilePicUrl: null, isBlocked: false, isGroup: false, lastMessageAt: null, clientId: null, groupId: null, createdAt: "2025-04-10T10:00:00Z", lastUpdate: "2025-04-10T10:00:00Z" },
  { id: 11, jid: "5511999990011@s.whatsapp.net", phoneNumber: "(11) 99999-0011", name: "Igor Barbosa", pushName: "Igor", profilePicUrl: null, isBlocked: false, isGroup: false, lastMessageAt: null, clientId: 5, groupId: null, createdAt: "2025-04-15T10:00:00Z", lastUpdate: "2025-04-15T10:00:00Z" },
  { id: 12, jid: "5511999990012@s.whatsapp.net", phoneNumber: "(11) 99999-0012", name: "Julia Campos", pushName: "Julia", profilePicUrl: null, isBlocked: false, isGroup: false, lastMessageAt: "2025-06-11T15:00:00Z", clientId: 5, groupId: null, createdAt: "2025-05-01T10:00:00Z", lastUpdate: "2025-06-11T15:00:00Z" },
  { id: 13, jid: "5511999990013@s.whatsapp.net", phoneNumber: "(11) 99999-0013", name: "Kevin Nunes", pushName: "Kevin", profilePicUrl: null, isBlocked: false, isGroup: false, lastMessageAt: "2025-06-10T09:00:00Z", clientId: null, groupId: null, createdAt: "2025-05-10T10:00:00Z", lastUpdate: "2025-06-10T09:00:00Z" },
]

const mockTasks: ClientTask[] = [
  { id: 1, title: "Implementar login SSO", description: "Integrar com provedor OAuth2", status: "Completed", priority: "High", dueDate: "2025-05-01", clientId: 1, assignedToUserId: 1, client: mockClients[0], createdAt: "2025-04-01T10:00:00Z", lastUpdate: "2025-04-28T10:00:00Z" },
  { id: 2, title: "Migrar servidor de banco", description: "Migrar do PostgreSQL para MySQL", status: "InProgress", priority: "High", dueDate: "2025-07-15", clientId: 1, assignedToUserId: 1, client: mockClients[0], createdAt: "2025-05-01T10:00:00Z", lastUpdate: "2025-06-10T10:00:00Z" },
  { id: 3, title: "Relatório mensal de vendas", description: "Criar dashboard com gráficos", status: "Open", priority: "Medium", dueDate: "2025-07-01", clientId: 2, assignedToUserId: 1, client: mockClients[1], createdAt: "2025-06-01T10:00:00Z", lastUpdate: "2025-06-01T10:00:00Z" },
  { id: 4, title: "Corrigir bug no checkout", description: "Erro 500 ao finalizar pedido", status: "Open", priority: "Urgent", dueDate: "2025-06-25", clientId: 2, assignedToUserId: 1, client: mockClients[1], createdAt: "2025-06-18T10:00:00Z", lastUpdate: "2025-06-18T10:00:00Z" },
  { id: 5, title: "Atualizar política de privacidade", description: "Adequar à LGPD", status: "InProgress", priority: "Low", dueDate: "2025-08-01", clientId: 3, assignedToUserId: 1, client: mockClients[2], createdAt: "2025-05-15T10:00:00Z", lastUpdate: "2025-06-05T10:00:00Z" },
  { id: 6, title: "Configurar CI/CD", description: "Pipeline no GitHub Actions", status: "Completed", priority: "Medium", dueDate: "2025-04-30", clientId: 4, assignedToUserId: 1, client: mockClients[3], createdAt: "2025-04-01T10:00:00Z", lastUpdate: "2025-04-29T10:00:00Z" },
  { id: 7, title: "Treinamento da equipe", description: "Workshop de React e TypeScript", status: "Open", priority: "Medium", dueDate: "2025-07-20", clientId: 5, assignedToUserId: 1, client: mockClients[4], createdAt: "2025-06-10T10:00:00Z", lastUpdate: "2025-06-10T10:00:00Z" },
  { id: 8, title: "Auditoria de segurança", description: "Pentest e análise de vulnerabilidades", status: "Cancelled", priority: "High", dueDate: "2025-05-30", clientId: 6, assignedToUserId: 1, client: mockClients[5], createdAt: "2025-05-01T10:00:00Z", lastUpdate: "2025-05-15T10:00:00Z" },
]

const mockOccurrences: Occurrence[] = [
  { id: 1, title: "Cliente reclamou do atraso", description: "Pedido #1234 com 5 dias de atraso", status: "Resolved", priority: "High", contactId: 1, assignedToUserId: 1, contact: mockContacts[0], createdAt: "2025-06-01T10:00:00Z", lastUpdate: "2025-06-05T10:00:00Z" },
  { id: 2, title: "Problema no envio de NF", description: "Nota fiscal com XML incorreto", status: "Open", priority: "Urgent", contactId: 3, assignedToUserId: 1, contact: mockContacts[2], createdAt: "2025-06-18T10:00:00Z", lastUpdate: "2025-06-18T10:00:00Z" },
  { id: 3, title: "Solicitação de cancelamento", description: "Cliente quer cancelar assinatura", status: "InProgress", priority: "Medium", contactId: 5, assignedToUserId: 1, contact: mockContacts[4], createdAt: "2025-06-15T10:00:00Z", lastUpdate: "2025-06-16T10:00:00Z" },
  { id: 4, title: "Troca de produto danificado", description: "Produto chegou com defeito", status: "Open", priority: "Medium", contactId: 8, assignedToUserId: 1, contact: mockContacts[7], createdAt: "2025-06-17T10:00:00Z", lastUpdate: "2025-06-17T10:00:00Z" },
  { id: 5, title: "Dúvida sobre fatura", description: "Cliente não reconhece cobrança", status: "Closed", priority: "Low", contactId: 2, assignedToUserId: 1, contact: mockContacts[1], createdAt: "2025-06-10T10:00:00Z", lastUpdate: "2025-06-12T10:00:00Z" },
]

const mockMessagesByContact: Record<number, Message[]> = {
  1: [
    { id: 1, phoneNumber: "(11) 99999-0001", body: "Olá, gostaria de saber sobre o status do meu pedido", direction: "Incoming", timestamp: 1718000000, sentAt: "2025-06-20T14:30:00Z", contactId: 1, messageId: null, fromJid: "", toJid: "", type: "text", notifyName: null, hasMedia: false, createdAt: "2025-06-20T14:30:00Z" },
    { id: 2, phoneNumber: "(11) 99999-0001", body: "Olá João, seu pedido #1234 está a caminho e deve chegar em até 3 dias úteis.", direction: "Outgoing", timestamp: 1718000100, sentAt: "2025-06-20T14:31:00Z", contactId: 1, messageId: null, fromJid: "", toJid: "", type: "text", notifyName: null, hasMedia: false, createdAt: "2025-06-20T14:31:00Z" },
    { id: 3, phoneNumber: "(11) 99999-0001", body: "Obrigado! Aguardarei.", direction: "Incoming", timestamp: 1718000200, sentAt: "2025-06-20T14:32:00Z", contactId: 1, messageId: null, fromJid: "", toJid: "", type: "text", notifyName: null, hasMedia: false, createdAt: "2025-06-20T14:32:00Z" },
  ],
  2: [
    { id: 10, phoneNumber: "(11) 99999-0002", body: "Bom dia! Quando renova minha assinatura?", direction: "Incoming", timestamp: 1717500000, sentAt: "2025-06-19T09:15:00Z", contactId: 2, messageId: null, fromJid: "", toJid: "", type: "text", notifyName: null, hasMedia: false, createdAt: "2025-06-19T09:15:00Z" },
    { id: 11, phoneNumber: "(11) 99999-0002", body: "Bom dia Maria! Sua assinatura vence em 30/06. Vou te enviar o boleto.", direction: "Outgoing", timestamp: 1717500100, sentAt: "2025-06-19T09:16:00Z", contactId: 2, messageId: null, fromJid: "", toJid: "", type: "text", notifyName: null, hasMedia: false, createdAt: "2025-06-19T09:16:00Z" },
  ],
  3: [
    { id: 20, phoneNumber: "(11) 99999-0003", body: "Preciso de suporte com o sistema", direction: "Incoming", timestamp: 1717000000, sentAt: "2025-06-18T16:45:00Z", contactId: 3, messageId: null, fromJid: "", toJid: "", type: "text", notifyName: null, hasMedia: false, createdAt: "2025-06-18T16:45:00Z" },
    { id: 21, phoneNumber: "(11) 99999-0003", body: "Claro Carlos, me fala qual o problema que já te ajudo.", direction: "Outgoing", timestamp: 1717000100, sentAt: "2025-06-18T16:46:00Z", contactId: 3, messageId: null, fromJid: "", toJid: "", type: "text", notifyName: null, hasMedia: false, createdAt: "2025-06-18T16:46:00Z" },
    { id: 22, phoneNumber: "(11) 99999-0003", body: "Não consigo acessar o módulo de relatórios", direction: "Incoming", timestamp: 1717000200, sentAt: "2025-06-18T16:47:00Z", contactId: 3, messageId: null, fromJid: "", toJid: "", type: "text", notifyName: null, hasMedia: false, createdAt: "2025-06-18T16:47:00Z" },
  ],
  5: [
    { id: 30, phoneNumber: "(11) 99999-0005", body: "Vou precisar remarcar a reunião", direction: "Incoming", timestamp: 1716300000, sentAt: "2025-06-16T08:00:00Z", contactId: 5, messageId: null, fromJid: "", toJid: "", type: "text", notifyName: null, hasMedia: false, createdAt: "2025-06-16T08:00:00Z" },
    { id: 31, phoneNumber: "(11) 99999-0005", body: "Sem problemas Pedro, qual a melhor data para você?", direction: "Outgoing", timestamp: 1716300100, sentAt: "2025-06-16T08:01:00Z", contactId: 5, messageId: null, fromJid: "", toJid: "", type: "text", notifyName: null, hasMedia: false, createdAt: "2025-06-16T08:01:00Z" },
  ],
  8: [
    { id: 40, phoneNumber: "(11) 99999-0008", body: "O produto chegou com defeito, quero trocar", direction: "Incoming", timestamp: 1715600000, sentAt: "2025-06-13T17:00:00Z", contactId: 8, messageId: null, fromJid: "", toJid: "", type: "text", notifyName: null, hasMedia: false, createdAt: "2025-06-13T17:00:00Z" },
    { id: 41, phoneNumber: "(11) 99999-0008", body: "Sinto muito Fernanda! Vou abrir uma ocorrência de troca para você.", direction: "Outgoing", timestamp: 1715600100, sentAt: "2025-06-13T17:01:00Z", contactId: 8, messageId: null, fromJid: "", toJid: "", type: "text", notifyName: null, hasMedia: false, createdAt: "2025-06-13T17:01:00Z" },
  ],
}

// ----------------------------------------------------------------- Mock API
function matchPath(pattern: string, path: string): Record<string, string> | null {
  const pParts = pattern.split("/")
  const rParts = path.split("/")
  if (pParts.length !== rParts.length) return null
  const params: Record<string, string> = {}
  for (let i = 0; i < pParts.length; i++) {
    if (pParts[i].startsWith(":")) {
      params[pParts[i].slice(1)] = rParts[i]
    } else if (pParts[i] !== rParts[i]) {
      return null
    }
  }
  return params
}

export const api = {
  async get<T>(path: string): Promise<T> {
    await delay()

    if (path === "/api/contacts") return [...mockContacts] as T
    if (path === "/api/clients") return [...mockClients] as T
    if (path === "/api/tasks") {
      return mockTasks.map((t) => ({ ...t, client: mockClients.find((c) => c.id === t.clientId) ?? null })) as T
    }
    if (path === "/api/occurrences") {
      return mockOccurrences.map((o) => ({ ...o, contact: mockContacts.find((c) => c.id === o.contactId) ?? null })) as T
    }

    const contactMatch = matchPath("/api/contacts/:id", path)
    if (contactMatch) {
      const c = mockContacts.find((c) => c.id === Number(contactMatch.id))
      if (!c) throw new Error("Contato não encontrado")
      return { ...c } as T
    }

    const msgMatch = matchPath("/api/messages/contact/:contactId", path)
    if (msgMatch) {
      const msgs = mockMessagesByContact[Number(msgMatch.contactId)] ?? []
      return { messages: msgs } as T
    }

    if (path === "/api/contacts") return [] as T

    throw new Error(`GET ${path} não implementado no mock`)
  },

  async post<T>(path: string, body?: unknown): Promise<T> {
    await delay()

    if (path === "/api/auth/login") {
      const { name, password } = body as { name: string; password: string }
      if (!name || !password) throw new Error("Nome e senha obrigatórios")
      const user = { id: 1, name, role: "Admin" }
      currentUser = user
      return { token: "mock-token-123", user } as T
    }

    if (path === "/api/auth/register") {
      return {} as T
    }

    if (path === "/api/auth/logout") {
      currentUser = null
      return {} as T
    }

    if (path === "/api/clients") {
      const { name, mainPhoneNumber } = body as { name: string; mainPhoneNumber: string | null }
      const now = new Date().toISOString()
      const client: Client = {
        id: nextClientId++,
        name,
        mainPhoneNumber: mainPhoneNumber ?? null,
        status: "Active",
        createdAt: now,
        lastUpdate: now,
      }
      mockClients.push(client)
      return client as T
    }

    if (path === "/api/messages/send") {
      const b = body as { phoneNumber: string; text: string }
      const now = new Date().toISOString()
      const msg: Message = {
        id: nextMsgId++,
        body: b.text,
        direction: "Outgoing",
        phoneNumber: b.phoneNumber,
        timestamp: Date.now() / 1000,
        sentAt: now,
        createdAt: now,
        contactId: null,
        messageId: null, fromJid: "", toJid: "", type: "text", notifyName: null, hasMedia: false,
      }
      return msg as T
    }

    throw new Error(`POST ${path} não implementado no mock`)
  },

  async put<T>(path: string, body?: unknown): Promise<T> {
    await delay()

    const contactMatch = matchPath("/api/contacts/:id", path)
    if (contactMatch) {
      const id = Number(contactMatch.id)
      const idx = mockContacts.findIndex((c) => c.id === id)
      if (idx === -1) throw new Error("Contato não encontrado")
      const b = body as { name: string; pushName: string }
      mockContacts[idx] = { ...mockContacts[idx], name: b.name, pushName: b.pushName, lastUpdate: new Date().toISOString() }
      return { ...mockContacts[idx] } as T
    }

    const clientMatch = matchPath("/api/clients/:id", path)
    if (clientMatch) {
      const id = Number(clientMatch.id)
      const idx = mockClients.findIndex((c) => c.id === id)
      if (idx === -1) throw new Error("Empresa não encontrada")
      const b = body as { name: string; mainPhoneNumber: string | null; status: string }
      mockClients[idx] = { ...mockClients[idx], name: b.name, mainPhoneNumber: b.mainPhoneNumber, status: b.status, lastUpdate: new Date().toISOString() }
      return { ...mockClients[idx] } as T
    }

    throw new Error(`PUT ${path} não implementado no mock`)
  },

  async patch<T>(path: string, body?: unknown): Promise<T> {
    await delay()

    const assignMatch = matchPath("/api/contacts/:id/assign", path)
    if (assignMatch) {
      const id = Number(assignMatch.id)
      const idx = mockContacts.findIndex((c) => c.id === id)
      if (idx === -1) throw new Error("Contato não encontrado")
      const b = body as { clientId: number }
      mockContacts[idx] = { ...mockContacts[idx], clientId: b.clientId, lastUpdate: new Date().toISOString() }
      return {} as T
    }

    const unassignMatch = matchPath("/api/contacts/:id/unassign", path)
    if (unassignMatch) {
      const id = Number(unassignMatch.id)
      const idx = mockContacts.findIndex((c) => c.id === id)
      if (idx === -1) throw new Error("Contato não encontrado")
      mockContacts[idx] = { ...mockContacts[idx], clientId: null, lastUpdate: new Date().toISOString() }
      return {} as T
    }

    throw new Error(`PATCH ${path} não implementado no mock`)
  },

  async delete<T>(path: string): Promise<T> {
    await delay()

    const contactMatch = matchPath("/api/contacts/:id", path)
    if (contactMatch) {
      const id = Number(contactMatch.id)
      const idx = mockContacts.findIndex((c) => c.id === id)
      if (idx === -1) throw new Error("Contato não encontrado")
      mockContacts.splice(idx, 1)
      return {} as T
    }

    const clientMatch = matchPath("/api/clients/:id", path)
    if (clientMatch) {
      const id = Number(clientMatch.id)
      const idx = mockClients.findIndex((c) => c.id === id)
      if (idx === -1) throw new Error("Empresa não encontrada")
      mockClients.splice(idx, 1)
      return {} as T
    }

    throw new Error(`DELETE ${path} não implementado no mock`)
  },
}
