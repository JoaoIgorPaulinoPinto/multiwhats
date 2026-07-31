import { api } from "./api"
import type { OccurrenceStatus, Priority } from "../types"
import type { OccurrenceResponse } from "../types/occurrence"
import type { ClientTaskStatus } from "../types"
import type { UserResponse } from "./auth.service"

export type { OccurrenceStatus, Priority, OccurrenceResponse, ClientTaskStatus, UserResponse }

export interface TaskResponse {
  id: number
  title: string
  description: string | null
  status: ClientTaskStatus
  priority: Priority
  dueDate: string | null
  clientId: number
  clientName: string | null
  assignedToUserId: number | null
  assignedToName: string | null
  createdByUserId: number | null
  createdByName: string | null
  createdAt: string
  lastUpdate: string
}

export interface AdvanceStatusResponse {
  message: string
  occurrence: OccurrenceResponse
}

export interface OccurrenceMetricsResponse {
  averageResolutionHours: number
  occurrencesPerDay: { date: string; count: number }[]
  perUser: { userId: number; userName: string | null; opened: number; closed: number }[]
  totalClosed: number
}

export const kanbanService = {
  listTasks() {
    return api.get<TaskResponse[]>("/api/tasks")
  },

  getTask(id: number) {
    return api.get<TaskResponse>(`/api/tasks/${id}`)
  },

  createTask(data: { title: string; description?: string; priority: string; clientId: number }) {
    return api.post<TaskResponse>("/api/tasks", data)
  },

  updateTask(id: number, data: { title?: string; description?: string; priority?: string; dueDate?: string }) {
    return api.put<TaskResponse>(`/api/tasks/${id}`, data)
  },

  updateTaskStatus(id: number, status: string) {
    return api.patch<TaskResponse>(`/api/tasks/${id}/status`, { status })
  },

  deleteTask(id: number) {
    return api.delete(`/api/tasks/${id}`)
  },

  listOccurrences() {
    return api.get<OccurrenceResponse[]>("/api/occurrences")
  },

  getOccurrence(id: number) {
    return api.get<OccurrenceResponse>(`/api/occurrences/${id}`)
  },

  createOccurrence(data: { title: string; description?: string; priority: number; chatId: number; assignedToUserId?: number }) {
    return api.post<OccurrenceResponse>("/api/occurrences", data)
  },

  updateOccurrence(id: number, data: { title?: string; description?: string; status?: number; priority?: number }) {
    return api.put<OccurrenceResponse>(`/api/occurrences/${id}`, data)
  },

  deleteOccurrence(id: number) {
    return api.delete(`/api/occurrences/${id}`)
  },

  advanceStatus(id: number, direction: 0 | 1) {
    return api.patch<AdvanceStatusResponse>(`/api/occurrences/${id}/status`, { direction })
  },

  getOccurrenceMetrics() {
    return api.get<OccurrenceMetricsResponse>("/api/occurrences/metrics")
  },

  listUsers() {
    return api.get<UserResponse[]>("/api/users")
  },
}
