'use client'

import { AlertCircle, MessageSquarePlus, RefreshCw, Search } from 'lucide-react'
import { useState } from 'react'
import {
  OCCURRENCE_STATUS_COLORS,
  OCCURRENCE_STATUS_LABELS,
} from '../../constants'
import type { ChatListResponse } from '../../services/chats.service'
import { MESSAGE_TYPE_MAP } from '../../types'
import { formatTime } from '../../utils/date-format'
import { AvatarView } from '../avatar/avatar.view'
import type { ChatTypeFilter } from './chat-sidebar.logic'
import styles from './chat-sidebar.module.css'
import { NewChatModal } from './new-chat-modal'

interface Props {
  selectedId: number | null
  onSelect: (
    id: number,
    name: string,
    phoneNumber: string,
    jid: string,
    contactId: number | null,
    lastMessage: string,
    lastMessageAt: string | null,
  ) => void
  search: string
  setSearch: (v: string) => void
  chatType: ChatTypeFilter
  setChatType: (v: ChatTypeFilter) => void
  chats: ChatListResponse[]
  loading: boolean
  load: () => void
}

function SkeletonChatItem() {
  return (
    <div className={styles.skeletonItem}>
      <div
        className="skeleton"
        style={{ width: 42, height: 42, borderRadius: '50%', flexShrink: 0 }}
      />
      <div
        style={{
          flex: 1,
          display: 'flex',
          flexDirection: 'column',
          gap: 6,
          minWidth: 0,
        }}
      >
        <div className="skeleton" style={{ height: 14, width: '55%' }} />
        <div className="skeleton" style={{ height: 11, width: '75%' }} />
      </div>
    </div>
  )
}

export function ChatSidebarView({
  selectedId,
  onSelect,
  search,
  setSearch,
  chatType,
  setChatType,
  chats,
  loading,
  load,
}: Props) {
  const [showNewChat, setShowNewChat] = useState(false)

  function handleNewChatStart(phone: string, name: string) {
    const jid = `${phone}@s.whatsapp.net`
    onSelect(-1, name, phone, jid, null, '', null)
    setShowNewChat(false)
  }

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
        <button
          className={styles.syncButton}
          onClick={() => setShowNewChat(true)}
          title="Novo chat"
        >
          <MessageSquarePlus size={16} />
        </button>
        <button
          className={styles.syncButton}
          onClick={load}
          title="Sincronizar"
        >
          <RefreshCw size={16} className={loading ? styles.spinning : ''} />
        </button>
      </header>

      <div className={styles.chatTypes}>
        <button
          className={chatType === 'open' ? styles.chatTypesActive : undefined}
          onClick={() => setChatType('open')}
        >
          Em aberto
        </button>
        <button
          className={chatType === 'mine' ? styles.chatTypesActive : undefined}
          onClick={() => setChatType('mine')}
        >
          Meus Chamados
        </button>
        <button
          className={chatType === 'all' ? styles.chatTypesActive : undefined}
          onClick={() => setChatType('all')}
        >
          Todos
        </button>
      </div>

      <section className={styles.chatList}>
        {loading ? (
          <>
            <SkeletonChatItem />
            <SkeletonChatItem />
            <SkeletonChatItem />
            <SkeletonChatItem />
            <SkeletonChatItem />
          </>
        ) : (
          chats.map((chat) => {
            const phone = chat.phoneNumber ?? ''
            const jid = chat.jid
            const displayName =
              chat.contactName ??
              chat.name ??
              chat.phoneNumber ??
              `Chat #${chat.id}`
            const occCount = chat.occurrences?.length ?? 0

            return (
              <div
                key={chat.id}
                className={`${styles.chatItem} ${selectedId === chat.id ? styles.active : ''}`}
                onClick={() =>
                  onSelect(
                    chat.id,
                    displayName,
                    phone,
                    jid,
                    chat.contactId,
                    chat.lastMessage?.body ?? '...',
                    chat.lastMessageAt,
                  )
                }
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
                    <label>#{phone}</label>
                    {chat.lastMessageAt && (
                      <span className={styles.chatTime}>
                        {formatTime(chat.lastMessageAt)}
                      </span>
                    )}
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
                          style={{
                            borderLeftColor:
                              OCCURRENCE_STATUS_COLORS[occ.status],
                          }}
                        >
                          <span className={styles.occTitle}>{occ.title}</span>
                          <span
                            className={styles.occStatus}
                            style={{
                              color: OCCURRENCE_STATUS_COLORS[occ.status],
                            }}
                          >
                            {OCCURRENCE_STATUS_LABELS[occ.status]}
                          </span>
                        </span>
                      ))}
                      {chat.occurrences.length > 2 && (
                        <span className={styles.occMore}>
                          +{chat.occurrences.length - 2} mais
                        </span>
                      )}
                    </div>
                  )}
                  {chat.lastMessage?.body ? (
                    <p className={styles.lastMsg}>{chat.lastMessage.body}</p>
                  ) : (
                    <p className={styles.lastMsg}>
                      {chat.lastMessage
                        ? `${MESSAGE_TYPE_MAP[chat.lastMessage.type] ?? 'Mensagem'}`
                        : ''}
                    </p>
                  )}
                </div>
              </div>
            )
          })
        )}
      </section>

      {showNewChat && (
        <NewChatModal
          onClose={() => setShowNewChat(false)}
          onStart={handleNewChatStart}
        />
      )}
    </aside>
  )
}
