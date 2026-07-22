"use client"

import { useEffect, useState, useRef, useCallback } from "react"
import { chatsService, type MessageResponse } from "../../services/chats.service"
import { contactsService } from "../../services/contacts.service"
import { companiesService, type ClientResponse } from "../../services/companies.service"
import { ws } from "../../services/websocket"

const cache = new Map<number, MessageResponse[]>()

export function useChatArea(chatId: number | null, jid: string, lastMessage?: string, lastMessageAt?: string | null) {
  const [inputValue, setInputValue] = useState("")
  const [messages, setMessages] = useState<MessageResponse[]>([])
  const [loading, setLoading] = useState(false)
  const [sendError, setSendError] = useState<string | null>(null)
  const [sending, setSending] = useState(false)
  const lastFetched = useRef<number | null>(null)

  const [showSaveModal, setShowSaveModal] = useState(false)
  const [formJid, setFormJid] = useState("")
  const [formPhone, setFormPhone] = useState("")
  const [formName, setFormName] = useState("")
  const [formPushName, setFormPushName] = useState("")
  const [assignClientId, setAssignClientId] = useState<number | null>(null)
  const [clients, setClients] = useState<ClientResponse[]>([])
  const [saveLoading, setSaveLoading] = useState(false)
  const [saveError, setSaveError] = useState<string | null>(null)

  useEffect(() => {
    companiesService.list().then(setClients).catch(console.error)
  }, [])

  const handleNewMessage = useCallback((payload: unknown) => {
    const msg = payload as MessageResponse
    if (msg.chatId === chatId) {
      setMessages((prev) => [...prev, msg])
      cache.delete(chatId)
    }
  }, [chatId])

  useEffect(() => {
    const unsubReceived = ws.on("message:received", handleNewMessage)
    const unsubSent = ws.on("message:sent", handleNewMessage)
    return () => { unsubReceived(); unsubSent() }
  }, [handleNewMessage])

  useEffect(() => {
    setInputValue("")
    setSendError(null)
    setShowSaveModal(false)

    if (!chatId) {
      setMessages([])
      return
    }

    const cached = cache.get(chatId)
    if (cached) {
      setMessages(cached)
      if (lastFetched.current === chatId) return
    }

    if (!cached && lastMessage) {
      setMessages([{
        id: 0,
        messageId: null,
        fromJid: "",
        toJid: null,
        phoneNumber: "",
        body: lastMessage,
        direction: 0 as const,
        type: "Text" as const,
        timestamp: 0,
        sentAt: lastMessageAt ?? "",
        notifyName: null,
        hasMedia: false,
        mediaUrl: null,
        mediaMimeType: null,
        mediaFilename: null,
        mediaSize: null,
        mediaCaption: null,
        deliveryStatus: "Delivered" as const,
        isForwarded: false,
        chatId,
        userId: null,
        occurrenceId: null,
        replyToId: null,
        createdAt: lastMessageAt ?? "",
      }])
    }

    const requestChatId = chatId
    lastFetched.current = chatId
    setLoading(true)
    chatsService
      .getMessages(chatId)
      .then((res) => {
        if (requestChatId !== lastFetched.current) return
        const newItems = res.items
        const oldItems = cache.get(chatId) ?? []
        const isSame =
          newItems.length === oldItems.length &&
          newItems.every((m, i) => m.id === oldItems[i].id && m.body === oldItems[i].body)
        cache.set(chatId, newItems)
        if (!isSame) setMessages(newItems.slice())
      })
      .catch((e) => {
        console.error(`[ChatArea] erro ao carregar mensagens:`, e)
        if (requestChatId === lastFetched.current && !cached) setMessages([])
      })
      .finally(() => {
        if (requestChatId === lastFetched.current) setLoading(false)
      })
  }, [chatId])

  function openSaveModal(phone: string, name: string) {
    setFormJid(jid)
    setFormPhone(phone)
    setFormName(name || "")
    setFormPushName("")
    setAssignClientId(null)
    setSaveError(null)
    setShowSaveModal(true)
  }

  function closeSaveModal() {
    setShowSaveModal(false)
    setFormJid("")
    setFormPhone("")
    setFormName("")
    setFormPushName("")
    setAssignClientId(null)
    setSaveError(null)
  }

  async function createContact() {
    if (!formJid || !formPhone) return
    setSaveLoading(true)
    setSaveError(null)
    try {
      await contactsService.create({
        jid: formJid,
        phoneNumber: formPhone,
        name: formName || undefined,
        pushName: formPushName || undefined,
      })
      closeSaveModal()
    } catch (e) {
      const message = e instanceof Error ? e.message : "Erro ao salvar contato"
      setSaveError(message)
      console.error(`[ChatArea] erro ao criar contato:`, e)
    } finally {
      setSaveLoading(false)
    }
  }

  async function sendMessage() {
    setSendError(null)
    if (!chatId || !inputValue.trim()) return

    setSending(true)
    try {
      await chatsService.sendMessage(jid, inputValue.trim())
      cache.delete(chatId)
      setInputValue("")
    } catch (e) {
      const message = e instanceof Error ? e.message : "Erro ao enviar mensagem"
      setSendError(message)
      console.error(`[ChatArea] falha ao enviar mensagem:`, e)
    } finally {
      setSending(false)
    }
  }

  return {
    inputValue,
    setInputValue,
    messages,
    loading,
    sending,
    sendError,
    sendMessage,
    showSaveModal,
    formJid,
    formPhone,
    formName,
    formPushName,
    assignClientId,
    clients,
    saveLoading,
    saveError,
    setFormPhone,
    setFormName,
    setAssignClientId,
    openSaveModal,
    closeSaveModal,
    createContact,
  }
}
