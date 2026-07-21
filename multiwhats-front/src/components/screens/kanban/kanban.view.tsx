"use client"

import { Plus } from "lucide-react"
import { useKanban } from "./kanban.logic"
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
  const { columns, loading } = useKanban()

  return (
    <div className={styles.page}>
      <header className={styles.header}>
        <h2>Kanban</h2>
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
                  <div key={`${card.type}-${card.id}`} className={`${styles.card} ${card.type === "occurrence" ? styles.occurrence : ""}`}>
                    <p>{card.title}</p>
                    <span className={styles.assignee}>{card.subtitle}</span>
                    <span className={styles.badge}>{card.type === "task" ? "Tarefa" : "Ocorrência"}</span>
                  </div>
                ))}
              </div>

              <button className={styles.addBtn}>
                <Plus size={16} />
                Adicionar
              </button>
            </div>
          ))
        )}
      </section>
    </div>
  )
}
