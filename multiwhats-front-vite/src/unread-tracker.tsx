import { useEffect, useRef } from "react"
import { useAuthStore } from "./stores/auth-store"
import { useUnreadStore } from "./stores/unread-store"
import { chatsService } from "./services/chats.service"
import { ws } from "./services/websocket"
import { toNumericStatus } from "./types"
import type { ChatListResponse, MessageResponse } from "./types/chat"

function messageKey(msg: Pick<MessageResponse, "messageId" | "chatId" | "id">): string {
  return msg.messageId ?? `${msg.chatId}:${msg.id}`
}

function isUnreadMessage(msg: MessageResponse): boolean {
  return msg.direction === 0 && toNumericStatus(msg.deliveryStatus) < 3
}

function chatHasUnreadLastMessage(chat: ChatListResponse): boolean {
  const lm = chat.lastMessage
  return !!lm && lm.direction === 0 && toNumericStatus(lm.deliveryStatus ?? 2) < 3
}

export function UnreadTracker() {
  const user = useAuthStore((s) => s.user)
  const total = useUnreadStore((s) => s.total)
  const addIncoming = useUnreadStore((s) => s.addIncoming)
  const removeRead = useUnreadStore((s) => s.removeRead)
  const seed = useUnreadStore((s) => s.seed)
  const seedRef = useRef(0)

  useEffect(() => {
    if (!user) return
    const runId = ++seedRef.current
    chatsService
      .listAllChats()
      .then(async (page) => {
        const unreadChats = page.items.filter(chatHasUnreadLastMessage)
        const counted: Record<string, number> = {}
        for (const chat of unreadChats) {
          try {
            const res = await chatsService.getMessages(chat.id)
            for (const m of res.items) {
              if (isUnreadMessage(m)) counted[messageKey(m)] = chat.id
            }
          } catch {
            // segue para o próximo chat se a busca de mensagens falhar
          }
        }
        if (runId !== seedRef.current) return
        seed(counted)
      })
      .catch(() => {})
  }, [user, seed])

  useEffect(() => {
    if (!user) return
    const unsubReceived = ws.on("message:received", (msg: MessageResponse) => {
      if (msg.direction === 0) addIncoming(messageKey(msg), msg.chatId)
    })
    const unsubStatus = ws.on("message:delivery-status", (msg: MessageResponse) => {
      if (toNumericStatus(msg.deliveryStatus) === 3) removeRead(messageKey(msg))
    })
    return () => {
      unsubReceived()
      unsubStatus()
    }
  }, [user, addIncoming, removeRead])

  useEffect(() => {
    document.title = total > 0 ? `(${total}) MultiWhats` : "MultiWhats"
  }, [total])

  return null
}
