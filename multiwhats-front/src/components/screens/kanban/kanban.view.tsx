'use client'

import {
  AlertCircle,
  ChevronLeft,
  ChevronRight,
  Clock,
  Plus,
  User,
  X,
} from 'lucide-react'
import { useEffect, useState } from 'react'
import {
  OCCURRENCE_STATUS_COLORS,
  OCCURRENCE_STATUS_OPTIONS,
  PRIORITY_COLORS,
  PRIORITY_LABELS,
} from '../../../constants'
import {
  kanbanService,
  type OccurrenceMetricsResponse,
} from '../../../services/kanban.service'
import { CreateOccurrenceModal } from './create-occurrence-modal'
import { useKanban, type KanbanCard } from './kanban.logic'
import styles from './kanban.module.css'
import { OccurrenceDetailModal } from './occurrence-detail-modal'

function timeAgo(dateStr: string): string {
  const diff = Date.now() - new Date(dateStr).getTime()
  const mins = Math.floor(diff / 60000)
  if (mins < 1) return 'agora'
  if (mins < 60) return `${mins}min`
  const hours = Math.floor(mins / 60)
  if (hours < 24) return `${hours}h`
  const days = Math.floor(hours / 24)
  if (days < 30) return `${days}d`
  return `${Math.floor(days / 30)}m`
}

function formatDate(dateStr: string): string {
  return new Date(dateStr).toLocaleDateString('pt-BR')
}

function SkeletonColumn() {
  return (
    <div className={styles.column}>
      <div className={styles.columnHeader}>
        <div className="skeleton" style={{ height: 14, width: 100 }} />
        <div
          className="skeleton"
          style={{ height: 20, width: 28, borderRadius: 10 }}
        />
      </div>
      <div className={styles.cards}>
        {Array.from({ length: 3 }).map((_, i) => (
          <div key={i} className={styles.skeletonCard}>
            <div
              className="skeleton"
              style={{ height: 13, width: '80%', marginBottom: 8 }}
            />
            <div
              className="skeleton"
              style={{ height: 11, width: '50%', marginBottom: 6 }}
            />
            <div
              className="skeleton"
              style={{ height: 11, width: '40%', marginBottom: 6 }}
            />
            <div
              className="skeleton"
              style={{ height: 18, width: 60, borderRadius: 6 }}
            />
          </div>
        ))}
      </div>
    </div>
  )
}

function CardActions({
  card,
  onAdvance,
}: {
  card: KanbanCard
  onAdvance: (id: number, dir: 'Advance' | 'Return') => void
}) {
  const opt = OCCURRENCE_STATUS_OPTIONS.find((s) => s.value === card.status)
  return (
    <div className={styles.cardActions}>
      {opt?.prev && (
        <button
          className={styles.actionBtn}
          onClick={(e) => {
            e.stopPropagation()
            onAdvance(card.id, 'Return')
          }}
          title={opt.prevLabel}
        >
          <ChevronLeft size={14} />
        </button>
      )}
      {opt?.next && (
        <button
          className={styles.actionBtn}
          onClick={(e) => {
            e.stopPropagation()
            onAdvance(card.id, 'Advance')
          }}
          title={opt.nextLabel}
        >
          <ChevronRight size={14} />
        </button>
      )}
    </div>
  )
}

function OccurrenceCard({
  card,
  onClick,
  onAdvance,
}: {
  card: KanbanCard
  onClick: () => void
  onAdvance: (id: number, dir: 'Advance' | 'Return') => void
}) {
  return (
    <div
      className={`${styles.card} ${styles.occurrenceCard}`}
      style={{ borderLeftColor: OCCURRENCE_STATUS_COLORS[card.status] }}
      onClick={onClick}
    >
      <div className={styles.cardHeader}>
        <p className={styles.cardTitle}>{card.title}</p>
        <span
          className={styles.priorityDot}
          style={{
            background:
              PRIORITY_COLORS[card.priority as keyof typeof PRIORITY_COLORS] ??
              '#6b7280',
          }}
          title={
            PRIORITY_LABELS[card.priority as keyof typeof PRIORITY_LABELS] ??
            String(card.priority)
          }
        />
      </div>

      {card.description && (
        <p className={styles.cardDescription}>
          {card.description.length > 80
            ? card.description.slice(0, 80) + '…'
            : card.description}
        </p>
      )}

      <div className={styles.cardMeta}>
        <span className={styles.metaItem}>
          <Clock size={11} />
          {formatDate(card.createdAt)} ({timeAgo(card.createdAt)})
        </span>
      </div>

      <div className={styles.cardFooter}>
        <span className={styles.assignee}>
          <User size={11} />
          {card.assignedToName ?? card.createdByName ?? 'Sem responsável'}
        </span>
        <CardActions card={card} onAdvance={onAdvance} />
      </div>
    </div>
  )
}

