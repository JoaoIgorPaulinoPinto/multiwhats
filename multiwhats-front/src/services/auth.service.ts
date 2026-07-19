import { api } from "./api"

export type UserRole = "Support" | "Dev" | "Admin"

export interface UserResponse {
  id: number
  name: string
  role: UserRole
  isActive: boolean
  createdAt: string
}

export interface LoginResponse {
  token: string
  user: UserResponse
}

export const authService = {
  login(name: string, password: string) {
    return api.post<LoginResponse>("/api/auth/login", { name, password })
  },

  register(name: string, password: string) {
    return api.post("/api/auth/register", { name, password })
  },

  logout() {
    return api.post("/api/auth/logout")
  },

  me() {
    return api.get<UserResponse>("/api/auth/me")
  },
}
