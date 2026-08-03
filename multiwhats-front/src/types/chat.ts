import type { DeliveryStatus, MessageDirection, MessageType } from './index'
import type { OccurrenceDetail, OccurrenceSummary } from './occurrence'

export type { DeliveryStatus, MessageDirection, MessageType }

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
  lastMessage: {
    type: number
    body: string | null
    direction?: MessageDirection
    deliveryStatus?: DeliveryStatus
  } | null
  assignedToUserId: number | null
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
  lastMessage: {
    type: number
    body: string | null
    direction?: MessageDirection
    deliveryStatus?: DeliveryStatus
  } | null
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
  source?: number | null
  fromMe?: boolean
  chatId: number
  userId: number | null
  occurrenceId: number | null
  replyToId: number | null
  createdAt: string
}
