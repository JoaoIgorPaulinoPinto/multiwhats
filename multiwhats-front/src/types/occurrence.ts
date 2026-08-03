import type { OccurrenceStatus, Priority } from "./index"

export interface OccurrenceSummary {
  id: number
  title: string
  status: OccurrenceStatus
  priority: Priority
  assignedToName: string | null
  messageCount: number
  createdAt: string
  byMe: boolean
}

export interface OccurrenceDetail {
  id: number
  title: string
  description: string | null
  status: OccurrenceStatus
  priority: Priority
  chatId: number
  assignedToUserId: number | null
  assignedToName: string | null
  createdByUserId: number | null
  createdByName: string | null
  messageCount: number
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

export const OCCURRENCE_STATUS_INT: Record<OccurrenceStatus, number> = {
  Open: 0,
  InProgress: 1,
  Resolved: 2,
  Closed: 3,
}
