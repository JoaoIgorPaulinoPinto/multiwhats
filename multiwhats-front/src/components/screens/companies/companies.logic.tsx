"use client"

import { useCallback, useEffect, useState } from "react"
import { companiesService, type ClientResponse } from "../../../services/companies.service"
import { contactsService, type ContactResponse } from "../../../services/contacts.service"
import { useToast } from "../../../components/toast/toast.provider"

export function useCompanies() {
  const { toast } = useToast()
  const [companies, setCompanies] = useState<ClientResponse[]>([])
  const [allContacts, setAllContacts] = useState<ContactResponse[]>([])
  const [editing, setEditing] = useState<ClientResponse | null>(null)
  const [formName, setFormName] = useState("")
  const [formPhone, setFormPhone] = useState("")
  const [formStatus, setFormStatus] = useState("Active")
  const [search, setSearch] = useState("")
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [deleting, setDeleting] = useState<number | null>(null)

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
    setSaving(true)
    console.log(`[Companies] salvando empresa #${editing.id}...`)
    try {
      await companiesService.update(editing.id, {
        name: formName,
        mainPhoneNumber: formPhone || null,
        status: formStatus,
      })
      console.log(`[Companies] empresa #${editing.id} atualizada`)
      toast.success("Empresa atualizada com sucesso")
      await loadData()
      cancelEdit()
    } catch (e) {
      console.error(`[Companies] erro ao salvar:`, e)
      toast.error("Erro ao salvar empresa")
    } finally {
      setSaving(false)
    }
  }

  async function createNewCompany() {
    setSaving(true)
    console.log(`[Companies] criando empresa...`)
    try {
      await companiesService.create({ name: formName, mainPhoneNumber: formPhone || null })
      console.log(`[Companies] empresa criada`)
      toast.success("Empresa criada com sucesso")
      await loadData()
      cancelEdit()
    } catch (e) {
      console.error(`[Companies] erro ao criar:`, e)
      toast.error("Erro ao criar empresa")
    } finally {
      setSaving(false)
    }
  }

  async function handleDeleteCompany(id: number) {
    if (!window.confirm("Tem certeza que deseja excluir esta empresa?")) return
    setDeleting(id)
    console.log(`[Companies] deletando empresa #${id}...`)
    try {
      await companiesService.delete(id)
      console.log(`[Companies] empresa #${id} deletada`)
      setCompanies((prev) => prev.filter((c) => c.id !== id))
      toast.success("Empresa excluída")
    } catch (e) {
      console.error(`[Companies] erro ao deletar:`, e)
      toast.error("Erro ao excluir empresa")
    } finally {
      setDeleting(null)
    }
  }

  const companyContacts = (clientId: number) =>
    allContacts.filter((c) => c.clientId === clientId)

  async function handleUnassignContact(contactId: number) {
    console.log(`[Companies] removendo contato #${contactId} da empresa...`)
    try {
      await companiesService.unassignContact(contactId)
      console.log(`[Companies] contato #${contactId} removido`)
      toast.success("Contato desvinculado")
      const updated = await contactsService.list()
      setAllContacts(updated)
    } catch (e) {
      console.error(`[Companies] erro ao remover contato:`, e)
      toast.error("Erro ao desvincular contato")
    }
  }

  return {
    companies: filtered,
    allContacts,
    loading,
    saving,
    deleting,
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
