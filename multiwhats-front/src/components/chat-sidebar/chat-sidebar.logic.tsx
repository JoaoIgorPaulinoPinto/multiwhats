'use client'

import { useCallback, useEffect, useState } from 'react'
import {
  chatsService,
  type ChatListResponse,
} from '../../services/chats.service'
import { ws } from '../../services/websocket'

let cachedChats: ChatListResponse[] | null = null

export type ChatTypeFilter = 'open' | 'mine' | 'all'

export function useChatSidebar() {
  const [search, setSearch] = useState('')
  const [chatType, setChatType] = useState<ChatTypeFilter>('all')
  const [chats, setChats] = useState<ChatListResponse[]>(
    () => cachedChats ?? [],
  )
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

    const unsubReceived = ws.on('message:received', load)
    const unsubSent = ws.on('message:sent', load)

    return () => {
      unsubReceived()
      unsubSent()
    }
  }, [load])

  const filtered = chats.filter((c) => {
    const q = search.toLowerCase()
    const display = c.contactName ?? c.name ?? c.phoneNumber ?? `Chat #${c.id}`
    if (!display.toLowerCase().includes(q)) return false
    const hasOccurrence = (c.occurrences?.length ?? 0) > 0
    const hasMyOccurrence = (c.occurrences ?? []).some((o) => o.byMe)
    if (chatType === 'open') return !hasOccurrence
    if (chatType === 'mine') return hasMyOccurrence
    return true
  })

  return {
    search,
    setSearch,
    chatType,
    setChatType,
    chats: filtered,
    loading,
    load,
  }
}
