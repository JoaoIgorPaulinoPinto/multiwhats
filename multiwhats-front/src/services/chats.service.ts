import { api } from "./api"
import type { PaginatedResponse } from "./paginated.response"
import type { OccurrenceStatus, Priority } from "../types"
import type { OccurrenceSummary, OccurrenceDetail } from "../types/occurrence"
import type { ChatListResponse, ChatDetailResponse, MessageResponse, MessageType } from "../types/chat"

export type { OccurrenceStatus, Priority, OccurrenceSummary, OccurrenceDetail, ChatListResponse, ChatDetailResponse, MessageResponse, MessageType }

export const chatsService = {
  listChats(page = 1, pageSize = 20) {
    return api.get<PaginatedResponse<ChatListResponse>>(`/api/chats?page=${page}&pageSize=${pageSize}`)
  },

  getChat(id: number) {
    return api.get<ChatDetailResponse>(`/api/chats/${id}`)
  },

  getMessages(chatId: number, page = 1, pageSize = 50) {
    return api.get<PaginatedResponse<MessageResponse>>(
      `/api/chats/${chatId}/messages?page=${page}&pageSize=${pageSize}`,
    )
  },

  getOccurrences(chatId: number) {
    return api.get(`/api/chats/${chatId}/occurrences`)
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
