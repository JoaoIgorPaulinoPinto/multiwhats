"use client"

import { useEffect, useState, useRef } from "react"
import { chatsService, type MessageResponse } from "../../services/chats.service"
import { contactsService } from "../../services/contacts.service"
import { companiesService, type ClientResponse } from "../../services/companies.service"

const cache = new Map<number, MessageResponse[]>()

export function useChatArea(chatId: number | null, jid: string) {
  const [inputValue, setInputValue] = useState("")
  const [messages, setMessages] = useState<MessageResponse[]>([])
  const [loading, setLoading] = useState(false)
  const [sendError, setSendError] = useState<string | null>(null)
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

    lastFetched.current = chatId
    setLoading(true)
    chatsService
      .getMessages(chatId)
      .then((res) => {
        cache.set(chatId, res.items)
        setMessages(res.items.reverse())
      })
      .catch((e) => {
        console.error(`[ChatArea] erro ao carregar mensagens:`, e)
        if (!cached) setMessages([])
      })
      .finally(() => setLoading(false))
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

    try {
      await chatsService.sendMessage(jid, inputValue.trim())
      cache.delete(chatId)
      setInputValue("")
    } catch (e) {
      const message = e instanceof Error ? e.message : "Erro ao enviar mensagem"
      setSendError(message)
      console.error(`[ChatArea] falha ao enviar mensagem:`, e)
    }
  }

  return {
    inputValue,
    setInputValue,
    messages,
    loading,
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
