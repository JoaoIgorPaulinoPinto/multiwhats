import { api } from "./api"

export interface UserResponse {
  id: number
  name: string
  role: string
  isActive: boolean
  createdAt: string
}

export type UserRole = "Support" | "Dev" | "Admin"

export interface UpdateUserRequest {
  name?: string
  newPassword?: string
  role?: UserRole
  isActive?: boolean
}

export const usersService = {
  list() {
    return api.get<UserResponse[]>("/api/users")
  },

  update(id: number, data: UpdateUserRequest) {
    return api.put<{ message: string; user: UserResponse }>(`/api/users/${id}`, data)
  },
}

export const ROLE_LABELS: Record<string, string> = {
  Support: "Suporte",
  Dev: "Desenvolvedor",
  Admin: "Administrador",
}
