"use client"

import { useCallback, useEffect, useState } from "react"
import { companiesService, type ClientResponse } from "../../../services/companies.service"
import { contactsService, type ContactResponse } from "../../../services/contacts.service"

export function useCompanies() {
  const [companies, setCompanies] = useState<ClientResponse[]>([])
  const [allContacts, setAllContacts] = useState<ContactResponse[]>([])
  const [editing, setEditing] = useState<ClientResponse | null>(null)
  const [formName, setFormName] = useState("")
  const [formPhone, setFormPhone] = useState("")
  const [formStatus, setFormStatus] = useState("Active")
  const [search, setSearch] = useState("")
  const [loading, setLoading] = useState(true)

  const loadData = useCallback(() =>
    Promise.all([
      companiesService.list(),
      contactsService.list(),
    ]).then(([c, ct]) => {
      setCompanies(c)
      setAllContacts(ct)
    }),
  [])

  useEffect(() => {
    console.log(`[Companies] carregando lista...`)
    loadData().catch((e) => console.error(`[Companies] erro ao carregar:`, e)).finally(() => setLoading(false))
  }, [loadData])

  const filtered = companies.filter((c) =>
    c.name.toLowerCase().includes(search.toLowerCase())
  )

  function startEdit(company: ClientResponse) {
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
    console.log(`[Companies] salvando empresa #${editing.id}...`)
    try {
      await companiesService.update(editing.id, {
        name: formName,
        mainPhoneNumber: formPhone || null,
        status: formStatus,
      })
      console.log(`[Companies] empresa #${editing.id} atualizada`)
      await loadData()
      cancelEdit()
    } catch (e) {
      console.error(`[Companies] erro ao salvar:`, e)
    }
  }

  async function createNewCompany() {
    console.log(`[Companies] criando empresa...`)
    try {
      await companiesService.create({ name: formName, mainPhoneNumber: formPhone || null })
      console.log(`[Companies] empresa criada`)
      await loadData()
      cancelEdit()
    } catch (e) {
      console.error(`[Companies] erro ao criar:`, e)
    }
  }

  async function handleDeleteCompany(id: number) {
    console.log(`[Companies] deletando empresa #${id}...`)
    try {
      await companiesService.delete(id)
      console.log(`[Companies] empresa #${id} deletada`)
      setCompanies((prev) => prev.filter((c) => c.id !== id))
    } catch (e) {
      console.error(`[Companies] erro ao deletar:`, e)
    }
  }

  const companyContacts = (clientId: number) =>
    allContacts.filter((c) => c.clientId === clientId)

  async function handleUnassignContact(contactId: number) {
    console.log(`[Companies] removendo contato #${contactId} da empresa...`)
    try {
      await companiesService.unassignContact(contactId)
      console.log(`[Companies] contato #${contactId} removido`)
      const updated = await contactsService.list()
      setAllContacts(updated)
    } catch (e) {
      console.error(`[Companies] erro ao remover contato:`, e)
    }
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
    createCompany: createNewCompany,
    deleteCompany: handleDeleteCompany,
    companyContacts,
    unassignContact: handleUnassignContact,
  }
}
