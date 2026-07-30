import { X } from "lucide-react"
import styles from "./modal.module.css"

interface ModalProps {
  title: string
  onClose: () => void
  children: React.ReactNode
  actions?: React.ReactNode
  error?: string | null
}

export function Modal({ title, onClose, children, actions, error }: ModalProps) {
  return (
    <>
      <div className={styles.overlay} onClick={onClose} />
      <div className={styles.modal}>
        <div className={styles.modalHeader}>
          <h3>{title}</h3>
          <button className={styles.closeBtn} onClick={onClose}>
            <X size={18} />
          </button>
        </div>

        {children}

        {error && <div className={styles.error}>{error}</div>}

        {actions && (
          <div className={styles.modalActions}>
            {actions}
          </div>
        )}
      </div>
    </>
  )
}

export function ModalField({ label, children }: { label: string; children: React.ReactNode }) {
  return (
    <div className={styles.field}>
      <label>{label}</label>
      {children}
    </div>
  )
}

export function ModalActions({ onCancel, onSaveText, onSave, saving, disabled }: {
  onCancel: () => void
  onSaveText: string
  savingText?: string
  onSave: () => void
  saving?: boolean
  disabled?: boolean
}) {
  return (
    <div className={styles.modalActions}>
      <button className={styles.cancelBtn} onClick={onCancel} disabled={saving}>Cancelar</button>
      <button className={styles.saveBtn} onClick={onSave} disabled={saving || disabled}>
        {saving ? "Salvando..." : onSaveText}
      </button>
    </div>
  )
}
