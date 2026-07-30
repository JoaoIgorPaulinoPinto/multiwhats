"use client"

import { useState } from "react"
import { kanbanService } from "../../../services/kanban.service"
import { PRIORITY_LABELS } from "../../../constants"
import { Modal, ModalField } from "../../modal/modal.view"
import styles from "./kanban.module.css"

interface CreateModalProps {
  onClose: () => void
  onCreated: () => void
}

export function CreateOccurrenceModal({ onClose, onCreated }: CreateModalProps) {
  const [title, setTitle] = useState("")
  const [description, setDescription] = useState("")
  const [priority, setPriority] = useState<number>(1)
  const [chatId, setChatId] = useState<string>("")
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)

  async function create() {
    if (!title.trim() || !chatId) return
    setSaving(true)
    setError(null)
    try {
      await kanbanService.createOccurrence({
        title: title.trim(),
        description: description.trim() || undefined,
        priority,
        chatId: Number(chatId),
      })
      onCreated()
      onClose()
    } catch (e) {
      setError(e instanceof Error ? e.message : "Erro ao criar ocorrência")
    } finally {
      setSaving(false)
    }
  }

  return (
    <Modal title="Nova Ocorrência" onClose={onClose} error={error}>
      <ModalField label="Título *">
        <input value={title} onChange={(e) => setTitle(e.target.value)} placeholder="Ex: Problema com boleto" />
      </ModalField>

      <ModalField label="Descrição">
        <textarea
          value={description}
          onChange={(e) => setDescription(e.target.value)}
          placeholder="Descreva o problema..."
          rows={3}
          style={{ resize: "vertical" }}
        />
      </ModalField>

      <ModalField label="Chat ID *">
        <input value={chatId} onChange={(e) => setChatId(e.target.value)} placeholder="ID do chat" type="number" />
      </ModalField>

      <ModalField label="Prioridade">
        <select className={styles.select} value={priority} onChange={(e) => setPriority(Number(e.target.value))}>
          {(Object.entries(PRIORITY_LABELS) as [string, string][]).map(([value, label]) => (
            <option key={value} value={value}>{label}</option>
          ))}
        </select>
      </ModalField>

      <div className={styles.modalActions}>
        <button className={styles.cancelBtn} onClick={onClose}>Cancelar</button>
        <button className={styles.saveBtn} onClick={create} disabled={saving || !title.trim() || !chatId}>
          {saving ? "Criando..." : "Criar Ocorrência"}
        </button>
      </div>
    </Modal>
  )
}
