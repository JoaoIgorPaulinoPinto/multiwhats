import { api } from "./api"
import type { PaginatedResponse } from "./paginated.response"

export type MessageDirection = 0 | 1
export type MessageType = "Text" | "Image" | "Audio" | "Video" | "Document" | "Sticker" | "Contact" | "Location" | "Unknown"
export type DeliveryStatus = "Pending" | "Sent" | "Delivered" | "Read" | "Failed" | 0 | 1 | 2 | 3

export interface ChatResponse {
  id: number
  jid: string
  phoneNumber: string
  name: string | null
  contactId: number | null
  contactName: string | null
  clientId: number | null
  clientName: string | null
  lastMessageAt: string | null
  lastMessageBody: string | null
  assignedToUserId: number | null
  assignedToUserName: string | null
  createdByUserId: number | null
  messageCount: number
  occurrenceCount: number
  createdAt: string
  lastUpdate: string
}

export interface MessageResponse {
  id: number
  messageId: string | null
  fromJid: string
  toJid: string | null
  phoneNumber: string
  body: string | null
  direction: MessageDirection
  type: MessageType
  timestamp: number
  sentAt: string
  notifyName: string | null
  hasMedia: boolean
  mediaUrl: string | null
  mediaMimeType: string | null
  mediaFilename: string | null
  mediaSize: number | null
  mediaCaption: string | null
  deliveryStatus: DeliveryStatus
  isForwarded: boolean
  chatId: number
  userId: number | null
  occurrenceId: number | null
  replyToId: number | null
  createdAt: string
}

export const chatsService = {
  listChats(page = 1, pageSize = 20) {
    return api.get<PaginatedResponse<ChatResponse>>(`/api/chats?page=${page}&pageSize=${pageSize}`)
  },

  getChat(id: number) {
    return api.get<ChatResponse>(`/api/chats/${id}`)
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
}
