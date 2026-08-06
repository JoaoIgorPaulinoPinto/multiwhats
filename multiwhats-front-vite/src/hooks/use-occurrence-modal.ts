
import { useState } from "react"
import { kanbanService } from "../services/kanban.service"

export function useOccurrenceModal(chatId: number | null, onCreated?: () => void) {
  const [showModal, setShowModal] = useState(false)
  const [title, setTitle] = useState("")
  const [description, setDescription] = useState("")
  const [priority, setPriority] = useState<number>(1)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)

  function openModal() {
    setTitle("")
    setDescription("")
    setPriority(1)
    setError(null)
    setShowModal(true)
  }

  function closeModal() {
    setShowModal(false)
    setTitle("")
    setDescription("")
    setPriority(1)
    setError(null)
  }

  async function createOccurrence() {
    if (!chatId || !title.trim()) return
    setSaving(true)
    setError(null)
    try {
      await kanbanService.createOccurrence({
        title: title.trim(),
        description: description.trim() || undefined,
        priority,
        chatId,
      })
      closeModal()
      onCreated?.()
    } catch (e) {
      const message = e instanceof Error ? e.message : "Erro ao criar ocorrência"
      setError(message)
    } finally {
      setSaving(false)
    }
  }

  return {
    showModal,
    title,
    setTitle,
    description,
    setDescription,
    priority,
    setPriority,
    saving,
    error,
    openModal,
    closeModal,
    createOccurrence,
  }
}
