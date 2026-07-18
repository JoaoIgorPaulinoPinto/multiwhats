const BASE_URL = "http://localhost:51563"

function getToken(): string | null {
  if (typeof window === "undefined") return null
  return localStorage.getItem("token")
}

async function request<T>(
  method: string,
  path: string,
  body?: unknown
): Promise<T> {
  const token = getToken()
  const headers: Record<string, string> = {
    "Content-Type": "application/json",
  }
  if (token) headers["Authorization"] = `Bearer ${token}`

  const logBody = body ? ` ${JSON.stringify(body).slice(0, 200)}` : ""
  console.log(`[API] → ${method} ${path}${logBody}`)

  const res = await fetch(`${BASE_URL}${path}`, {
    method,
    headers,
    body: body ? JSON.stringify(body) : undefined,
  })

  if (res.status === 401) {
    console.warn(`[API] 401 ${method} ${path} — limpando token`)
    localStorage.removeItem("token")
    localStorage.removeItem("user")
    window.location.reload()
    throw new Error("Não autorizado")
  }

  const data = await res.json()
  if (!res.ok) {
    console.error(`[API] ✘ ${res.status} ${method} ${path} — ${data.message}`)
    throw new Error(data.message || "Erro na requisição")
  }

  console.log(`[API] ← ${method} ${path} → ${res.status}`)
  return data as T
}

export const api = {
  get: <T>(path: string) => request<T>("GET", path),
  post: <T>(path: string, body?: unknown) => request<T>("POST", path, body),
  put: <T>(path: string, body?: unknown) => request<T>("PUT", path, body),
  patch: <T>(path: string, body?: unknown) => request<T>("PATCH", path, body),
  delete: <T>(path: string) => request<T>("DELETE", path),
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
