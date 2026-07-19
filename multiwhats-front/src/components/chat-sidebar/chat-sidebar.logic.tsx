"use client"

import { useEffect, useState } from "react"
import { chatsService, type ChatResponse } from "../../services/chats.service"

export function useChatSidebar() {
  const [search, setSearch] = useState("")
  const [chats, setChats] = useState<ChatResponse[]>([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    console.log(`[ChatSidebar] carregando chats...`)
    chatsService
      .listChats()
      .then((res) => {
        console.log(`[ChatSidebar] ${res.items.length} chats carregados`)
        setChats(res.items)
      })
      .catch((e) => console.error(`[ChatSidebar] erro ao carregar:`, e))
      .finally(() => setLoading(false))
  }, [])

  const filtered = chats.filter((c) => {
    const q = search.toLowerCase()
    const display = c.contactName ?? c.name ?? c.phoneNumber ?? `Chat #${c.id}`
    return display.toLowerCase().includes(q)
  })

  return { search, setSearch, chats: filtered, loading }
}
