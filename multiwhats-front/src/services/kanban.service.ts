import { api } from "./api"

export type Priority = "Low" | "Medium" | "High" | "Urgent"
export type ClientTaskStatus = "Open" | "InProgress" | "Completed" | "Cancelled"
export type OccurrenceStatus = "Open" | "InProgress" | "Resolved" | "Closed"

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

export interface OccurrenceResponse {
  id: number
  title: string
  description: string | null
  status: OccurrenceStatus
  priority: Priority
  chatId: number
  chatName: string | null
  assignedToUserId: number | null
  assignedToName: string | null
  createdByUserId: number | null
  createdByName: string | null
  messageCount: number
  createdAt: string
  lastUpdate: string
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

  createOccurrence(data: { title: string; description?: string; priority: string; chatId: number }) {
    return api.post<OccurrenceResponse>("/api/occurrences", data)
  },

  updateOccurrence(id: number, data: { title?: string; description?: string; status?: string; priority?: string }) {
    return api.put<OccurrenceResponse>(`/api/occurrences/${id}`, data)
  },

  deleteOccurrence(id: number) {
    return api.delete(`/api/occurrences/${id}`)
  },
}
