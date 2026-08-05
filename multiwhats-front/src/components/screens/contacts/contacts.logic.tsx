"use client"

import { useEffect, useState } from "react"
import { contactsService, type ContactResponse } from "../../../services/contacts.service"
import { companiesService, type ClientResponse } from "../../../services/companies.service"
import { chatsService, type ChatListResponse } from "../../../services/chats.service"
import { useToast } from "../../../components/toast/toast.provider"

export function useContacts() {
  const { toast } = useToast()
  const [contacts, setContacts] = useState<ContactResponse[]>([])
  const [clients, setClients] = useState<ClientResponse[]>([])
  const [chats, setChats] = useState<ChatListResponse[]>([])
  const [editing, setEditing] = useState<ContactResponse | null>(null)
  const [creating, setCreating] = useState(false)
  const [formChatId, setFormChatId] = useState<number | "">("")
  const [formJid, setFormJid] = useState("")
  const [formPhone, setFormPhone] = useState("")
  const [formName, setFormName] = useState("")
  const [formPushName, setFormPushName] = useState("")
  const [assignClientId, setAssignClientId] = useState<number | null>(null)
  const [search, setSearch] = useState("")
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [deleting, setDeleting] = useState<number | null>(null)

  const loadData = () =>
    Promise.all([
      contactsService.list(),
      companiesService.list(),
      chatsService.listAllChats().then((res) => res.items),
    ]).then(([c, cl, chatItems]) => {
      setContacts(c)
      setClients(cl)
      setChats(chatItems)
    })

  useEffect(() => {
    loadData().catch((e) => console.error(`[Contacts] erro ao carregar:`, e)).finally(() => setLoading(false))
  }, [])

  const filtered = contacts.filter((c) => {
    const q = search.toLowerCase()
    return (c.name ?? c.pushName ?? c.phoneNumber).toLowerCase().includes(q)
  })

  function resetForm() {
    setFormChatId("")
    setFormJid("")
    setFormPhone("")
    setFormName("")
    setFormPushName("")
    setAssignClientId(null)
  }

  function startCreate() {
    resetForm()
    setCreating(true)
    setEditing(null)
  }

  function selectChat(chatId: number | "") {
    setFormChatId(chatId)
    if (chatId === "") {
      setFormJid("")
      setFormPhone("")
      setFormName("")
      setFormPushName("")
      setAssignClientId(null)
      return
    }
    const chat = chats.find((c) => c.id === chatId)
    if (!chat) return
    setFormJid(chat.jid)
    setFormPhone(chat.phoneNumber ?? "")
    setFormName(chat.contactName ?? chat.name ?? "")
    setFormPushName("")
    setAssignClientId(chat.clientId)
  }

  function startEdit(contact: ContactResponse) {
    setCreating(false)
    setEditing(contact)
    setFormJid(contact.jid)
    setFormPhone(contact.phoneNumber)
    setFormName(contact.name ?? "")
    setFormPushName(contact.pushName ?? "")
    setAssignClientId(contact.clientId)
  }

  function cancelEdit() {
    setCreating(false)
    setEditing(null)
    resetForm()
  }

  async function saveEdit() {
    if (!editing) return
    setSaving(true)
    try {
      await contactsService.update(editing.id, { name: formName, pushName: formPushName })
      if (assignClientId !== editing.clientId) {
        if (assignClientId) {
          await contactsService.assign(editing.id, assignClientId)
        } else {
          await contactsService.unassign(editing.id)
        }
      }
      const updated = await contactsService.getById(editing.id)
      setContacts((prev) => prev.map((c) => (c.id === editing.id ? updated : c)))
      toast.success("Contato atualizado com sucesso")
      cancelEdit()
    } catch (e) {
      console.error(`[Contacts] erro ao salvar:`, e)
      toast.error("Erro ao salvar contato")
    } finally {
      setSaving(false)
    }
  }

  async function createContact() {
    if (formChatId === "") return
    setSaving(true)
    try {
      const created = await contactsService.create({
        jid: formJid,
        phoneNumber: formPhone,
        name: formName || undefined,
        pushName: formPushName || undefined,
      })
      if (assignClientId) {
        await contactsService.assign(created.id, assignClientId)
      }
      toast.success("Contato criado com sucesso")
      await loadData()
      cancelEdit()
    } catch (e) {
      console.error(`[Contacts] erro ao criar:`, e)
      toast.error("Erro ao criar contato")
    } finally {
      setSaving(false)
    }
  }

  async function handleDeleteContact(id: number) {
    if (!window.confirm("Tem certeza que deseja excluir este contato?")) return
    setDeleting(id)
    try {
      await contactsService.delete(id)
      setContacts((prev) => prev.filter((c) => c.id !== id))
      toast.success("Contato excluído")
    } catch (e) {
      console.error(`[Contacts] erro ao deletar:`, e)
      toast.error("Erro ao excluir contato")
    } finally {
      setDeleting(null)
    }
  }

  const modalOpen = creating || editing !== null

  return {
    contacts: filtered,
    clients,
    chats,
    loading,
    saving,
    deleting,
    search,
    setSearch,
    creating,
    editing,
    formChatId,
    formJid,
    formPhone,
    formName,
    formPushName,
    assignClientId,
    selectChat,
    setFormJid,
    setFormPhone,
    setFormName,
    setFormPushName,
    setAssignClientId,
    startCreate,
    startEdit,
    cancelEdit,
    saveEdit,
    createContact,
    deleteContact: handleDeleteContact,
    modalOpen,
  }
}
