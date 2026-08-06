
import { useEffect, useState } from 'react'
import type { ChatListResponse } from '../../../services/chats.service'
import { chatsService } from '../../../services/chats.service'
import { useAuthStore } from '../../../stores/auth-store'
import { toNumericStatus } from '../../../types'
import { ChatAreaView } from '../../chat-area/chat-area.view'
import { AssignChatModal } from '../../chat-sidebar/assign-chat-modal'
import { useChatSidebar } from '../../chat-sidebar/chat-sidebar.logic'
import { ChatSidebarView } from '../../chat-sidebar/chat-sidebar.view'
import styles from './chats.module.css'

interface PendingSelection {
  id: number
  name: string
  clientName: string | null
  phoneNumber: string
  jid: string
  contactId: number | null
  lastMessage: string
  lastMessageAt: string | null
  assignedToUserId: number | null
  hasUnread: boolean
  contactProfilePicUrl: string | null
}

export function ChatsView() {
  const {
    search,
    setSearch,
    chatType,
    setChatType,
    onlyMine,
    setOnlyMine,
    chats,
    loading,
    load,
    loadMore,
    loadingMore,
    hasNext,
    totalCount,
  } = useChatSidebar()
  const currentUserId = useAuthStore((s) => s.user?.id)
  const [selectedId, setSelectedId] = useState<number | null>(null)
  const [selectedName, setSelectedName] = useState<string>('')
  const [selectedClientName, setSelectedClientName] = useState<string | null>(
    null,
  )
  const [selectedPhone, setSelectedPhone] = useState<string>('')
  const [selectedJid, setSelectedJid] = useState<string>('')
  const [selectedLastMessage, setSelectedLastMessage] = useState<string>('')
  const [selectedLastMessageAt, setSelectedLastMessageAt] = useState<
    string | null
  >(null)
  const [selectedContactId, setSelectedContactId] = useState<number | null>(
    null,
  )
  const [selectedAssignedToUserId, setSelectedAssignedToUserId] = useState<
    number | null
  >(null)
  const [selectedProfilePicUrl, setSelectedProfilePicUrl] = useState<
    string | null
  >(null)

  const [pending, setPending] = useState<PendingSelection | null>(null)
  const [assigning, setAssigning] = useState(false)
  const [assignError, setAssignError] = useState<string | null>(null)

  function applySelection(sel: PendingSelection) {
    setSelectedId(sel.id)
    setSelectedName(sel.name)
    setSelectedClientName(sel.clientName)
    setSelectedPhone(sel.phoneNumber)
    setSelectedJid(sel.jid)
    setSelectedContactId(sel.contactId)
    setSelectedLastMessage(sel.lastMessage)
    setSelectedLastMessageAt(sel.lastMessageAt)
    setSelectedAssignedToUserId(sel.assignedToUserId)
    setSelectedProfilePicUrl(sel.contactProfilePicUrl)
  }

  function clearSelection() {
    setSelectedId(null)
    setSelectedName('')
    setSelectedClientName(null)
    setSelectedPhone('')
    setSelectedJid('')
    setSelectedContactId(null)
    setSelectedLastMessage('')
    setSelectedLastMessageAt(null)
    setSelectedAssignedToUserId(null)
    setSelectedProfilePicUrl(null)
    setPending(null)
    setAssignError(null)
  }
  useEffect(() => {
    const count = chats.filter(
      (chat) =>
        chat.lastMessage?.deliveryStatus === 0 &&
        chat.lastMessage?.direction === 1,
    ).length
    document.title = count > 0 ? `(${count}) MultiWhats` : 'MultiWhats'
  }, [chats])

  useEffect(() => {
    function handleKeyDown(e: KeyboardEvent) {
      if (e.key === 'Escape') clearSelection()
    }
    window.addEventListener('keydown', handleKeyDown)
    return () => window.removeEventListener('keydown', handleKeyDown)
  }, [])

  function handleSelect(
    id: number,
    name: string,
    clientName: string | null,
    phoneNumber: string,
    jid: string,
    contactId: number | null,
    lastMessage: string,
    lastMessageAt: string | null,
    assignedToUserId: number | null,
    hasUnread: boolean,
    contactProfilePicUrl: string | null,
  ) {
    const sel: PendingSelection = {
      id,
      name,
      clientName,
      phoneNumber,
      jid,
      contactId,
      lastMessage,
      lastMessageAt,
      assignedToUserId,
      hasUnread,
      contactProfilePicUrl,
    }
    if (hasUnread && assignedToUserId !== currentUserId) {
      setAssignError(null)
      setPending(sel)
      return
    }
    setPending(null)
    applySelection(sel)
  }

  async function handleAssignConfirm() {
    if (!pending) return
    setAssigning(true)
    setAssignError(null)
    try {
      const res = await chatsService.assignChat(pending.id)
      applySelection({ ...pending, assignedToUserId: res.assignedToUserId })
      load()
      setPending(null)
    } catch (e) {
      setAssignError(
        e instanceof Error ? e.message : 'Erro ao iniciar atendimento',
      )
    } finally {
      setAssigning(false)
    }
  }

  function handleAssignCancel() {
    if (!pending) return
    applySelection(pending)
    setPending(null)
  }

  async function handleFinishAttendance() {
    if (selectedId == null) return
    if (
      !window.confirm(
        'Finalizar o atendimento deste chat? A ocorrência será mantida.',
      )
    )
      return
    try {
      const res = await chatsService.unassignChat(selectedId)
      setSelectedAssignedToUserId(res.assignedToUserId)
      load()
    } catch (e) {
      window.alert(
        e instanceof Error ? e.message : 'Erro ao finalizar atendimento',
      )
    }
  }

  function handleStartChat(phone: string, name: string) {
    const jid = `${phone}@s.whatsapp.net`
    setPending(null)
    setSelectedId(-1)
    setSelectedName(name)
    setSelectedClientName(null)
    setSelectedPhone(phone)
    setSelectedJid(jid)
    setSelectedContactId(null)
    setSelectedLastMessage('')
    setSelectedLastMessageAt(null)
    setSelectedAssignedToUserId(null)
    setSelectedProfilePicUrl(null)
  }

  function handleMerged(result: {
    survivorJid: string
    survivor?: ChatListResponse
  }) {
    load()
    if (!result.survivor) return
    const s = result.survivor
    handleSelect(
      s.id,
      s.contactName ?? s.name ?? s.phoneNumber ?? `Chat #${s.id}`,
      s.clientName,
      s.phoneNumber ?? '',
      s.jid,
      s.contactId,
      s.lastMessage?.body ?? '',
      s.lastMessageAt,
      s.assignedToUserId,
      !!(
        s.lastMessage &&
        s.lastMessage.direction === 0 &&
        toNumericStatus(s.lastMessage.deliveryStatus ?? 2) < 3
      ),
      s.contactProfilePicUrl,
    )
  }

  const canMarkRead =
    currentUserId != null && selectedAssignedToUserId === currentUserId

  return (
    <div className={styles.container}>
      <ChatSidebarView
        selectedId={selectedId}
        onSelect={handleSelect}
        search={search}
        setSearch={setSearch}
        chatType={chatType}
        setChatType={setChatType}
        onlyMine={onlyMine}
        setOnlyMine={setOnlyMine}
        chats={chats}
        loading={loading}
        load={load}
        loadMore={loadMore}
        loadingMore={loadingMore}
        hasNext={hasNext}
        totalCount={totalCount}
      />
      <div className={styles.chatAreaWrap}>
        <ChatAreaView
          chatId={selectedId}
          contactName={selectedName}
          clientName={selectedClientName}
          phoneNumber={selectedPhone}
          jid={selectedJid}
          chatContactId={selectedContactId}
          contactProfilePicUrl={selectedProfilePicUrl}
          lastMessage={selectedLastMessage}
          lastMessageAt={selectedLastMessageAt}
          canMarkRead={canMarkRead}
          canOpenOccurrence={canMarkRead}
          onStartChat={handleStartChat}
          onOccurrenceCreated={load}
          onFinishAttendance={handleFinishAttendance}
          onMerged={handleMerged}
        />
        {pending ? (
          <AssignChatModal
            chatName={pending.name}
            saving={assigning}
            error={assignError}
            inline
            onConfirm={handleAssignConfirm}
            onCancel={handleAssignCancel}
          />
        ) : null}
      </div>
    </div>
  )
}