function ClosedMetricsModal({ onClose }: { onClose: () => void }) {
  const [metrics, setMetrics] = useState<OccurrenceMetricsResponse | null>(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    kanbanService
      .getOccurrenceMetrics()
      .then(setMetrics)
      .catch(console.error)
      .finally(() => setLoading(false))
  }, [])

  return (
    <>
      <div className={styles.overlay} onClick={onClose} />
      <div className={`${styles.modal} ${styles.metricsModal}`}>
        <div className={styles.modalHeader}>
          <h3>Métricas de Ocorrências Fechadas</h3>
          <button className={styles.closeBtn} onClick={onClose}>
            <X size={18} />
          </button>
        </div>

        {loading ? (
          <div className={styles.metricsLoading}>
            <span className="spinner spinnerDark" />
            Carregando métricas...
          </div>
        ) : metrics ? (
          <div className={styles.metricsContent}>
            <div className={styles.metricsGrid}>
              <div className={styles.metricCard}>
                <strong className={styles.metricValue}>
                  {metrics.totalClosed}
                </strong>
                <span className={styles.metricLabel}>Total fechadas</span>
              </div>
              <div className={styles.metricCard}>
                <strong className={styles.metricValue}>
                  {metrics.averageResolutionHours}h
                </strong>
                <span className={styles.metricLabel}>Tempo médio (horas)</span>
              </div>
            </div>

            <div className={styles.metricsSection}>
              <h4>Ocorrências por dia</h4>
              <div className={styles.perDayList}>
                {metrics.occurrencesPerDay.map((d) => (
                  <div key={d.date} className={styles.perDayRow}>
                    <span>
                      {new Date(d.date + 'T00:00:00').toLocaleDateString(
                        'pt-BR',
                      )}
                    </span>
                    <span className={styles.perDayCount}>{d.count}</span>
                  </div>
                ))}
              </div>
            </div>

            <div className={styles.metricsSection}>
              <h4>Por usuário</h4>
              <div className={styles.userTable}>
                <div className={styles.userTableHeader}>
                  <span>Usuário</span>
                  <span>Abertas</span>
                  <span>Fechadas</span>
                </div>
                {metrics.perUser.map((u) => (
                  <div key={u.userId} className={styles.userTableRow}>
                    <span>{u.userName ?? `Usuário #${u.userId}`}</span>
                    <span>{u.opened}</span>
                    <span>{u.closed}</span>
                  </div>
                ))}
              </div>
            </div>
          </div>
        ) : (
          <p
            style={{
              color: 'var(--text-muted)',
              textAlign: 'center',
              padding: 20,
            }}
          >
            Erro ao carregar métricas.
          </p>
        )}
      </div>
    </>
  )
}

function ClosedOccurrencesModal({
  occurrences,
  onAdvance,
  onCardClick,
  onClose,
}: {
  occurrences: KanbanCard[]
  onAdvance: (id: number, dir: 'Advance' | 'Return') => void
  onCardClick: (card: KanbanCard) => void
  onClose: () => void
}) {
  return (
    <>
      <div className={styles.overlay} onClick={onClose} />
      <div className={`${styles.modal} ${styles.closedModal}`}>
        <div className={styles.modalHeader}>
          <h3>Ocorrências Fechadas ({occurrences.length})</h3>
          <button className={styles.closeBtn} onClick={onClose}>
            <X size={18} />
          </button>
        </div>
        <div className={styles.closedList}>
          {occurrences.map((card) => (
            <div
              key={card.id}
              className={styles.closedItem}
              onClick={() => onCardClick(card)}
            >
              <div className={styles.closedItemInfo}>
                <strong>{card.title}</strong>
                <span>
                  {card.description
                    ? card.description.length > 60
                      ? card.description.slice(0, 60) + '…'
                      : card.description
                    : 'Sem descrição'}
                </span>
                <small>
                  {formatDate(card.createdAt)} -{' '}
                  {card.assignedToName ?? 'Sem responsável'}
                </small>
              </div>
              <CardActions card={card} onAdvance={onAdvance} />
            </div>
          ))}
        </div>
      </div>
    </>
  )
}

