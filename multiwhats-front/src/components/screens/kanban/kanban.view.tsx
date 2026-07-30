"use client"

import { ChevronRight, Plus } from "lucide-react"
import { useState } from "react"
import type { OccurrenceResponse, Priority } from "../../../services/kanban.service"
import { useKanban } from "./kanban.logic"
import { OccurrenceDetailModal } from "./occurrence-detail-modal"
import { CreateOccurrenceModal } from "./create-occurrence-modal"
import { OCCURRENCE_STATUS_OPTIONS, PRIORITY_COLORS, PRIORITY_LABELS } from "../../../constants"
import styles from "./kanban.module.css"

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
                        style={{ background: PRIORITY_COLORS[card.priority as Priority] ?? "#6b7280" }}
                        title={PRIORITY_LABELS[card.priority as Priority] ?? card.priority}
                      />
                    </div>
                    <span className={styles.assignee}>{card.subtitle}</span>
                    <div className={styles.cardFooter}>
                      <span className={styles.badge}>{card.type === "task" ? "Tarefa" : "Ocorrência"}</span>
                      {card.type === "occurrence" && (
                        <div className={styles.statusActions} onClick={(e) => e.stopPropagation()}>
                          {OCCURRENCE_STATUS_OPTIONS.find((s) => s.value === card.status)?.next && (
                            <button
                              className={styles.statusBtn}
                              onClick={() => {
                                const next = OCCURRENCE_STATUS_OPTIONS.find((s) => s.value === card.status)?.next
                                if (next) changeOccurrenceStatus(card.id, next)
                              }}
                              title={OCCURRENCE_STATUS_OPTIONS.find((s) => s.value === card.status)?.nextLabel}
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
