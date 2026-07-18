"use client"

import { useEffect, useState } from "react"
import { api, type Contact, type Client } from "../../../services/api"

export function useContacts() {
  const [contacts, setContacts] = useState<Contact[]>([])
  const [clients, setClients] = useState<Client[]>([])
  const [editing, setEditing] = useState<Contact | null>(null)
  const [formName, setFormName] = useState("")
  const [formPushName, setFormPushName] = useState("")
  const [assignClientId, setAssignClientId] = useState<number | null>(null)
  const [search, setSearch] = useState("")
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    console.log(`[Contacts] carregando lista...`)
    Promise.all([
      api.get<Contact[]>("/api/contacts"),
      api.get<Client[]>("/api/clients"),
    ]).then(([c, cl]) => {
      console.log(`[Contacts] ${c.length} contatos, ${cl.length} empresas carregados`)
      setContacts(c)
      setClients(cl)
    }).catch((e) => console.error(`[Contacts] erro ao carregar:`, e)).finally(() => setLoading(false))
  }, [])

  const filtered = contacts.filter((c) => {
    const q = search.toLowerCase()
    return (c.name ?? c.pushName ?? c.phoneNumber).toLowerCase().includes(q)
  })

  function startEdit(contact: Contact) {
    setEditing(contact)
    setFormName(contact.name ?? "")
    setFormPushName(contact.pushName ?? "")
    setAssignClientId(contact.clientId)
  }

  function cancelEdit() {
    setEditing(null)
    setFormName("")
    setFormPushName("")
    setAssignClientId(null)
  }

  async function saveEdit() {
    if (!editing) return
    console.log(`[Contacts] salvando contato #${editing.id}: nome="${formName}", pushName="${formPushName}", clientId=${assignClientId}`)
    await api.put(`/api/contacts/${editing.id}`, { name: formName, pushName: formPushName })
    if (assignClientId !== editing.clientId) {
      if (assignClientId) {
        console.log(`[Contacts] atribuindo contato #${editing.id} ao cliente #${assignClientId}`)
        await api.patch(`/api/contacts/${editing.id}/assign`, { clientId: assignClientId })
      } else {
        console.log(`[Contacts] desatrelar contato #${editing.id} do cliente`)
        await api.patch(`/api/contacts/${editing.id}/unassign`)
      }
    }
    const updated = await api.get<Contact>(`/api/contacts/${editing.id}`)
    console.log(`[Contacts] contato #${editing.id} atualizado`)
    setContacts((prev) => prev.map((c) => (c.id === editing.id ? updated : c)))
    cancelEdit()
  }

  async function deleteContact(id: number) {
    console.log(`[Contacts] deletando contato #${id}`)
    await api.delete(`/api/contacts/${id}`)
    console.log(`[Contacts] contato #${id} deletado`)
    setContacts((prev) => prev.filter((c) => c.id !== id))
  }

  return {
    contacts: filtered,
    clients,
    loading,
    search,
    setSearch,
    editing,
    formName,
    formPushName,
    assignClientId,
    setFormName,
    setFormPushName,
    setAssignClientId,
    startEdit,
    cancelEdit,
    saveEdit,
    deleteContact,
  }
}
