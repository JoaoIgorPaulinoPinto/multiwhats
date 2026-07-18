"use client"

import { useEffect, useState, useRef } from "react"
import { api, type Message } from "../../services/api"

const cache = new Map<number, Message[]>()

export function useChatArea(contactId?: number | null) {
  const [inputValue, setInputValue] = useState("")
  const [messages, setMessages] = useState<Message[]>([])
  const lastFetched = useRef<number | null>(null)

  useEffect(() => {
    setInputValue("")

    if (!contactId) {
      setMessages([])
      return
    }

    const cached = cache.get(contactId)
    if (cached) {
      console.log(`[ChatArea] cache hit para contato #${contactId} (${cached.length} mensagens)`)
      setMessages(cached)
      if (lastFetched.current === contactId) return
    }

    lastFetched.current = contactId
    console.log(`[ChatArea] ${cached ? "background refresh" : "carregando"} mensagens do contato #${contactId}...`)
    api
      .get<{ messages?: Message[] }>(`/api/messages/contact/${contactId}`)
      .then((data) => {
        const msgs = Array.isArray(data) ? data : (data as { messages?: Message[] }).messages ?? []
        console.log(`[ChatArea] ${msgs.length} mensagens carregadas para contato #${contactId}`)
        cache.set(contactId, msgs)
        setMessages(msgs)
      })
      .catch((e) => {
        console.error(`[ChatArea] erro ao carregar mensagens:`, e)
        if (!cached) setMessages([])
      })
  }, [contactId])

  async function sendMessage() {
    if (!inputValue.trim() || !contactId) return
    console.log(`[ChatArea] enviando mensagem para contato #${contactId}: "${inputValue.trim().slice(0, 80)}"`)
    try {
      await api.post("/api/messages/send", {
        phoneNumber: inputValue.trim(),
        text: inputValue.trim(),
      })
      console.log(`[ChatArea] mensagem enviada — invalidando cache do contato #${contactId}`)
      cache.delete(contactId)
      setInputValue("")
    } catch (e) {
      console.error(`[ChatArea] falha ao enviar mensagem:`, e)
    }
  }

  return { inputValue, setInputValue, messages, sendMessage }
}
