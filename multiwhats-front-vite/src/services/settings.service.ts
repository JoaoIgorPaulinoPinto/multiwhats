import { api } from "./api"

export interface SystemParameterResponse {
  id: number
  key: string
  value: string | null
  type: string
  group: string | null
  description: string | null
  isRequired: boolean
  createdAt: string
  updatedAt: string
  updatedByUserId: number | null
}

export const settingsService = {
  list() {
    return api.get<SystemParameterResponse[]>("/api/admin/config")
  },

  getByKey(key: string) {
    return api.get<SystemParameterResponse>(`/api/admin/config/${encodeURIComponent(key)}`)
  },

  update(key: string, value: string) {
    return api.put<{ message: string }>(`/api/admin/config/${encodeURIComponent(key)}`, { value })
  },

  reload() {
    return api.post<{ message: string }>("/api/admin/config/reload")
  },
}
