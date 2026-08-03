'use client'

import styles from '../modal/modal.module.css'
import { Modal } from '../modal/modal.view'

interface AssignChatModalProps {
  chatName: string
  saving: boolean
  error?: string | null
  inline?: boolean
  onConfirm: () => void
  onCancel: () => void
}

export function AssignChatModal({
  chatName,
  saving,
  error,
  inline,
  onConfirm,
  onCancel,
}: AssignChatModalProps) {
  return (
    <Modal
      title="Atendimento"
      onClose={saving ? () => {} : onCancel}
      error={error}
      inline={inline}
    >
      <p className={styles.modalText}>
        O chat <strong>{chatName}</strong> não está em atendimento. Deseja
        realizar o atendimento?
      </p>
      <div className={styles.modalActions}>
        <button
          className={styles.cancelBtn}
          onClick={onCancel}
          disabled={saving}
        >
          Não
        </button>
        <button
          className={styles.saveBtn}
          onClick={onConfirm}
          disabled={saving}
        >
          {saving ? 'Atribuindo...' : 'Sim'}
        </button>
      </div>
    </Modal>
  )
}
