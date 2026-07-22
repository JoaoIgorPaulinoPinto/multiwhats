"use client"

import { Search } from "lucide-react"
import { AvatarView } from "../avatar/avatar.view"
import { useChatSidebar } from "./chat-sidebar.logic"
import styles from "./chat-sidebar.module.css"

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
        ) : chats.map((chat) => (
          <div
            key={chat.id}
            className={`${styles.chatItem} ${selectedId === chat.id ? styles.active : ""}`}
            onClick={() => onSelect(chat.id, chat.contactName ?? chat.phoneNumber ?? `Chat #${chat.id}`, chat.phoneNumber ?? "", chat.jid, chat.contactId, chat.lastMessageBody ?? "", chat.lastMessageAt)}
          >
            <AvatarView name={chat.contactName ?? chat.name ?? chat.phoneNumber ?? "?"} size={42} />
            <div className={styles.chatInfo}>
              <div className={styles.chatTop}>
                <strong>{chat.contactName ?? chat.name ?? chat.phoneNumber ?? `Chat #${chat.id}`}</strong>
                <label>{chat.phoneNumber ?? ""}</label>
              </div>
              {chat.lastMessageBody && <p className={styles.lastMsg}>{chat.lastMessageBody}</p>}
            </div>
          </div>
        ))}
      </section>
    </aside>
  )
}
