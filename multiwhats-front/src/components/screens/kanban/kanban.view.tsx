"use client"

import { AlertTriangle, Calendar, ChevronLeft, ChevronRight, MessageSquare, Pencil, Plus, Trash2, User, X } from "lucide-react"
import { useState } from "react"
import { kanbanService, type OccurrenceResponse, type OccurrenceStatus, type Priority } from "../../../services/kanban.service"
import { useKanban } from "./kanban.logic"
import styles from "./kanban.module.css"

const STATUS_OPTIONS: { value: OccurrenceStatus; label: string; next?: OccurrenceStatus; nextLabel?: string }[] = [
  { value: "Open", label: "A fazer", next: "InProgress", nextLabel: "Iniciar" },
  { value: "InProgress", label: "Em andamento", next: "Resolved", nextLabel: "Resolver" },
  { value: "Resolved", label: "Resolvido", next: "Closed", nextLabel: "Fechar" },
  { value: "Closed", label: "Fechado" },
]

const PRIORITY_COLORS: Record<number, string> = {
  0: "#6b7280",
  1: "#d97706",
  2: "#ea580c",
  3: "#dc2626",
}

const PRIORITY_LABELS: Record<number, string> = {
  0: "Baixa",
  1: "Média",
  2: "Alta",
  3: "Urgente",
}

function SkeletonColumn() {
  return (
    <div className={styles.column}>
      <div className={styles.columnHeader}>
        <div className="skeleton" style={{ height: 14, width: 100 }} />
        <div className="skeleton" style={{ height: 20, width: 28, borderRadius: 10 }} />
      </div>
      <div className={styles.cards}>
        {Array.from({ length: 3 }).map((_, i) => (
          <div key={i} className={styles.skeletonCard}>
            <div className="skeleton" style={{ height: 13, width: "80%", marginBottom: 8 }} />
            <div className="skeleton" style={{ height: 11, width: "50%", marginBottom: 6 }} />
            <div className="skeleton" style={{ height: 18, width: 60, borderRadius: 6 }} />
          </div>
        ))}
      </div>
    </div>
  )
}

interface DetailModalProps {
  occurrence: OccurrenceResponse
  onClose: () => void
  onStatusChange: (id: number, status: OccurrenceStatus) => void
  onDelete: (id: number) => void
}

function OccurrenceDetailModal({ occurrence, onClose, onStatusChange, onDelete }: DetailModalProps) {
  const [editing, setEditing] = useState(false)
  const [title, setTitle] = useState(occurrence.title)
  const [description, setDescription] = useState(occurrence.description ?? "")
  const [priority, setPriority] = useState<number>(occurrence.priority)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const statusInfo = STATUS_OPTIONS.find((s) => s.value === occurrence.status)

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
      setError(e instanceof Error ? e.message : "Erro ao salvar")
    } finally {
      setSaving(false)
    }
  }

  async function handleDelete() {
    if (!confirm("Tem certeza que deseja excluir esta ocorrência?")) return
    onDelete(occurrence.id)
    onClose()
  }

  return (
    <>
      <div className={styles.overlay} onClick={onClose} />
      <div className={styles.modal}>
        <div className={styles.modalHeader}>
          <h3>{editing ? "Editar Ocorrência" : "Detalhes da Ocorrência"}</h3>
          <button className={styles.closeBtn} onClick={onClose}>
            <X size={18} />
          </button>
        </div>

        <div className={styles.detailGrid}>
          {editing ? (
            <>
              <div className={styles.field}>
                <label>Título</label>
                <input value={title} onChange={(e) => setTitle(e.target.value)} />
              </div>
              <div className={styles.field}>
                <label>Descrição</label>
                <textarea
                  value={description}
                  onChange={(e) => setDescription(e.target.value)}
                  rows={3}
                  style={{ resize: "vertical" }}
                />
              </div>
              <div className={styles.field}>
                <label>Prioridade</label>
                <select className={styles.select} value={priority} onChange={(e) => setPriority(Number(e.target.value))}>
                  <option value={0}>Baixa</option>
                  <option value={1}>Média</option>
                  <option value={2}>Alta</option>
                  <option value={3}>Urgente</option>
                </select>
              </div>
            </>
          ) : (
            <>
              <div className={styles.detailRow}>
                <span className={styles.detailLabel}>Status</span>
                <span className={styles.detailValue}>{statusInfo?.label ?? occurrence.status}</span>
              </div>
              <div className={styles.detailRow}>
                <span className={styles.detailLabel}>Prioridade</span>
                <span className={styles.detailValue} style={{ color: PRIORITY_COLORS[occurrence.priority] }}>
                  {PRIORITY_LABELS[occurrence.priority] ?? occurrence.priority}
                </span>
              </div>
              {occurrence.chatName && (
                <div className={styles.detailRow}>
                  <span className={styles.detailLabel}>Chat</span>
                  <span className={styles.detailValue}>{occurrence.chatName}</span>
                </div>
              )}
              {occurrence.assignedToName && (
                <div className={styles.detailRow}>
                  <span className={styles.detailLabel}>Responsável</span>
                  <span className={styles.detailValue}>{occurrence.assignedToName}</span>
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
                <span className={styles.detailValue}>{occurrence.messageCount}</span>
              </div>
              <div className={styles.detailRow}>
                <span className={styles.detailLabel}>Criado em</span>
                <span className={styles.detailValue}>
                  {new Date(occurrence.createdAt).toLocaleDateString("pt-BR")}
                </span>
              </div>
            </>
          )}
        </div>

        {error && <div className={styles.error}>{error}</div>}

        <div className={styles.modalActions}>
          {editing ? (
            <>
              <button className={styles.cancelBtn} onClick={() => setEditing(false)}>Cancelar</button>
              <button className={styles.saveBtn} onClick={save} disabled={saving || !title.trim()}>
                {saving ? "Salvando..." : "Salvar"}
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
                  onClick={() => { onStatusChange(occurrence.id, statusInfo.next!); onClose() }}
                >
                  <ChevronRight size={14} />
                  {statusInfo.nextLabel}
                </button>
              )}
            </>
          )}
        </div>
      </div>
    </>
  )
}

