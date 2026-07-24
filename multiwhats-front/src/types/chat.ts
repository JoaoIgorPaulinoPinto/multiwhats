import type { MessageDirection, MessageType, DeliveryStatus } from "./index"
import type { OccurrenceSummary, OccurrenceDetail } from "./occurrence"

export type { MessageDirection, MessageType, DeliveryStatus }

export interface ChatListResponse {
  id: number
  jid: string
  name: string
  phoneNumber: string | null
  contactId: number | null
  contactName: string | null
  clientId: number | null
  clientName: string | null
  lastMessageAt: string | null
  lastMessage: {Type: string, Body:string}
  assignedToUserName: string | null
  messageCount: number
  occurrences: OccurrenceSummary[] | null
  createdAt: string
}

export interface ChatDetailResponse {
  id: number
  jid: string
  phoneNumber: string | null
  name: string | null
  contactId: number | null
  contactName: string | null
  clientId: number | null
  clientName: string | null
  lastMessageAt: string | null
  lastMessage: {messageType: string, messgeBody:string}
  occurrences: OccurrenceDetail[] | null
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
