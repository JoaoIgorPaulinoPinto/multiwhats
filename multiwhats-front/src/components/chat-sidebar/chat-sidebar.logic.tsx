'use client'

import { useCallback, useEffect, useRef, useState } from 'react'
import {
  chatsService,
  type ChatListResponse,
} from '../../services/chats.service'
import { ws } from '../../services/websocket'
import { useAuthStore } from '../../stores/auth-store'
import { toNumericStatus } from '../../types'

const CHAT_PAGE_SIZE = 30

interface ChatCache {
  chats: ChatListResponse[]
  page: number
  totalCount: number
  totalPages: number
  hasNext: boolean
}

let cachedChats: ChatCache | null = null

export type ChatTypeFilter = 'open' | 'mine' | 'all'

export function useChatSidebar() {
  const [search, setSearch] = useState('')
  const [chatType, setChatType] = useState<ChatTypeFilter>('all')
  const [onlyMine, setOnlyMine] = useState(true)
  const [chats, setChats] = useState<ChatListResponse[]>(
    () => cachedChats?.chats ?? [],
  )
  const [page, setPage] = useState(() => cachedChats?.page ?? 1)
  const [totalCount, setTotalCount] = useState(
    () => cachedChats?.totalCount ?? 0,
  )
  const [hasNext, setHasNext] = useState(() => cachedChats?.hasNext ?? false)
  const [loading, setLoading] = useState(cachedChats === null)
  const [loadingMore, setLoadingMore] = useState(false)
  const currentUserId = useAuthStore((s) => s.user?.id)

  const stateRef = useRef({ page, chats })

  const applyResponse = useCallback((next: ChatCache) => {
    stateRef.current = { page: next.page, chats: next.chats }
    cachedChats = next
    setChats(next.chats)
    setPage(next.page)
    setTotalCount(next.totalCount)
    setHasNext(next.hasNext)
  }, [])

  const load = useCallback(() => {
    chatsService
      .listChats(1, CHAT_PAGE_SIZE)
      .then((res) => {
        applyResponse({
          chats: res.items,
          page: res.page,
          totalCount: res.totalCount,
          totalPages: res.totalPages,
          hasNext: res.hasNext,
        })
      })
      .catch((e) => console.error(`[ChatSidebar] erro ao carregar:`, e))
      .finally(() => setLoading(false))
  }, [applyResponse])

  const loadMore = useCallback(() => {
    if (loadingMore || !hasNext) return
    setLoadingMore(true)
    chatsService
      .listChats(page + 1, CHAT_PAGE_SIZE)
      .then((res) => {
        applyResponse({
          chats: [...chats, ...res.items],
          page: res.page,
          totalCount: res.totalCount,
          totalPages: res.totalPages,
          hasNext: res.hasNext,
        })
      })
      .catch((e) => console.error(`[ChatSidebar] erro ao carregar mais:`, e))
      .finally(() => setLoadingMore(false))
  }, [applyResponse, chats, page, hasNext, loadingMore])

  const refresh = useCallback(() => {
    const { page: currentPage, chats: currentChats } = stateRef.current
    chatsService
      .listChats(1, CHAT_PAGE_SIZE)
      .then((res) => {
        const seen = new Set<number>()
        const merged: ChatListResponse[] = []
        for (const item of res.items) {
          if (seen.has(item.id)) continue
          seen.add(item.id)
          merged.push(item)
        }
        for (const item of currentChats.slice(CHAT_PAGE_SIZE)) {
          if (seen.has(item.id)) continue
          seen.add(item.id)
          merged.push(item)
        }
        merged.sort((a, b) =>
          (b.lastMessageAt ?? b.createdAt).localeCompare(
            a.lastMessageAt ?? a.createdAt,
          ),
        )
        applyResponse({
          chats: merged,
          page: currentPage,
          totalCount: res.totalCount,
          totalPages: res.totalPages,
          hasNext: res.hasNext,
        })
      })
      .catch((e) => console.error(`[ChatSidebar] erro ao atualizar:`, e))
  }, [applyResponse])

  useEffect(() => {
    load()

    const unsubReceived = ws.on('message:received', refresh)
    const unsubSent = ws.on('message:sent', refresh)
    const unsubStatus = ws.on('message:delivery-status', refresh)
    const unsubAssigned = ws.on('chat:assigned', load)
    const unsubUnassigned = ws.on('chat:unassigned', load)

    return () => {
      unsubReceived()
      unsubSent()
      unsubStatus()
      unsubAssigned()
      unsubUnassigned()
    }
  }, [load, refresh])

  const filtered = chats.filter((c) => {
    const q = search.toLowerCase()
    const display = c.contactName ?? c.name ?? c.phoneNumber ?? `Chat #${c.id}`
    if (!display.toLowerCase().includes(q)) return false
    const hasOccurrence = (c.occurrences?.length ?? 0) > 0
    const assignedByMe =
      currentUserId != null && c.assignedToUserId === currentUserId
    const myOccurrence = (c.occurrences ?? []).some((o) => o.byMe)
    const isAssigned = c.assignedToUserId != null
    const lastIncoming = c.lastMessage?.direction === 0
    const lastUnread =
      c.lastMessage != null &&
      toNumericStatus(c.lastMessage.deliveryStatus ?? 2) < 3

    if (chatType === 'open')
      return (
        lastIncoming &&
        lastUnread &&
        !isAssigned &&
        !assignedByMe
      )
    if (chatType === 'mine')
      return onlyMine
        ? assignedByMe || myOccurrence
        : isAssigned || hasOccurrence
    return true
  })

  return {
    search,
    setSearch,
    chatType,
    setChatType,
    onlyMine,
    setOnlyMine,
    chats: filtered,
    loading,
    load,
    loadMore,
    loadingMore,
    hasNext,
    totalCount,
  }
}
