export class ApiError extends Error {
  status: number

  constructor(message: string, status: number) {
    super(message)
    this.name = "ApiError"
    this.status = status
  }
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

  private async request<T>(method: string, path: string, body?: unknown): Promise<T> {
    const token = this.getToken()
    const headers: Record<string, string> = {
      "Content-Type": "application/json",
    }
    if (token) {
      headers["Authorization"] = `Bearer ${token}`
    }

    const res = await fetch(`${this.baseUrl}${path}`, {
      method,
      headers,
      body: body ? JSON.stringify(body) : undefined,
    })

    if (!res.ok) {
      if (res.status === 401) {
        localStorage.removeItem("token")
        localStorage.removeItem("user")
        window.location.href = "/login"
      }
      const error = await res.json().catch(() => ({ message: res.statusText }))
      throw new ApiError(error.message ?? `HTTP ${res.status}`, res.status)
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

export const api = new ApiClient(process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5261")
