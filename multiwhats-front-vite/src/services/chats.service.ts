import type { OccurrenceStatus, Priority } from '../types'
import type {
  ChatDetailResponse,
  ChatListResponse,
  MessageResponse,
  MessageType,
} from '../types/chat'
import type { OccurrenceDetail, OccurrenceSummary } from '../types/occurrence'
import { api } from './api'
import type { PaginatedResponse } from './paginated.response'

export type {
  ChatDetailResponse,
  ChatListResponse,
  MessageResponse,
  MessageType,
  OccurrenceDetail,
  OccurrenceStatus,
  OccurrenceSummary,
  Priority,
}

export interface ChatFullInfoOccurrenceSummary {
  id: number
  title: string
  status: number
  priority: number
  assignedToName: string | null
  messageCount: number
  createdAt: string
  byMe: boolean
}

export interface ChatFullInfoResponse {
  id: number
  jid: string
  phoneNumber: string | null
  name: string | null
  contactId: number | null
  contactName: string | null
  contactPushName: string | null
  contactProfilePicUrl: string | null
  contactIsBlocked: boolean
  contactIsGroup: boolean
  clientId: number | null
  clientName: string | null
  clientMainPhoneNumber: string | null
  assignedToUserId: number | null
  assignedToUserName: string | null
  createdByUserId: number | null
  createdByName: string | null
  lastMessageAt: string | null
  lastMessage: { type: number; body: string | null } | null
  messageCount: number
  outgoingMessageCount: number
  incomingMessageCount: number
  imageCount: number
  videoCount: number
  audioCount: number
  documentCount: number
  textCount: number
  stickerCount: number
  mediaCount: number
  mediaSentCount: number
  daysActive: number
  timeSinceLastOccurrenceSeconds: number | null
  occurrences: ChatFullInfoOccurrenceSummary[]
  occurrenceCount: number
  createdAt: string
  lastUpdate: string
}

export interface ChatHistoryAtendimento {
  id: number
  startedAt: string
  endedAt: string | null
  isOpen: boolean
  startedByUserId: number | null
  startedByName: string | null
  endedByUserId: number | null
  endedByName: string | null
  durationSeconds: number | null
}

export interface ChatHistoryOccurrence {
  id: number
  title: string
  description: string | null
  status: number
  priority: number
  createdByUserId: number | null
  createdByName: string | null
  assignedToUserId: number | null
  assignedToName: string | null
  createdAt: string
  lastUpdate: string
}

export type ChatHistoryTimelineType =
  | "AtendimentoIniciado"
  | "AtendimentoFinalizado"
  | "OcorrenciaCriada"
  | "OcorrenciaAtualizada"
  | "OcorrenciaExcluida"

export interface ChatHistoryTimelineItem {
  type: ChatHistoryTimelineType
  title: string
  description: string
  timestamp: string
  userId: number | null
  userName: string | null
  occurrenceId: number | null
}

export interface ChatHistoryResponse {
  chatId: number
  atendimentos: ChatHistoryAtendimento[]
  occurrences: ChatHistoryOccurrence[]
  timeline: ChatHistoryTimelineItem[]
}

export const chatsService = {
  listChats(page = 1, pageSize = 30) {
    return api.get<PaginatedResponse<ChatListResponse>>(
      `/api/chats?page=${page}&pageSize=${pageSize}`,
    )
  },

  listAllChats() {
    return api.get<PaginatedResponse<ChatListResponse>>(
      `/api/chats?page=1&pageSize=10000`,
    )
  },

  getChat(id: number) {
    return api.get<ChatDetailResponse>(`/api/chats/${id}`)
  },

  getFullInfo(id: number) {
    return api.get<ChatFullInfoResponse>(`/api/chats/${id}/full-info`)
  },

  getHistory(id: number) {
    return api.get<ChatHistoryResponse>(`/api/chats/${id}/history`)
  },

  getMessages(chatId: number, page = 1, pageSize = 100) {
    return api.get<PaginatedResponse<MessageResponse>>(
      `/api/chats/${chatId}/messages?page=${page}&pageSize=${pageSize}`,
    )
  },

  markChatRead(chatId: number) {
    return api.put(`/api/chats/${chatId}/read`)
  },

  assignChat(chatId: number) {
    return api.put<{
      id: number
      assignedToUserId: number | null
      assignedToUserName: string | null
    }>(`/api/chats/${chatId}/assign`)
  },

  unassignChat(chatId: number) {
    return api.put<{
      id: number
      assignedToUserId: number | null
      assignedToUserName: string | null
    }>(`/api/chats/${chatId}/unassign`)
  },

  getOccurrences(chatId: number) {
    return api.get(`/api/chats/${chatId}/occurrences`)
  },

  mergeChats(mergeJid: string, toJid: string) {
    return api.patch(
      `/api/chats/merge?mergeJid=${encodeURIComponent(mergeJid)}&toJid=${encodeURIComponent(toJid)}`,
    )
  },

  sendMessage(jid: string, text: string) {
    return api.post<MessageResponse>('/api/messages/send', { jid, text })
  },

  sendMediaMessage(
    jid: string,
    type: number,
    MediaBase64: string,
    options?: {
      text?: string
      mediaMimeType?: string
      mediaFilename?: string
      mediaCaption?: string
    },
  ) {
    return api.post<MessageResponse>('/api/messages/send', {
      jid,
      text: options?.text,
      type,
      MediaBase64,
      mediaMimeType: options?.mediaMimeType,
      mediaFilename: options?.mediaFilename,
      mediaCaption: options?.mediaCaption,
    })
  },
}
