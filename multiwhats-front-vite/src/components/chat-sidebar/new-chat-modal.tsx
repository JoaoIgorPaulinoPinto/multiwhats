
import { useState } from "react"
import { Modal, ModalField } from "../modal/modal.view"
import styles from "../modal/modal.module.css"

interface NewChatModalProps {
  onClose: () => void
  onStart: (phone: string, name: string) => void
}

export function NewChatModal({ onClose, onStart }: NewChatModalProps) {
  const [phone, setPhone] = useState("")
  const [name, setName] = useState("")

  function handleStart() {
    const cleanPhone = phone.replace(/\D/g, "")
    if (!cleanPhone || cleanPhone.length < 8) return
    onStart(cleanPhone, name || "Contato")
  }

  return (
    <Modal title="Novo Chat" onClose={onClose}>
      <ModalField label="Telefone *">
        <input
          value={phone}
          onChange={(e) => setPhone(e.target.value)}
          placeholder="Ex: 5515999999999"
          autoFocus
        />
      </ModalField>

      <ModalField label="Nome">
        <input
          value={name}
          onChange={(e) => setName(e.target.value)}
          placeholder="Nome do contato"
        />
      </ModalField>

      <div className={styles.modalActions}>
        <button className={styles.cancelBtn} onClick={onClose}>Cancelar</button>
        <button
          className={styles.saveBtn}
          onClick={handleStart}
          disabled={!phone.replace(/\D/g, "") || phone.replace(/\D/g, "").length < 8}
        >
          Iniciar Chat
        </button>
      </div>
    </Modal>
  )
}
