
import { useState, useEffect } from "react"
import { kanbanService, type UserResponse } from "../../../services/kanban.service"
import { chatsService, type ChatListResponse } from "../../../services/chats.service"
import { PRIORITY_LABELS } from "../../../constants"
import { Modal, ModalField } from "../../modal/modal.view"
import styles from "./kanban.module.css"

interface CreateModalProps {
  onClose: () => void
  onCreated: () => void
}

function chatLabel(chat: ChatListResponse): string {
  const name = chat.contactName ?? chat.name ?? chat.phoneNumber ?? `Chat #${chat.id}`
  return chat.clientName ? `${name} — ${chat.clientName}` : name
}

export function CreateOccurrenceModal({ onClose, onCreated }: CreateModalProps) {
  const [title, setTitle] = useState("")
  const [description, setDescription] = useState("")
  const [priority, setPriority] = useState<number>(1)
  const [chatId, setChatId] = useState<number | "">("")
  const [chats, setChats] = useState<ChatListResponse[]>([])
  const [assignedToUserId, setAssignedToUserId] = useState<number | "">("")
  const [users, setUsers] = useState<UserResponse[]>([])
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    kanbanService.listUsers().then(setUsers).catch(() => {})
    chatsService.listAllChats().then((res) => setChats(res.items)).catch(() => {})
  }, [])

  async function create() {
    if (!title.trim() || chatId === "") return
    setSaving(true)
    setError(null)
    try {
      await kanbanService.createOccurrence({
        title: title.trim(),
        description: description.trim() || undefined,
        priority,
        chatId: Number(chatId),
        assignedToUserId: assignedToUserId === "" ? undefined : Number(assignedToUserId),
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

      <ModalField label="Chat *">
        <select className={styles.select} value={chatId} onChange={(e) => setChatId(e.target.value === "" ? "" : Number(e.target.value))}>
          <option value="">Selecione um chat</option>
          {chats.map((c) => (
            <option key={c.id} value={c.id}>{chatLabel(c)}</option>
          ))}
        </select>
      </ModalField>

      <ModalField label="Prioridade">
        <select className={styles.select} value={priority} onChange={(e) => setPriority(Number(e.target.value))}>
          {(Object.entries(PRIORITY_LABELS) as [string, string][]).map(([value, label]) => (
            <option key={value} value={value}>{label}</option>
          ))}
        </select>
      </ModalField>

      <ModalField label="Responsável">
        <select className={styles.select} value={assignedToUserId} onChange={(e) => setAssignedToUserId(e.target.value === "" ? "" : Number(e.target.value))}>
          <option value="">Sem responsável</option>
          {users.map((u) => (
            <option key={u.id} value={u.id}>{u.name}</option>
          ))}
        </select>
      </ModalField>

      <div className={styles.modalActions}>
        <button className={styles.cancelBtn} onClick={onClose}>Cancelar</button>
        <button className={styles.saveBtn} onClick={create} disabled={saving || !title.trim() || chatId === ""}>
          {saving ? "Criando..." : "Criar Ocorrência"}
        </button>
      </div>
    </Modal>
  )
}
