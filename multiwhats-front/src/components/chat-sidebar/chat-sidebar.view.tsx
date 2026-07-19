"use client"

import { Search } from "lucide-react"
import { useChatSidebar } from "./chat-sidebar.logic"
import styles from "./chat-sidebar.module.css"

interface Props {
  selectedId: number | null
  onSelect: (id: number, name: string, phoneNumber: string) => void
}

export function ChatSidebarView({ selectedId, onSelect }: Props) {
  const { search, setSearch, chats } = useChatSidebar()

  return (
    <aside className={styles.sidebar}>
      <header className={styles.sidebarHeader}>
        <div className={styles.search}>
          <Search size={18} />
          <input
            placeholder="Pesquisar conversa"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
          />
        </div>
      </header>

      <section className={styles.chatList}>
        {chats.map((chat) => (
          <div
            key={chat.id}
            className={`${styles.chatItem} ${selectedId === chat.id ? styles.active : ""}`}
            onClick={() => onSelect(chat.id, chat.contactName ?? chat.phoneNumber ?? `Chat #${chat.id}`, chat.phoneNumber ?? "")}
          >
            <div className={styles.chatInfo}>
              <div className={styles.chatTop}>
                <strong>{chat.name ?? chat.phoneNumber ?? `Chat #${chat.id}`}</strong>
                <label>{ chat.phoneNumber ?? `Chat #${chat.id}`}</label>
              </div>
              {chat.lastMessageBody && <p className={styles.lastMsg}>{chat.lastMessageBody}</p>}
            </div>
          </div>
        ))}
      </section>
    </aside>
  )
}
