"use client"

import { useEffect, useState, useCallback } from "react"
import { chatsService, type ChatListResponse } from "../../services/chats.service"
import { ws } from "../../services/websocket"

let cachedChats: ChatListResponse[] | null = null

export function useChatSidebar() {
  const [search, setSearch] = useState("")
  const [chats, setChats] = useState<ChatListResponse[]>(() => cachedChats ?? [])
  const [loading, setLoading] = useState(cachedChats === null)

  const load = useCallback(() => {
    chatsService
      .listChats()
      .then((res) => {
        cachedChats = res.items
        setChats(res.items)
      })
      .catch((e) => console.error(`[ChatSidebar] erro ao carregar:`, e))
      .finally(() => setLoading(false))
  }, [])

  useEffect(() => {
    load()

    const unsubReceived = ws.on("message:received", load)
    const unsubSent = ws.on("message:sent", load)

    return () => { unsubReceived(); unsubSent() }
  }, [load])

  const filtered = chats.filter((c) => {
    const q = search.toLowerCase()
    const display = c.contactName ?? c.name ?? c.phoneNumber ?? `Chat #${c.id}`
    return display.toLowerCase().includes(q)
  })

  return { search, setSearch, chats: filtered, loading, load }
}
