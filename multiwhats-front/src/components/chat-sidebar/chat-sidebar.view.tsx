"use client"

import { Search } from "lucide-react"
import { AvatarView } from "../avatar/avatar.view"
import { useChatSidebar } from "./chat-sidebar.logic"
import styles from "./chat-sidebar.module.css"

interface Props {
  selectedId: number | null
  onSelect: (id: number, name: string) => void
}

export function ChatSidebarView({ selectedId, onSelect }: Props) {
  const { search, setSearch, contacts } = useChatSidebar()

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
        {contacts.map((contact) => (
          <div
            key={contact.id}
            className={`${styles.chatItem} ${selectedId === contact.id ? styles.active : ""}`}
            onClick={() => onSelect(contact.id, contact.name ?? contact.pushName ?? contact.phoneNumber)}
          >
            {contact.profilePicUrl ? (
              <img src={contact.profilePicUrl} alt="" className={styles.avatarImg} />
            ) : (
              <AvatarView name={contact.name ?? contact.pushName ?? contact.phoneNumber} />
            )}
            <div className={styles.chatInfo}>
              <div className={styles.chatTop}>
                <strong>{contact.name ?? contact.pushName ?? contact.phoneNumber}</strong>
              </div>
              <p>{contact.phoneNumber}</p>
            </div>
          </div>
        ))}
      </section>
    </aside>
  )
}
