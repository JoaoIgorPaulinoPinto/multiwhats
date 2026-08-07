import { Combine, Headset, X } from 'lucide-react'
import { useCallback, useEffect, useState } from 'react'
import { useToast } from '../../components/toast/toast.provider'
import { OCCURRENCE_STATUS_LABELS, PRIORITY_LABELS } from '../../constants'
import {
  chatsService,
  type ChatFullInfoResponse,
  type ChatHistoryResponse,
  type ChatListResponse,
} from '../../services/chats.service'
import { useAuthStore } from '../../stores/auth-store'
import { useUnreadStore } from '../../stores/unread-store'
import type { OccurrenceStatus, Priority } from '../../types'
import {
  formatDateTime,
  formatDuration,
  formatRelativeTime,
  formatRelativeToNow,
} from '../../utils/date-format'
import { Modal } from '../modal/modal.view'
import styles from './chat-info-panel.module.css'
import { MergeChatModal } from './merge-chat-modal'

interface Props {
  chatId: number
  contactName: string
  phoneNumber: string
  jid: string
  onClose: () => void
  onMerged: (result: {
    survivorJid: string
    survivor?: ChatListResponse
  }) => void
}

const STATUS_MAP: Record<number, OccurrenceStatus> = {
  0: 'Open',
  1: 'InProgress',
  2: 'Resolved',
  3: 'Closed',
}

const TIMELINE_DOT_COLORS: Record<string, string> = {
  AtendimentoIniciado: '#16a34a',
  AtendimentoFinalizado: '#6b7280',
  OcorrenciaCriada: '#2563eb',
  OcorrenciaAtualizada: '#d97706',
  OcorrenciaExcluida: '#dc2626',
}

