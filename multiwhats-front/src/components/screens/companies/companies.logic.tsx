"use client"

import { useEffect, useState } from "react"
import { api, type Client, type Contact } from "../../../services/api"

export function useCompanies() {
  const [companies, setCompanies] = useState<Client[]>([])
  const [allContacts, setAllContacts] = useState<Contact[]>([])
  const [editing, setEditing] = useState<Client | null>(null)
  const [formName, setFormName] = useState("")
  const [formPhone, setFormPhone] = useState("")
  const [formStatus, setFormStatus] = useState("Active")
  const [search, setSearch] = useState("")
  const [loading, setLoading] = useState(true)

  const loadData = () => {
    console.log(`[Companies] carregando lista...`)
    return Promise.all([
      api.get<Client[]>("/api/clients"),
      api.get<Contact[]>("/api/contacts"),
    ]).then(([c, ct]) => {
      console.log(`[Companies] ${c.length} empresas, ${ct.length} contatos carregados`)
      setCompanies(c)
      setAllContacts(ct)
    }).catch((e) => console.error(`[Companies] erro ao carregar:`, e))
  }

  useEffect(() => {
    loadData().finally(() => setLoading(false))
  }, [])

  const filtered = companies.filter((c) =>
    c.name.toLowerCase().includes(search.toLowerCase())
  )

  function startEdit(company: Client) {
    setEditing(company)
    setFormName(company.name)
    setFormPhone(company.mainPhoneNumber ?? "")
    setFormStatus(company.status)
  }

  function cancelEdit() {
    setEditing(null)
    setFormName("")
    setFormPhone("")
    setFormStatus("Active")
  }

  async function saveEdit() {
    if (!editing) return
    console.log(`[Companies] salvando empresa #${editing.id}: nome="${formName}", phone="${formPhone}", status="${formStatus}"`)
    await api.put(`/api/clients/${editing.id}`, {
      name: formName,
      mainPhoneNumber: formPhone || null,
      status: formStatus,
    })
    console.log(`[Companies] empresa #${editing.id} atualizada`)
    await loadData()
    cancelEdit()
  }

  async function createCompany() {
    console.log(`[Companies] criando empresa: nome="${formName}", phone="${formPhone}"`)
    await api.post("/api/clients", { name: formName, mainPhoneNumber: formPhone || null })
    console.log(`[Companies] empresa criada`)
    await loadData()
    cancelEdit()
  }

  async function deleteCompany(id: number) {
    console.log(`[Companies] deletando empresa #${id}`)
    await api.delete(`/api/clients/${id}`)
    console.log(`[Companies] empresa #${id} deletada`)
    setCompanies((prev) => prev.filter((c) => c.id !== id))
  }

  const companyContacts = (clientId: number) =>
    allContacts.filter((c) => c.clientId === clientId)

  async function unassignContact(contactId: number) {
    console.log(`[Companies] removendo contato #${contactId} da empresa`)
    await api.patch(`/api/contacts/${contactId}/unassign`)
    console.log(`[Companies] contato #${contactId} removido`)
    const updated = await api.get<Contact[]>(`/api/contacts`)
    setAllContacts(updated)
  }

  return {
    companies: filtered,
    allContacts,
    loading,
    search,
    setSearch,
    editing,
    formName,
    formPhone,
    formStatus,
    setFormName,
    setFormPhone,
    setFormStatus,
    startEdit,
    cancelEdit,
    saveEdit,
    createCompany,
    deleteCompany,
    companyContacts,
    unassignContact,
  }
}
