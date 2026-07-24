"use client"

import { useChatMessaging } from "../../hooks/use-chat-messaging"
import { useSaveContactModal } from "../../hooks/use-save-contact-modal"
import { useOccurrenceModal } from "../../hooks/use-occurrence-modal"

export function useChatArea(chatId: number | null, jid: string, lastMessage?: string, lastMessageAt?: string | null) {
  const messaging = useChatMessaging(chatId, jid, lastMessage, lastMessageAt)
  const saveContact = useSaveContactModal(jid)
  const occurrence = useOccurrenceModal(chatId)

  return {
    ...messaging,
    saveContact,
    occurrence,
  }
}
