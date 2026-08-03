import { api } from "./api"
import type { PaginatedResponse } from "./paginated.response"
import type { OccurrenceStatus, Priority } from "../types"
import type { OccurrenceSummary, OccurrenceDetail } from "../types/occurrence"
import type { ChatListResponse, ChatDetailResponse, MessageResponse, MessageType } from "../types/chat"

export type { OccurrenceStatus, Priority, OccurrenceSummary, OccurrenceDetail, ChatListResponse, ChatDetailResponse, MessageResponse, MessageType }

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

export const chatsService = {
  listChats(page = 1, pageSize = 20) {
    return api.get<PaginatedResponse<ChatListResponse>>(`/api/chats?page=${page}&pageSize=${pageSize}`)
  },

  listAllChats() {
    return api.get<PaginatedResponse<ChatListResponse>>(`/api/chats?page=1&pageSize=10000`)
  },

  getChat(id: number) {
    return api.get<ChatDetailResponse>(`/api/chats/${id}`)
  },

  getFullInfo(id: number) {
    return api.get<ChatFullInfoResponse>(`/api/chats/${id}/full-info`)
  },

  getMessages(chatId: number, page = 1, pageSize = 50) {
    return api.get<PaginatedResponse<MessageResponse>>(
      `/api/chats/${chatId}/messages?page=${page}&pageSize=${pageSize}`,
    )
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
    return api.post<MessageResponse>("/api/messages/send", { jid, text })
  },

  sendMediaMessage(jid: string, type: number, MediaBase64: string, options?: { text?: string; mediaMimeType?: string; mediaFilename?: string; mediaCaption?: string }) {
    return api.post<MessageResponse>("/api/messages/send", {
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
