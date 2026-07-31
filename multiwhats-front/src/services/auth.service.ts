import { api } from "./api"


export interface UserResponse {
  id: number
  name: string
  role: string
  isActive: boolean
  createdAt: string
}

export interface LoginResponse {
  token: string
  user: UserResponse
}

export interface RegistrationCodeResponse {
  id: number
  code: string
  isUsed: boolean
  usedByUserId: number | null
  expiresAt: string
  createdAt: string
}

export const authService = {
  login(name: string, password: string) {
    return api.post<LoginResponse>("/api/auth/login", { name, password })
  },

  register(name: string, password: string, registrationCode?: string) {
    return api.post("/api/auth/register", { name, password, registrationCode })
  },

  generateCodes(quantity = 1) {
    return api.post<RegistrationCodeResponse[]>("/api/auth/codes", { quantity })
  },

  logout() {
    return api.post("/api/auth/logout")
  },

  me() {
    return api.get<UserResponse>("/api/auth/me")
  },
}
