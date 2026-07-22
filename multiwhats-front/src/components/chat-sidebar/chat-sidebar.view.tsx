"use client"

import { AlertCircle, Search } from "lucide-react"
import { AvatarView } from "../avatar/avatar.view"
import { useChatSidebar } from "./chat-sidebar.logic"
import styles from "./chat-sidebar.module.css"
import type { OccurrenceStatus } from "../../services/chats.service"

interface Props {
  selectedId: number | null
  onSelect: (id: number, name: string, phoneNumber: string, jid: string, contactId: number | null, lastMessage: string, lastMessageAt: string | null) => void
}

function SkeletonChatItem() {
  return (
    <div className={styles.skeletonItem}>
      <div className="skeleton" style={{ width: 42, height: 42, borderRadius: "50%", flexShrink: 0 }} />
      <div style={{ flex: 1, display: "flex", flexDirection: "column", gap: 6, minWidth: 0 }}>
        <div className="skeleton" style={{ height: 14, width: "55%" }} />
        <div className="skeleton" style={{ height: 11, width: "75%" }} />
      </div>
    </div>
  )
}

function getOccurrenceStatusLabel(status: OccurrenceStatus): string {
  if (status === "Open" || status === 0) return "Aberta"
  if (status === "InProgress" || status === 1) return "Em andamento"
  if (status === "Resolved" || status === 2) return "Resolvida"
  if (status === "Closed" || status === 3) return "Fechada"
  return "Desconhecido"
}

function getOccurrenceStatusColor(status: OccurrenceStatus): string {
  if (status === "Open" || status === 0) return "#d97706"
  if (status === "InProgress" || status === 1) return "#2563eb"
  if (status === "Resolved" || status === 2) return "#16a34a"
  if (status === "Closed" || status === 3) return "#6b7280"
  return "#6b7280"
}

export function ChatSidebarView({ selectedId, onSelect }: Props) {
  const { search, setSearch, chats, loading } = useChatSidebar()

  return (
    <aside className={styles.sidebar}>
      <header className={styles.sidebarHeader}>
        <div className={styles.search}>
          <Search size={15} />
          <input
            placeholder="Pesquisar conversa"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
          />
        </div>
      </header>

      <section className={styles.chatList}>
        {loading ? (
          <>
            <SkeletonChatItem />
            <SkeletonChatItem />
            <SkeletonChatItem />
            <SkeletonChatItem />
            <SkeletonChatItem />
          </>
        ) : chats.map((chat) => {
          const phone = chat.phoneNumber ?? ""
          const jid = phone ? `${phone}@c.us` : ""
          const displayName = chat.contactName ?? chat.name ?? chat.phoneNumber ?? `Chat #${chat.id}`
          const occCount = chat.occurrences?.length ?? 0

          return (
            <div
              key={chat.id}
              className={`${styles.chatItem} ${selectedId === chat.id ? styles.active : ""}`}
              onClick={() => onSelect(chat.id, displayName, phone, jid, null, chat.lastMessageBody ?? "", chat.lastMessageAt)}
            >
              <AvatarView name={displayName} size={42} />
              <div className={styles.chatInfo}>
                <div className={styles.chatTop}>
                  <strong>{displayName}</strong>
                  {occCount > 0 && (
                    <span className={styles.occBadge}>
                      <AlertCircle size={11} />
                      {occCount}
                    </span>
                  )}
                  <label>{phone}</label>
                </div>
                {chat.clientName && (
                  <span className={styles.clientName}>{chat.clientName}</span>
                )}
                {chat.occurrences && chat.occurrences.length > 0 && (
                  <div className={styles.occurrences}>
                    {chat.occurrences.slice(0, 2).map((occ) => (
                      <span
                        key={occ.id}
                        className={styles.occItem}
                        style={{ borderLeftColor: getOccurrenceStatusColor(occ.status) }}
                      >
                        <span className={styles.occTitle}>{occ.title}</span>
                        <span className={styles.occStatus} style={{ color: getOccurrenceStatusColor(occ.status) }}>
                          {getOccurrenceStatusLabel(occ.status)}
                        </span>
                      </span>
                    ))}
                    {chat.occurrences.length > 2 && (
                      <span className={styles.occMore}>+{chat.occurrences.length - 2} mais</span>
                    )}
                  </div>
                )}
                {chat.lastMessageBody && <p className={styles.lastMsg}>{chat.lastMessageBody}</p>}
              </div>
            </div>
          )
        })}
      </section>
    </aside>
  )
}
