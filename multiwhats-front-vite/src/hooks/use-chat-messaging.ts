
import { useCallback, useEffect, useRef, useState } from 'react'
import { chatsService } from '../services/chats.service'
import { ws } from '../services/websocket'
import { useUnreadStore } from '../stores/unread-store'
import { toNumericStatus, toNumericType } from '../types'
import type { MessageResponse, MessageType } from '../types/chat'
import { detectMediaType, fileToBase64 } from '../utils/media'
const cache = new Map<number, MessageResponse[]>()
const lastAccessed = new Map<number, number>()
const MAX_CACHED_CHATS = 5

function touchCache(chatId: number) {
  lastAccessed.set(chatId, Date.now())
}

function evictOldestCachedChat() {
  if (cache.size <= MAX_CACHED_CHATS) return
  let oldestId: number | null = null
  let oldestTs = Infinity
  lastAccessed.forEach((ts, id) => {
    if (ts < oldestTs) {
      oldestTs = ts
      oldestId = id
    }
  })
  if (oldestId !== null) {
    cache.delete(oldestId)
    lastAccessed.delete(oldestId)
  }
}

export function useChatMessaging(
  chatId: number | null,
  jid: string,
  lastMessage?: string,
  lastMessageAt?: string | null,
  canMarkRead = false,
) {
  const [inputValue, setInputValue] = useState('')
  const [messages, setMessages] = useState<MessageResponse[]>([])
  const [loading, setLoading] = useState(false)
  const [sendError, setSendError] = useState<string | null>(null)
  const [sendingCount, setSendingCount] = useState(0)
  const lastFetched = useRef<number | null>(null)

  const [selectedFile, setSelectedFile] = useState<File | null>(null)
  const [mediaPreview, setMediaPreview] = useState<string | null>(null)
  const [mediaType, setMediaType] = useState<MessageType | null>(null)
  const fileInputRef = useRef<HTMLInputElement>(null)

  const markChatAsRead = useCallback((chatId: number) => {
    useUnreadStore.getState().clearChat(chatId)
    const applyRead = (m: MessageResponse): MessageResponse =>
      m.direction === 0 && toNumericStatus(m.deliveryStatus) < 3
        ? { ...m, deliveryStatus: 'Read' as const }
        : m
    setMessages((prev) => prev.map(applyRead))
    const cached = cache.get(chatId)
    if (cached) {
      touchCache(chatId)
      cache.set(chatId, cached.map(applyRead))
    }
    chatsService.markChatRead(chatId).catch((e: unknown) => {
      console.error(`[ChatMessages] erro ao marcar mensagens como lidas:`, e)
    })
  }, [])

  const handleNewMessage = useCallback(
    (msg: MessageResponse) => {
      if (msg.chatId !== chatId) return
      setMessages((prev) => {
        const duplicate = prev.some(
          (m) =>
            m.id === msg.id ||
            (!!m.messageId &&
              !!msg.messageId &&
              m.messageId === msg.messageId),
        )
        if (duplicate) return prev
        return [...prev, msg]
      })
      if (chatId !== null) {
        cache.delete(chatId)
        lastAccessed.delete(chatId)
      }
      if (chatId !== null && msg.direction === 0 && canMarkRead)
        markChatAsRead(chatId)
    },
    [chatId, canMarkRead, markChatAsRead],
  )

  const handleDeliveryStatus = useCallback(
    (msg: MessageResponse) => {
      if (msg.chatId !== chatId || !msg.messageId) return
      const apply = (m: MessageResponse) =>
        m.messageId === msg.messageId
          ? { ...m, deliveryStatus: msg.deliveryStatus }
          : m
      setMessages((prev) => prev.map(apply))
      if (chatId !== null) {
        const cached = cache.get(chatId)
        if (cached) {
          touchCache(chatId)
          cache.set(chatId, cached.map(apply))
        }
      }
    },
    [chatId],
  )

  useEffect(() => {
    const unsubReceived = ws.on('message:received', handleNewMessage)
    const unsubSent = ws.on('message:sent', handleNewMessage)
    const unsubStatus = ws.on('message:delivery-status', handleDeliveryStatus)
    return () => {
      unsubReceived()
      unsubSent()
      unsubStatus()
    }
  }, [handleNewMessage, handleDeliveryStatus])

  useEffect(() => {
    setInputValue('')
    setSendError(null)
    setSelectedFile(null)
    setMediaPreview(null)
    setMediaType(null)

    if (!chatId || chatId === -1) {
      setMessages([])
      if (chatId === -1 && lastMessage) {
        setMessages([
          {
            id: -1,
            messageId: null,
            fromJid: '',
            toJid: null,
            phoneNumber: '',
            body: lastMessage,
            direction: 0 as const,
            type: 'Text' as const,
            timestamp: 0,
            sentAt: lastMessageAt ?? '',
            notifyName: null,
            hasMedia: false,
            mediaUrl: null,
            mediaMimeType: null,
            mediaFilename: null,
            mediaSize: null,
            mediaCaption: null,
            deliveryStatus: 'Delivered' as const,
            isForwarded: false,
            chatId: -1,
            userId: null,
            occurrenceId: null,
            replyToId: null,
            createdAt: lastMessageAt ?? '',
          },
        ])
      }
      return
    }

    const cached = cache.get(chatId)
    if (cached) {
      touchCache(chatId)
      setMessages(cached)
      if (canMarkRead) markChatAsRead(chatId)
      if (lastFetched.current === chatId) return
    }

    if (!cached && lastMessage) {
      setMessages([
        {
          id: -1,
          messageId: null,
          fromJid: '',
          toJid: null,
          phoneNumber: '',
          body: lastMessage,
          direction: 0 as const,
          type: 'Text' as const,
          timestamp: 0,
          sentAt: lastMessageAt ?? '',
          notifyName: null,
          hasMedia: false,
          mediaUrl: null,
          mediaMimeType: null,
          mediaFilename: null,
          mediaSize: null,
          mediaCaption: null,
          deliveryStatus: 'Delivered' as const,
          isForwarded: false,
          chatId,
          userId: null,
          occurrenceId: null,
          replyToId: null,
          createdAt: lastMessageAt ?? '',
        },
      ])
    }

    const requestChatId = chatId
    lastFetched.current = chatId
    setLoading(true)
    chatsService
      .getMessages(chatId)
      .then((res: { items: MessageResponse[] }) => {
        if (requestChatId !== lastFetched.current) return
        const seenIds = new Set<number>()
        const seenMsgIds = new Set<string>()
        const newItems = res.items.filter((m: MessageResponse) => {
          if (seenIds.has(m.id)) return false
          seenIds.add(m.id)
          if (m.messageId) {
            if (seenMsgIds.has(m.messageId)) return false
            seenMsgIds.add(m.messageId)
          }
          return true
        })
        const oldItems = cache.get(chatId) ?? []
        const isSame =
          newItems.length === oldItems.length &&
          newItems.every(
            (m: MessageResponse, i: number) =>
              m.id === oldItems[i].id &&
              m.body === oldItems[i].body &&
              m.mediaUrl === oldItems[i].mediaUrl &&
              m.hasMedia === oldItems[i].hasMedia,
          )
        cache.set(chatId, newItems)
        touchCache(chatId)
        evictOldestCachedChat()
        if (!isSame) setMessages(newItems.slice())
        if (canMarkRead) markChatAsRead(chatId)
      })
      .catch((e: unknown) => {
        console.error(`[ChatArea] erro ao carregar mensagens:`, e)
        if (requestChatId === lastFetched.current && !cached) setMessages([])
      })
      .finally(() => {
        if (requestChatId === lastFetched.current) setLoading(false)
      })
  }, [chatId, canMarkRead, markChatAsRead])

  function handleFileSelect(e: React.ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0]
    if (!file) return

    setSelectedFile(file)
    setMediaType(detectMediaType(file))

    const reader = new FileReader()
    reader.onload = () => setMediaPreview(reader.result as string)
    reader.readAsDataURL(file)
  }

  function clearMedia() {
    setSelectedFile(null)
    setMediaPreview(null)
    setMediaType(null)
    if (fileInputRef.current) fileInputRef.current.value = ''
  }

  function handleFileDrop(file: File) {
    setSelectedFile(file)
    setMediaType(detectMediaType(file))

    const reader = new FileReader()
    reader.onload = () => setMediaPreview(reader.result as string)
    reader.readAsDataURL(file)
  }

  async function sendMessage() {
    setSendError(null)
    if (!chatId) return

    const hasText = inputValue.trim().length > 0
    const hasMedia = selectedFile !== null
    if (!hasText && !hasMedia) return

    setSendingCount((c) => c + 1)
    const _inputValue = inputValue
    const _midia = selectedFile
    setInputValue('')
    clearMedia()
    try {
      if (hasMedia && selectedFile) {
        const base64 = await fileToBase64(selectedFile)
        await chatsService.sendMediaMessage(
          jid,
          toNumericType(mediaType!),
          base64,
          {
            text: _inputValue.trim() || undefined,
            mediaMimeType: _midia!.type,
            mediaFilename: _midia!.name,
            mediaCaption: _inputValue.trim() || undefined,
          },
        )
      } else {
        await chatsService.sendMessage(jid, _inputValue.trim())
      }
      cache.delete(chatId)
      lastAccessed.delete(chatId)
    } catch (e) {
      const message = e instanceof Error ? e.message : 'Erro ao enviar mensagem'
      setSendError(message)
      console.error(`[ChatArea] falha ao enviar mensagem:`, e)
    } finally {
      setSendingCount((c) => c - 1)
    }
  }

  return {
    inputValue,
    setInputValue,
    messages,
    loading,
    sendingCount,
    sendError,
    sendMessage,
    selectedFile,
    mediaPreview,
    mediaType,
    fileInputRef,
    handleFileSelect,
    handleFileDrop,
    clearMedia,
  }
}
