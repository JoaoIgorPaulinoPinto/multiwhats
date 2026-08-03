"use client"

import { useChatMessaging } from "../../hooks/use-chat-messaging"
import { useSaveContactModal } from "../../hooks/use-save-contact-modal"
import { useOccurrenceModal } from "../../hooks/use-occurrence-modal"

export function useChatArea(chatId: number | null, jid: string, lastMessage?: string, lastMessageAt?: string | null, onOccurrenceCreated?: () => void, canMarkRead = false) {
  const messaging = useChatMessaging(chatId, jid, lastMessage, lastMessageAt, canMarkRead)
  const saveContact = useSaveContactModal(jid)
  const occurrence = useOccurrenceModal(chatId, onOccurrenceCreated)

  return {
    ...messaging,
    saveContact,
    occurrence,
  }
}
