
import { ChevronRight, Pencil, Trash2 } from 'lucide-react'
import { useState } from 'react'
import {
  OCCURRENCE_STATUS_OPTIONS,
  PRIORITY_COLORS,
  PRIORITY_LABELS,
} from '../../../constants'
import {
  kanbanService,
  type OccurrenceResponse,
  type OccurrenceStatus,
} from '../../../services/kanban.service'
import { Modal, ModalField } from '../../modal/modal.view'
import styles from './kanban.module.css'

interface DetailModalProps {
  occurrence: OccurrenceResponse
  onClose: () => void
  onStatusChange: (id: number, status: OccurrenceStatus) => void
  onDelete: (id: number) => void
}

export function OccurrenceDetailModal({
  occurrence,
  onClose,
  onStatusChange,
  onDelete,
}: DetailModalProps) {
  const [editing, setEditing] = useState(false)
  const [title, setTitle] = useState(occurrence.title)
  const [description, setDescription] = useState(occurrence.description ?? '')
  const [priority, setPriority] = useState<number>(occurrence.priority)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const statusInfo = OCCURRENCE_STATUS_OPTIONS.find(
    (s) => s.value === occurrence.status,
  )

  async function save() {
    setSaving(true)
    setError(null)
    try {
      await kanbanService.updateOccurrence(occurrence.id, {
        title: title.trim(),
        description: description.trim() || undefined,
        priority,
      })
      onClose()
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Erro ao salvar')
    } finally {
      setSaving(false)
    }
  }

  async function handleDelete() {
    if (!confirm('Tem certeza que deseja excluir esta ocorrência?')) return
    onDelete(occurrence.id)
    onClose()
  }

  return (
    <Modal
      title={editing ? 'Editar Ocorrência' : 'Detalhes da Ocorrência'}
      onClose={onClose}
      error={error}
    >
      <div className={styles.detailGrid}>
        {editing ? (
          <>
            <ModalField label="Título">
              <input value={title} onChange={(e) => setTitle(e.target.value)} />
            </ModalField>
            <ModalField label="Descrição">
              <textarea
                value={description}
                onChange={(e) => setDescription(e.target.value)}
                rows={3}
                style={{ resize: 'vertical' }}
              />
            </ModalField>
            <ModalField label="Prioridade">
              <select
                className={styles.select}
                value={priority}
                onChange={(e) => setPriority(Number(e.target.value))}
              >
                {(Object.entries(PRIORITY_LABELS) as [string, string][]).map(
                  ([value, label]) => (
                    <option key={value} value={value}>
                      {label}
                    </option>
                  ),
                )}
              </select>
            </ModalField>
          </>
        ) : (
          <>
            <div className={styles.detailRow}>
              <span className={styles.detailLabel}>Status</span>
              <span className={styles.detailValue}>
                {statusInfo?.label ?? occurrence.status}
              </span>
            </div>
            <div className={styles.detailRow}>
              <span className={styles.detailLabel}>Prioridade</span>
              <span
                className={styles.detailValue}
                style={{ color: PRIORITY_COLORS[occurrence.priority] }}
              >
                {PRIORITY_LABELS[occurrence.priority] ?? occurrence.priority}
              </span>
            </div>
            {occurrence.chatName && (
              <div className={styles.detailRow}>
                <span className={styles.detailLabel}>Chat</span>
                <span className={styles.detailValue}>
                  {occurrence.chatName}
                </span>
              </div>
            )}
            {occurrence.assignedToName && (
              <div className={styles.detailRow}>
                <span className={styles.detailLabel}>Responsável</span>
                <span className={styles.detailValue}>
                  {occurrence.assignedToName}
                </span>
              </div>
            )}
            {occurrence.description && (
              <div className={styles.field}>
                <label>Descrição</label>
                <p className={styles.detailDesc}>{occurrence.description}</p>
              </div>
            )}
            <div className={styles.detailRow}>
              <span className={styles.detailLabel}>Mensagens</span>
              <span className={styles.detailValue}>
                {occurrence.messageCount}
              </span>
            </div>
            <div className={styles.detailRow}>
              <span className={styles.detailLabel}>Criado em</span>
              <span className={styles.detailValue}>
                {new Date(occurrence.createdAt).toLocaleDateString('pt-BR')}
              </span>
            </div>
          </>
        )}
      </div>

      <div className={styles.modalActions}>
        {editing ? (
          <>
            <button
              className={styles.cancelBtn}
              onClick={() => setEditing(false)}
            >
              Cancelar
            </button>
            <button
              className={styles.saveBtn}
              onClick={save}
              disabled={saving || !title.trim()}
            >
              {saving ? 'Salvando...' : 'Salvar'}
            </button>
          </>
        ) : (
          <>
            <button className={styles.deleteBtn} onClick={handleDelete}>
              <Trash2 size={14} />
              Excluir
            </button>
            <button className={styles.editBtn} onClick={() => setEditing(true)}>
              <Pencil size={14} />
              Editar
            </button>
            {statusInfo?.next && (
              <button
                className={styles.advanceBtn}
                onClick={() => {
                  onStatusChange(occurrence.id, statusInfo.next!)
                  onClose()
                }}
              >
                <ChevronRight size={14} />
                {statusInfo.nextLabel}
              </button>
            )}
          </>
        )}
      </div>
    </Modal>
  )
}
