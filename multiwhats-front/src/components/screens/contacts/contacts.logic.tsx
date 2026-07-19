"use client"

import { useEffect, useState } from "react"
import { contactsService, type ContactResponse } from "../../../services/contacts.service"
import { companiesService, type ClientResponse } from "../../../services/companies.service"

export function useContacts() {
  const [contacts, setContacts] = useState<ContactResponse[]>([])
  const [clients, setClients] = useState<ClientResponse[]>([])
  const [editing, setEditing] = useState<ContactResponse | null>(null)
  const [creating, setCreating] = useState(false)
  const [formJid, setFormJid] = useState("")
  const [formPhone, setFormPhone] = useState("")
  const [formName, setFormName] = useState("")
  const [formPushName, setFormPushName] = useState("")
  const [assignClientId, setAssignClientId] = useState<number | null>(null)
  const [search, setSearch] = useState("")
  const [loading, setLoading] = useState(true)

  const loadData = () =>
    Promise.all([
      contactsService.list(),
      companiesService.list(),
    ]).then(([c, cl]) => {
      setContacts(c)
      setClients(cl)
    })

  useEffect(() => {
    console.log(`[Contacts] carregando lista...`)
    loadData().catch((e) => console.error(`[Contacts] erro ao carregar:`, e)).finally(() => setLoading(false))
  }, [])

  const filtered = contacts.filter((c) => {
    const q = search.toLowerCase()
    return (c.name ?? c.pushName ?? c.phoneNumber).toLowerCase().includes(q)
  })

  function resetForm() {
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
    console.log(`[Contacts] salvando contato #${editing.id}...`)
    try {
      await contactsService.update(editing.id, { name: formName, pushName: formPushName })
      if (assignClientId !== editing.clientId) {
        if (assignClientId) {
          await contactsService.assign(editing.id, assignClientId)
        } else {
          await contactsService.unassign(editing.id)
        }
      }
      console.log(`[Contacts] contato #${editing.id} atualizado`)
      const updated = await contactsService.getById(editing.id)
      setContacts((prev) => prev.map((c) => (c.id === editing.id ? updated : c)))
      cancelEdit()
    } catch (e) {
      console.error(`[Contacts] erro ao salvar:`, e)
    }
  }

  async function createContact() {
    console.log(`[Contacts] criando contato...`)
    try {
      await contactsService.create({
        jid: formJid,
        phoneNumber: formPhone,
        name: formName || undefined,
        pushName: formPushName || undefined,
      })
      console.log(`[Contacts] contato criado`)
      await loadData()
      cancelEdit()
    } catch (e) {
      console.error(`[Contacts] erro ao criar:`, e)
    }
  }

  async function handleDeleteContact(id: number) {
    console.log(`[Contacts] deletando contato #${id}...`)
    try {
      await contactsService.delete(id)
      console.log(`[Contacts] contato #${id} deletado`)
      setContacts((prev) => prev.filter((c) => c.id !== id))
    } catch (e) {
      console.error(`[Contacts] erro ao deletar:`, e)
    }
  }

  const modalOpen = creating || editing !== null

  return {
    contacts: filtered,
    clients,
    loading,
    search,
    setSearch,
    creating,
    editing,
    formJid,
    formPhone,
    formName,
    formPushName,
    assignClientId,
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
