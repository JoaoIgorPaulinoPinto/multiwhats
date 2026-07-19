"use client"

import { useEffect, useState, useRef } from "react"
import { chatsService, type MessageResponse } from "../../services/chats.service"

const cache = new Map<number, MessageResponse[]>()

function normalizePhone(raw: string): string {
  return raw.replace(/\D/g, "")
}

function isValidPhone(raw: string): boolean {
  const digits = normalizePhone(raw)
  if (digits.length < 10 || digits.length > 13) return false
  return true
}

export function useChatArea(chatId?: number | null, phoneNumber?: string) {
  const [inputValue, setInputValue] = useState("")
  const [messages, setMessages] = useState<MessageResponse[]>([])
  const [loading, setLoading] = useState(false)
  const [sendError, setSendError] = useState<string | null>(null)
  const lastFetched = useRef<number | null>(null)

  useEffect(() => {
    setInputValue("")
    setSendError(null)

    if (!chatId) {
      setMessages([])
      return
    }

    const cached = cache.get(chatId)
    if (cached) {
      console.log(`[ChatArea] cache hit para chat #${chatId} (${cached.length} mensagens)`)
      setMessages(cached)
      if (lastFetched.current === chatId) return
    }

    lastFetched.current = chatId
    setLoading(true)
    console.log(`[ChatArea] ${cached ? "background refresh" : "carregando"} mensagens do chat #${chatId}...`)
    chatsService
      .getMessages(chatId)
      .then((res) => {
        console.log(`[ChatArea] ${res.items.length} mensagens carregadas para chat #${chatId}`)
        cache.set(chatId, res.items)
        setMessages(res.items)
      })
      .catch((e) => {
        console.error(`[ChatArea] erro ao carregar mensagens:`, e)
        if (!cached) setMessages([])
      })
      .finally(() => setLoading(false))
  }, [chatId])

  async function handleSendMessage() {
    setSendError(null)

    if (!chatId) return
    if (!inputValue.trim()) return

    if (!phoneNumber) {
      setSendError("Número de telefone não disponível para este chat")
      return
    }

    if (!isValidPhone(phoneNumber)) {
      setSendError(
        `O número "${phoneNumber}" parece inválido. Deve conter apenas dígitos (ex: 5515997076327).`,
      )
      return
    }

    const cleaned = normalizePhone(phoneNumber)
    console.log(`[ChatArea] enviando mensagem para ${cleaned}: "${inputValue.trim().slice(0, 80)}"`)
    try {
      await chatsService.sendMessage(cleaned, inputValue.trim())
      console.log(`[ChatArea] mensagem enviada — invalidando cache do chat #${chatId}`)
      cache.delete(chatId)
      setInputValue("")
    } catch (e) {
      const message = e instanceof Error ? e.message : "Erro ao enviar mensagem"
      setSendError(message)
      console.error(`[ChatArea] falha ao enviar mensagem:`, e)
    }
  }

  return { inputValue, setInputValue, messages, loading, sendError, sendMessage: handleSendMessage }
}
