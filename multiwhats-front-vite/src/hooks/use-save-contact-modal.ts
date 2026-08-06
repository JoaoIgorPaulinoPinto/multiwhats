
import { useEffect, useState } from "react"
import { contactsService } from "../services/contacts.service"
import { companiesService, type ClientResponse } from "../services/companies.service"

export function useSaveContactModal(jid: string) {
  const [showModal, setShowModal] = useState(false)
  const [formJid, setFormJid] = useState("")
  const [formPhone, setFormPhone] = useState("")
  const [formName, setFormName] = useState("")
  const [formPushName, setFormPushName] = useState("")
  const [assignClientId, setAssignClientId] = useState<number | null>(null)
  const [clients, setClients] = useState<ClientResponse[]>([])
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    companiesService.list().then(setClients).catch(console.error)
  }, [])

  function openModal(phone: string, name: string) {
    setFormJid(jid)
    setFormPhone(phone)
    setFormName(name || "")
    setFormPushName("")
    setAssignClientId(null)
    setError(null)
    setShowModal(true)
  }

  function closeModal() {
    setShowModal(false)
    setFormJid("")
    setFormPhone("")
    setFormName("")
    setFormPushName("")
    setAssignClientId(null)
    setError(null)
  }

  async function createContact() {
    if (!formJid || !formPhone) return
    setSaving(true)
    setError(null)
    try {
      await contactsService.create({
        jid: formJid,
        phoneNumber: formPhone,
        name: formName || undefined,
        pushName: formPushName || undefined,
      })
      closeModal()
    } catch (e) {
      const message = e instanceof Error ? e.message : "Erro ao salvar contato"
      setError(message)
    } finally {
      setSaving(false)
    }
  }

  return {
    showModal,
    formJid,
    formPhone,
    formName,
    formPushName,
    assignClientId,
    clients,
    saving,
    error,
    setFormPhone,
    setFormName,
    setAssignClientId,
    openModal,
    closeModal,
    createContact,
  }
}