interface CreateModalProps {
  onClose: () => void
  onCreated: () => void
}

function CreateOccurrenceModal({ onClose, onCreated }: CreateModalProps) {
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
    <>
      <div className={styles.overlay} onClick={onClose} />
      <div className={styles.modal}>
        <div className={styles.modalHeader}>
          <h3>Nova Ocorrência</h3>
          <button className={styles.closeBtn} onClick={onClose}>
            <X size={18} />
          </button>
        </div>

        <div className={styles.field}>
          <label>Título *</label>
          <input value={title} onChange={(e) => setTitle(e.target.value)} placeholder="Ex: Problema com boleto" />
        </div>

        <div className={styles.field}>
          <label>Descrição</label>
          <textarea
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            placeholder="Descreva o problema..."
            rows={3}
            style={{ resize: "vertical" }}
          />
        </div>

        <div className={styles.field}>
          <label>Chat ID *</label>
          <input value={chatId} onChange={(e) => setChatId(e.target.value)} placeholder="ID do chat" type="number" />
        </div>

        <div className={styles.field}>
          <label>Prioridade</label>
          <select className={styles.select} value={priority} onChange={(e) => setPriority(Number(e.target.value))}>
            <option value={0}>Baixa</option>
            <option value={1}>Média</option>
            <option value={2}>Alta</option>
            <option value={3}>Urgente</option>
          </select>
        </div>

        {error && <div className={styles.error}>{error}</div>}

        <div className={styles.modalActions}>
          <button className={styles.cancelBtn} onClick={onClose}>Cancelar</button>
          <button className={styles.saveBtn} onClick={create} disabled={saving || !title.trim() || !chatId}>
            {saving ? "Criando..." : "Criar Ocorrência"}
          </button>
        </div>
      </div>
    </>
  )
}

export function KanbanView() {
  const { columns, loading, load, changeOccurrenceStatus, deleteOccurrence, occurrences } = useKanban()
  const [detailOcc, setDetailOcc] = useState<OccurrenceResponse | null>(null)
  const [showCreate, setShowCreate] = useState(false)

  return (
    <div className={styles.page}>
      <header className={styles.header}>
        <h2>Kanban</h2>
        <button className={styles.addOccBtn} onClick={() => setShowCreate(true)}>
          <Plus size={16} />
          Nova Ocorrência
        </button>
      </header>

      <section className={styles.board}>
        {loading ? (
          <>
            <SkeletonColumn />
            <SkeletonColumn />
            <SkeletonColumn />
          </>
        ) : (
          columns.map((column) => (
            <div key={column.id} className={styles.column}>
              <div className={styles.columnHeader}>
                <h3>{column.title}</h3>
                <span className={styles.count}>{column.cards.length}</span>
              </div>

              <div className={styles.cards}>
                {column.cards.map((card) => (
                  <div
                    key={`${card.type}-${card.id}`}
                    className={`${styles.card} ${card.type === "occurrence" ? styles.occurrence : ""}`}
                    onClick={() => {
                      if (card.type === "occurrence") {
                        const full = occurrences.find((o) => o.id === card.id)
                        if (full) setDetailOcc(full)
                      }
                    }}
                  >
                    <div className={styles.cardHeader}>
                      <p>{card.title}</p>
                      <span
                        className={styles.priorityDot}
                        style={{ background: PRIORITY_COLORS[card.priority] ?? "#6b7280" }}
                        title={PRIORITY_LABELS[card.priority] ?? card.priority}
                      />
                    </div>
                    <span className={styles.assignee}>{card.subtitle}</span>
                    <div className={styles.cardFooter}>
                      <span className={styles.badge}>{card.type === "task" ? "Tarefa" : "Ocorrência"}</span>
                      {card.type === "occurrence" && (
                        <div className={styles.statusActions} onClick={(e) => e.stopPropagation()}>
                          {STATUS_OPTIONS.find((s) => s.value === card.status)?.next && (
                            <button
                              className={styles.statusBtn}
                              onClick={() => {
                                const next = STATUS_OPTIONS.find((s) => s.value === card.status)?.next
                                if (next) changeOccurrenceStatus(card.id, next)
                              }}
                              title={STATUS_OPTIONS.find((s) => s.value === card.status)?.nextLabel}
                            >
                              <ChevronRight size={12} />
                            </button>
                          )}
                        </div>
                      )}
                    </div>
                  </div>
                ))}
              </div>

              {column.id === "todo" && (
                <button className={styles.addBtn} onClick={() => setShowCreate(true)}>
                  <Plus size={16} />
                  Adicionar
                </button>
              )}
            </div>
          ))
        )}
      </section>

      {detailOcc && (
        <OccurrenceDetailModal
          occurrence={detailOcc}
          onClose={() => setDetailOcc(null)}
          onStatusChange={changeOccurrenceStatus}
          onDelete={deleteOccurrence}
        />
      )}

      {showCreate && (
        <CreateOccurrenceModal
          onClose={() => setShowCreate(false)}
          onCreated={load}
        />
      )}
    </div>
  )
}
