"use client"

import { PRIORITY_LABELS } from "../../constants"
import { Modal, ModalField } from "../modal/modal.view"
import styles from "../modal/modal.module.css"

interface SaveContactModalProps {
  formJid: string
  formPhone: string
  formName: string
  formPushName: string
  assignClientId: number | null
  clients: { id: number; name: string }[]
  saving: boolean
  error: string | null
  setFormPhone: (v: string) => void
  setFormName: (v: string) => void
  setAssignClientId: (v: number | null) => void
  onClose: () => void
  onSave: () => void
}

export function SaveContactModal({
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
  onClose,
  onSave,
}: SaveContactModalProps) {
  return (
    <Modal title="Salvar em contatos" onClose={onClose} error={error}>
      <ModalField label="JID (WhatsApp ID)">
        <input value={formJid} readOnly className={styles.readOnly} />
      </ModalField>

      <ModalField label="Telefone">
        <input value={formPhone} onChange={(e) => setFormPhone(e.target.value)} />
      </ModalField>

      <ModalField label="Nome">
        <input value={formName} onChange={(e) => setFormName(e.target.value)} />
      </ModalField>

      <ModalField label="Push Name (WhatsApp)">
        <input value={formPushName} readOnly className={styles.readOnly} />
      </ModalField>

      <ModalField label="Empresa">
        <select
          className={styles.select}
          value={assignClientId ?? ""}
          onChange={(e) => setAssignClientId(e.target.value ? Number(e.target.value) : null)}
        >
          <option value="">Sem empresa</option>
          {clients.map((c) => (
            <option key={c.id} value={c.id}>{c.name}</option>
          ))}
        </select>
      </ModalField>

      <div className={styles.modalActions}>
        <button className={styles.cancelBtn} onClick={onClose}>Cancelar</button>
        <button className={styles.saveBtn} onClick={onSave} disabled={saving}>
          {saving ? "Salvando..." : "Salvar"}
        </button>
      </div>
    </Modal>
  )
}

interface OccurrenceModalProps {
  title: string
  setTitle: (v: string) => void
  description: string
  setDescription: (v: string) => void
  priority: number
  setPriority: (v: number) => void
  saving: boolean
  error: string | null
  onClose: () => void
  onSave: () => void
}

export function OccurrenceModal({
  title,
  setTitle,
  description,
  setDescription,
  priority,
  setPriority,
  saving,
  error,
  onClose,
  onSave,
}: OccurrenceModalProps) {
  return (
    <Modal title="Abrir Ocorrência" onClose={onClose} error={error}>
      <ModalField label="Título *">
        <input
          value={title}
          onChange={(e) => setTitle(e.target.value)}
          placeholder="Ex: Problema com boleto"
        />
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

      <ModalField label="Prioridade">
        <select
          className={styles.select}
          value={priority}
          onChange={(e) => setPriority(Number(e.target.value))}
        >
          {(Object.entries(PRIORITY_LABELS) as [string, string][]).map(([value, label]) => (
            <option key={value} value={value}>{label}</option>
          ))}
        </select>
      </ModalField>

      <div className={styles.modalActions}>
        <button className={styles.cancelBtn} onClick={onClose}>Cancelar</button>
        <button className={styles.saveBtn} onClick={onSave} disabled={saving || !title.trim()}>
          {saving ? "Criando..." : "Criar Ocorrência"}
        </button>
      </div>
    </Modal>
  )
}