export function ChatInfoPanel({
  chatId,
  contactName,
  phoneNumber,
  jid,
  onClose,
  onMerged,
}: Props) {
  const { toast } = useToast()
  const currentUserId = useAuthStore((s) => s.user?.id)
  const unreadCount = useUnreadStore((s) => s.perChat[chatId] ?? 0)
  const [info, setInfo] = useState<ChatFullInfoResponse | null>(null)
  const [history, setHistory] = useState<ChatHistoryResponse | null>(null)
  const [loading, setLoading] = useState(true)
  const [historyLoading, setHistoryLoading] = useState(true)
  const [showMerge, setShowMerge] = useState(false)
  const [showAssignConfirm, setShowAssignConfirm] = useState(false)
  const [assigning, setAssigning] = useState(false)

  const load = useCallback(() => {
    Promise.all([
      chatsService.getFullInfo(chatId),
      chatsService.getHistory(chatId),
    ])
      .then(([infoData, historyData]) => {
        setInfo(infoData)
        setHistory(historyData)
      })
      .catch(() => toast.error('Erro ao carregar informações do chat'))
      .finally(() => {
        setLoading(false)
        setHistoryLoading(false)
      })
  }, [chatId, toast])

  useEffect(() => {
    load()
  }, [load])

  const name = info?.contactName ?? info?.name ?? contactName
  const phone = info?.phoneNumber ?? phoneNumber
  const statusLabel = (status: number) =>
    OCCURRENCE_STATUS_LABELS[STATUS_MAP[status] ?? 'Open']
  const priorityLabel = (priority: number) =>
    PRIORITY_LABELS[priority as Priority] ?? String(priority)

  const isAvailableToAttend =
    (unreadCount > 0 && info?.assignedToUserId == null) ||
    (info?.assignedToUserId != null && info.assignedToUserId !== currentUserId)

  async function handleAssign() {
    setAssigning(true)
    try {
      await chatsService.assignChat(chatId)
      toast.success('Atendimento iniciado com sucesso.')
      setShowAssignConfirm(false)
      load()
    } catch (e) {
      const message =
        e instanceof Error ? e.message : 'Erro ao iniciar atendimento'
      toast.error(message)
    } finally {
      setAssigning(false)
    }
  }

  return (
    <div className={styles.overlay} onClick={onClose}>
      <aside className={styles.panel} onClick={(e) => e.stopPropagation()}>
        <header className={styles.panelHeader}>
          <h3 className={styles.panelTitle}>Informações do Chat</h3>
          <button className={styles.closeBtn} onClick={onClose}>
            <X size={16} />
          </button>
        </header>

        <div className={styles.panelBody}>
          {loading ? (
            <p className={styles.emptyText}>
              <span className="spinner spinnerDark" /> Carregando...
            </p>
          ) : (
            <>
              <section className={styles.section}>
                <h4 className={styles.sectionTitle}>Contato</h4>
                <div className={styles.infoRow}>
                  <span className={styles.label}>Nome</span>
                  <span className={styles.value}>{name}</span>
                </div>
                <div className={styles.infoRow}>
                  <span className={styles.label}>Telefone</span>
                  <span className={styles.value}>{phone}</span>
                </div>
                <div className={styles.infoRow}>
                  <span className={styles.label}>JID</span>
                  <span className={styles.value}>{info?.jid ?? jid}</span>
                </div>
              </section>

              <section className={styles.section}>
                <h4 className={styles.sectionTitle}>Empresa</h4>
                <div className={styles.infoRow}>
                  <span className={styles.label}>Nome</span>
                  <span className={styles.value}>
                    {info?.clientName ?? '—'}
                  </span>
                </div>
                <div className={styles.infoRow}>
                  <span className={styles.label}>Telefone</span>
                  <span className={styles.value}>
                    {info?.clientMainPhoneNumber ?? '—'}
                  </span>
                </div>
              </section>

              <section className={styles.section}>
                <h4 className={styles.sectionTitle}>Atendimento</h4>
                <div className={styles.infoRow}>
                  <span className={styles.label}>Responsável</span>
                  <span className={styles.value}>
                    {info?.assignedToUserName ?? '—'}
                  </span>
                </div>
              </section>

              <section className={styles.section}>
                <h4 className={styles.sectionTitle}>Estatísticas</h4>
                <div className={styles.statsGrid}>
                  <div className={styles.statCard}>
                    <strong className={styles.statValue}>
                      {info?.outgoingMessageCount ?? 0}
                    </strong>
                    <span className={styles.statLabel}>Enviadas</span>
                  </div>
                  <div className={styles.statCard}>
                    <strong className={styles.statValue}>
                      {info?.mediaSentCount ?? 0}
                    </strong>
                    <span className={styles.statLabel}>Mídias enviadas</span>
                  </div>
                  <div className={styles.statCard}>
                    <strong className={styles.statValue}>
                      {info?.occurrenceCount ?? 0}
                    </strong>
                    <span className={styles.statLabel}>Ocorrências</span>
                  </div>
                  <div className={styles.statCard}>
                    <strong className={styles.statValue}>
                      {info?.timeSinceLastOccurrenceSeconds != null
                        ? formatRelativeTime(
                            info.timeSinceLastOccurrenceSeconds,
                          )
                        : '—'}
                    </strong>
                    <span className={styles.statLabel}>Última ocorrência</span>
                  </div>
                  <div className={styles.statCard}>
                    <strong className={styles.statValue}>
                      {info?.daysActive ?? 0}
                    </strong>
                    <span className={styles.statLabel}>Dias ativo</span>
                  </div>
                  <div className={styles.statCard}>
                    <strong className={styles.statValue}>
                      {info?.lastMessageAt
                        ? formatRelativeToNow(info.lastMessageAt)
                        : '—'}
                    </strong>
                    <span className={styles.statLabel}>Última interação</span>
                  </div>
                </div>
              </section>

              <section className={styles.section}>
                <h4 className={styles.sectionTitle}>Ocorrências</h4>
                {!info || info.occurrences.length === 0 ? (
                  <p className={styles.emptyText}>Nenhuma ocorrência</p>
                ) : (
                  <div className={styles.occList}>
                    {info.occurrences.map((occ) => (
                      <div key={occ.id} className={styles.occCard}>
                        <div className={styles.occTop}>
                          <span className={styles.occTitle}>{occ.title}</span>
                          <span
                            className={`${styles.occStatus} ${occ.priority >= 2 ? styles.highPrio : ''}`}
                          >
                            {statusLabel(occ.status)}
                          </span>
                        </div>
                        <span className={styles.occPriority}>
                          Prioridade: {priorityLabel(occ.priority)}
                          {occ.assignedToName ? ` · ${occ.assignedToName}` : ''}
                        </span>
                      </div>
                    ))}
                  </div>
                )}
              </section>

              <section className={styles.section}>
                <h4 className={styles.sectionTitle}>Histórico</h4>
                {historyLoading ? (
                  <p className={styles.emptyText}>
                    <span className="spinner spinnerDark" /> Carregando...
                  </p>
                ) : !history ? (
                  <p className={styles.emptyText}>Nenhum histórico</p>
                ) : (
                  <>
                    {history.atendimentos.length > 0 && (
                      <div className={styles.historySubBlock}>
                        <h5 className={styles.historySubTitle}>Atendimentos</h5>
                        <div className={styles.historyAtendList}>
                          {history.atendimentos.map((atendimento) => (
                            <div
                              key={atendimento.id}
                              className={styles.historyAtendCard}
                            >
                              <div className={styles.historyAtendRow}>
                                <span className={styles.historyAtendLabel}>
                                  Início
                                </span>
                                <span className={styles.historyAtendValue}>
                                  {atendimento.startedByName ??
                                    'Usuário removido'}{' '}
                                  · {formatDateTime(atendimento.startedAt)}
                                </span>
                              </div>
                              <div className={styles.historyAtendRow}>
                                <span className={styles.historyAtendLabel}>
                                  Fim
                                </span>
                                <span className={styles.historyAtendValue}>
                                  {atendimento.isOpen
                                    ? 'Em andamento'
                                    : `${atendimento.endedByName ?? 'Usuário removido'} · ${formatDateTime(atendimento.endedAt ?? '')}`}
                                </span>
                              </div>
                              {!atendimento.isOpen &&
                                atendimento.durationSeconds != null && (
                                  <div className={styles.historyAtendRow}>
                                    <span className={styles.historyAtendLabel}>
                                      Duração
                                    </span>
                                    <span className={styles.historyAtendValue}>
                                      {formatDuration(
                                        atendimento.durationSeconds,
                                      )}
                                    </span>
                                  </div>
                                )}
                            </div>
                          ))}
                        </div>
                      </div>
                    )}

                    <h5 className={styles.historySubTitle}>Linha do tempo</h5>
                    <div className={styles.historySubBlock}>
                      {history.timeline.length === 0 ? (
                        <p className={styles.emptyText}>
                          Nenhum evento registrado
                        </p>
                      ) : (
                        <div className={styles.historyList}>
                          {history.timeline.map((item, index) => (
                            <div key={index} className={styles.historyItem}>
                              <span
                                className={styles.historyDot}
                                style={{
                                  background:
                                    TIMELINE_DOT_COLORS[item.type] ??
                                    'var(--text-muted)',
                                }}
                              />
                              <div className={styles.historyContent}>
                                <span className={styles.historyTitle}>
                                  {item.title}
                                </span>
                                <span className={styles.historyDesc}>
                                  {item.description}
                                </span>
                                <span className={styles.historyMeta}>
                                  {formatDateTime(item.timestamp)}
                                </span>
                              </div>
                            </div>
                          ))}
                        </div>
                      )}
                    </div>
                  </>
                )}
              </section>

              <section className={styles.section}>
                <h4 className={styles.sectionTitle}>Ações</h4>
                <button
                  className={styles.mergeBtn}
                  onClick={() => setShowMerge(true)}
                >
                  <Combine size={14} />
                  Mesclar conversa
                </button>
                {isAvailableToAttend && (
                  <button
                    className={styles.assignBtn}
                    onClick={() => setShowAssignConfirm(true)}
                  >
                    <Headset size={14} />
                    Realizar Atendimento
                  </button>
                )}
              </section>
            </>
          )}
        </div>
      </aside>

      {showMerge && (
        <div onClick={(e) => e.stopPropagation()}>
          <MergeChatModal
            current={{ id: chatId, name, jid: info?.jid ?? jid }}
            onClose={() => setShowMerge(false)}
            onMerged={(result) => {
              setShowMerge(false)
              onMerged(result)
            }}
          />
        </div>
      )}

      {showAssignConfirm && (
        <div onClick={(e) => e.stopPropagation()}>
          <Modal
            title="Realizar Atendimento"
            onClose={() => setShowAssignConfirm(false)}
          >
            <p className={styles.confirmText}>
              Você está prestes a realizar o atendimento deste chat.
            </p>
            <p className={styles.confirmText}>
              {info?.assignedToUserId != null ? (
                <>
                  Este atendimento será{' '}
                  <strong>
                    desvinculado do usuário que o estiver realizando no momento
                  </strong>{' '}
                  e vinculado a você.
                </>
              ) : (
                <>
                  O chat será <strong>vinculado a você</strong> como responsável
                  pelo atendimento.
                </>
              )}
            </p>
            <div className={styles.assignActions}>
              <button
                className={styles.cancelBtn}
                onClick={() => setShowAssignConfirm(false)}
                disabled={assigning}
              >
                Cancelar
              </button>
              <button
                className={styles.assignConfirmBtn}
                onClick={handleAssign}
                disabled={assigning}
              >
                {assigning ? 'Realizando...' : 'Confirmar'}
              </button>
            </div>
          </Modal>
        </div>
      )}
    </div>
  )
}
