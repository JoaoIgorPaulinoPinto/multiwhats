import { isJwtExpired } from "../utils/jwt"

export class ApiError extends Error {
  status: number

  constructor(message: string, status: number) {
    super(message)
    this.name = "ApiError"
    this.status = status
  }
}

interface AuthRefreshResponse {
  token: string
  refreshToken: string
}

let refreshingPromise: Promise<AuthRefreshResponse | null> | null = null

function clearSessionAndRedirect() {
  localStorage.removeItem("token")
  localStorage.removeItem("refreshToken")
  localStorage.removeItem("user")
  if (typeof window !== "undefined" && window.location.pathname !== "/login") {
    window.location.href = "/login"
  }
}

async function refreshTokens(baseUrl: string): Promise<AuthRefreshResponse | null> {
  if (typeof window === "undefined") return null
  const refreshToken = localStorage.getItem("refreshToken")
  if (!refreshToken) return null

  if (!refreshingPromise) {
    refreshingPromise = fetch(`${baseUrl}/api/auth/refresh`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ refreshToken }),
    })
      .then(async (res) => {
        if (!res.ok) return null
        const data: AuthRefreshResponse = await res.json()
        localStorage.setItem("token", data.token)
        localStorage.setItem("refreshToken", data.refreshToken)
        return data
      })
      .catch(() => null)
      .finally(() => {
        refreshingPromise = null
      })
  }

  return refreshingPromise
}

// Renova o token se estiver expirado. Usado pelo WebSocket antes de (re)conectar.
export async function ensureFreshAccessToken(): Promise<string | null> {
  if (typeof window === "undefined") return null
  const token = localStorage.getItem("token")
  if (!token) return null
  if (!isJwtExpired(token)) return token
  const fresh = await refreshTokens(resolveApiUrl())
  return fresh?.token ?? null
}

class ApiClient {
  private baseUrl: string

  constructor(baseUrl: string) {
    this.baseUrl = baseUrl
  }

  private getToken(): string | null {
    if (typeof window === "undefined") return null
    return localStorage.getItem("token")
  }

  private async doFetch(method: string, path: string, headers: Record<string, string>, body?: unknown): Promise<Response> {
    try {
      return await fetch(`${this.baseUrl}${path}`, {
        method,
        headers,
        body: body ? JSON.stringify(body) : undefined,
      })
    } catch {
      throw new ApiError("Não foi possível conectar ao servidor. Tente novamente.", 0)
    }
  }

  private async request<T>(method: string, path: string, body?: unknown): Promise<T> {
    const token = this.getToken()
    const headers: Record<string, string> = {
      "Content-Type": "application/json",
    }
    if (token) {
      headers["Authorization"] = `Bearer ${token}`
    }

    let res = await this.doFetch(method, path, headers, body)

    // Token expirado/revogado: tenta renovar uma única vez e repetir a chamada.
    if (res.status === 401 && token) {
      const fresh = await refreshTokens(this.baseUrl)
      if (fresh) {
        headers["Authorization"] = `Bearer ${fresh.token}`
        res = await this.doFetch(method, path, headers, body)
      }
    }

    if (!res.ok) {
      const error = await res.json().catch(() => ({ message: res.statusText }))
      const message = error.message ?? `HTTP ${res.status}`
      if (res.status === 401 && token) {
        clearSessionAndRedirect()
      }
      throw new ApiError(message, res.status)
    }

    const text = await res.text()
    return text ? JSON.parse(text) : ({} as T)
  }

  get<T>(path: string) {
    return this.request<T>("GET", path)
  }

  post<T>(path: string, body?: unknown) {
    return this.request<T>("POST", path, body)
  }

  put<T>(path: string, body?: unknown) {
    return this.request<T>("PUT", path, body)
  }

  patch<T>(path: string, body?: unknown) {
    return this.request<T>("PATCH", path, body)
  }

  delete<T>(path: string) {
    return this.request<T>("DELETE", path)
  }
}

function resolveApiUrl(): string {
  if (process.env.NEXT_PUBLIC_API_URL) return process.env.NEXT_PUBLIC_API_URL
  if (typeof window !== "undefined") {
    return `${window.location.protocol}//${window.location.hostname}:5261`
  }
  return "http://localhost:5261"
}

export const api = new ApiClient(resolveApiUrl())