export function KanbanView() {
  const {
    columns,
    loading,
    load,
    advanceStatus,
    deleteOccurrence,
    occurrences,
  } = useKanban()
  const [detailCard, setDetailCard] = useState<KanbanCard | null>(null)
  const [showCreate, setShowCreate] = useState(false)
  const [showClosedModal, setShowClosedModal] = useState(false)
  const [showMetrics, setShowMetrics] = useState(false)

  const closedCards = columns.find((c) => c.id === 'Closed')?.cards ?? []
  const visibleClosed = closedCards.slice(0, 10)
  const hasMoreClosed = closedCards.length > 10

  function handleAdvance(id: number, direction: 'Advance' | 'Return') {
    advanceStatus(id, direction).catch(() => {})
  }

  function handleCardClick(card: KanbanCard) {
    setDetailCard(card)
  }

  return (
    <div className={styles.page}>
      <header className={styles.header}>
        <h2>Ocorrências</h2>
        <div className={styles.headerActions}>
          {closedCards.length > 0 && (
            <button
              className={styles.metricsBtn}
              onClick={() => setShowMetrics(true)}
            >
              <AlertCircle size={14} />
              Métricas
            </button>
          )}
          <button
            className={styles.addOccBtn}
            onClick={() => setShowCreate(true)}
          >
            <Plus size={16} />
            Nova Ocorrência
          </button>
        </div>
      </header>

      <section className={styles.board}>
        {loading ? (
          <>
            <SkeletonColumn />
            <SkeletonColumn />
            <SkeletonColumn />
            <SkeletonColumn />
          </>
        ) : (
          columns.map((column) => {
            const isClosed = column.id === 'Closed'
            const cards = isClosed ? visibleClosed : column.cards

            return (
              <div key={column.id} className={styles.column}>
                <div className={styles.columnHeader}>
                  <h3>{column.title}</h3>
                  <span className={styles.count}>{column.cards.length}</span>
                </div>

                <div className={styles.cards}>
                  {cards.map((card) => (
                    <OccurrenceCard
                      key={card.id}
                      card={card}
                      onClick={() => handleCardClick(card)}
                      onAdvance={handleAdvance}
                    />
                  ))}
                  {cards.length === 0 && (
                    <p className={styles.emptyColumn}>Nenhuma ocorrência</p>
                  )}
                </div>

                {isClosed && hasMoreClosed && (
                  <button
                    className={styles.viewAllBtn}
                    onClick={() => setShowClosedModal(true)}
                  >
                    Ver todas ({closedCards.length})
                  </button>
                )}
              </div>
            )
          })
        )}
      </section>

      {detailCard && (
        <OccurrenceDetailModal
          occurrence={occurrences.find((o) => o.id === detailCard.id)!}
          onClose={() => setDetailCard(null)}
          onStatusChange={(id, status) => handleAdvance(id, 'Advance')}
          onDelete={deleteOccurrence}
        />
      )}

      {showCreate && (
        <CreateOccurrenceModal
          onClose={() => setShowCreate(false)}
          onCreated={load}
        />
      )}

      {showClosedModal && (
        <ClosedOccurrencesModal
          occurrences={closedCards}
          onAdvance={handleAdvance}
          onCardClick={handleCardClick}
          onClose={() => setShowClosedModal(false)}
        />
      )}

      {showMetrics && (
        <ClosedMetricsModal onClose={() => setShowMetrics(false)} />
      )}
    </div>
  )
}
