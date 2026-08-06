
import { useEffect, useState, useCallback } from "react"
import { Combine, X } from "lucide-react"
import styles from "./chat-info-panel.module.css"
import { chatsService, type ChatFullInfoResponse, type ChatListResponse } from "../../services/chats.service"
import { useToast } from "../../components/toast/toast.provider"
import { OCCURRENCE_STATUS_LABELS, PRIORITY_LABELS } from "../../constants"
import type { OccurrenceStatus, Priority } from "../../types"
import { formatRelativeTime, formatRelativeToNow } from "../../utils/date-format"
import { MergeChatModal } from "./merge-chat-modal"

interface Props {
  chatId: number
  contactName: string
  phoneNumber: string
  jid: string
  onClose: () => void
  onMerged: (result: { survivorJid: string; survivor?: ChatListResponse }) => void
}

const STATUS_MAP: Record<number, OccurrenceStatus> = {
  0: "Open",
  1: "InProgress",
  2: "Resolved",
  3: "Closed",
}

export function ChatInfoPanel({ chatId, contactName, phoneNumber, jid, onClose, onMerged }: Props) {
  const { toast } = useToast()
  const [info, setInfo] = useState<ChatFullInfoResponse | null>(null)
  const [loading, setLoading] = useState(true)
  const [showMerge, setShowMerge] = useState(false)

  const load = useCallback(() => {
    chatsService
      .getFullInfo(chatId)
      .then(setInfo)
      .catch(() => toast.error("Erro ao carregar informações do chat"))
      .finally(() => setLoading(false))
  }, [chatId, toast])

  useEffect(() => {
    load()
  }, [load])

  const name = info?.contactName ?? info?.name ?? contactName
  const phone = info?.phoneNumber ?? phoneNumber
  const statusLabel = (status: number) => OCCURRENCE_STATUS_LABELS[STATUS_MAP[status] ?? "Open"]
  const priorityLabel = (priority: number) => PRIORITY_LABELS[priority as Priority] ?? String(priority)

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
                  <span className={styles.value}>{info?.clientName ?? "—"}</span>
                </div>
                <div className={styles.infoRow}>
                  <span className={styles.label}>Telefone</span>
                  <span className={styles.value}>{info?.clientMainPhoneNumber ?? "—"}</span>
                </div>
              </section>

              <section className={styles.section}>
                <h4 className={styles.sectionTitle}>Atendimento</h4>
                <div className={styles.infoRow}>
                  <span className={styles.label}>Responsável</span>
                  <span className={styles.value}>
                    {info?.assignedToUserName ?? "—"}
                  </span>
                </div>
              </section>

              <section className={styles.section}>
                <h4 className={styles.sectionTitle}>Estatísticas</h4>
                <div className={styles.statsGrid}>
                  <div className={styles.statCard}>
                    <strong className={styles.statValue}>{info?.outgoingMessageCount ?? 0}</strong>
                    <span className={styles.statLabel}>Enviadas</span>
                  </div>
                  <div className={styles.statCard}>
                    <strong className={styles.statValue}>{info?.mediaSentCount ?? 0}</strong>
                    <span className={styles.statLabel}>Mídias enviadas</span>
                  </div>
                  <div className={styles.statCard}>
                    <strong className={styles.statValue}>{info?.occurrenceCount ?? 0}</strong>
                    <span className={styles.statLabel}>Ocorrências</span>
                  </div>
                  <div className={styles.statCard}>
                    <strong className={styles.statValue}>
                      {info?.timeSinceLastOccurrenceSeconds != null
                        ? formatRelativeTime(info.timeSinceLastOccurrenceSeconds)
                        : "—"}
                    </strong>
                    <span className={styles.statLabel}>Última ocorrência</span>
                  </div>
                  <div className={styles.statCard}>
                    <strong className={styles.statValue}>{info?.daysActive ?? 0}</strong>
                    <span className={styles.statLabel}>Dias ativo</span>
                  </div>
                  <div className={styles.statCard}>
                    <strong className={styles.statValue}>
                      {info?.lastMessageAt ? formatRelativeToNow(info.lastMessageAt) : "—"}
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
                          <span className={`${styles.occStatus} ${occ.priority >= 2 ? styles.highPrio : ""}`}>
                            {statusLabel(occ.status)}
                          </span>
                        </div>
                        <span className={styles.occPriority}>
                          Prioridade: {priorityLabel(occ.priority)}
                          {occ.assignedToName ? ` · ${occ.assignedToName}` : ""}
                        </span>
                      </div>
                    ))}
                  </div>
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
    </div>
  )
}
