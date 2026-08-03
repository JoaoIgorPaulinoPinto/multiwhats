"use client"

import { useState } from "react"
import { ChatAreaView } from "../../chat-area/chat-area.view"
import { ChatSidebarView } from "../../chat-sidebar/chat-sidebar.view"
import { useChatSidebar } from "../../chat-sidebar/chat-sidebar.logic"
import { AssignChatModal } from "../../chat-sidebar/assign-chat-modal"
import { chatsService } from "../../../services/chats.service"
import { useAuthStore } from "../../../stores/auth-store"
import type { ChatListResponse } from "../../../services/chats.service"
import styles from "./chats.module.css"

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
}

export function ChatsView() {
  const { search, setSearch, chatType, setChatType, chats, loading, load } =
    useChatSidebar()
  const currentUserId = useAuthStore((s) => s.user?.id)
  const [selectedId, setSelectedId] = useState<number | null>(null)
  const [selectedName, setSelectedName] = useState<string>("")
  const [selectedClientName, setSelectedClientName] = useState<string | null>(null)
  const [selectedPhone, setSelectedPhone] = useState<string>("")
  const [selectedJid, setSelectedJid] = useState<string>("")
  const [selectedLastMessage, setSelectedLastMessage] = useState<string>("")
  const [selectedLastMessageAt, setSelectedLastMessageAt] = useState<string | null>(null)
  const [selectedContactId, setSelectedContactId] = useState<number | null>(null)
  const [selectedAssignedToUserId, setSelectedAssignedToUserId] = useState<number | null>(null)

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
  }

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
    }
    if (assignedToUserId == null) {
      setAssignError(null)
      setPending(sel)
      return
    }
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
      setAssignError(e instanceof Error ? e.message : "Erro ao iniciar atendimento")
    } finally {
      setAssigning(false)
    }
  }

  function handleAssignCancel() {
    if (!pending) return
    applySelection(pending)
    setPending(null)
  }

  function handleStartChat(phone: string, name: string) {
    const jid = `${phone}@s.whatsapp.net`
    setSelectedId(-1)
    setSelectedName(name)
    setSelectedClientName(null)
    setSelectedPhone(phone)
    setSelectedJid(jid)
    setSelectedContactId(null)
    setSelectedLastMessage("")
    setSelectedLastMessageAt(null)
    setSelectedAssignedToUserId(null)
  }

  function handleMerged(result: { survivorJid: string; survivor?: ChatListResponse }) {
    load()
    if (!result.survivor) return
    const s = result.survivor
    handleSelect(
      s.id,
      s.contactName ?? s.name ?? s.phoneNumber ?? `Chat #${s.id}`,
      s.clientName,
      s.phoneNumber ?? "",
      s.jid,
      s.contactId,
      s.lastMessage?.body ?? "",
      s.lastMessageAt,
      s.assignedToUserId,
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
        chats={chats}
        loading={loading}
        load={load}
      />
      <div className={styles.chatAreaWrap}>
        <ChatAreaView
          chatId={selectedId}
          contactName={selectedName}
          clientName={selectedClientName}
          phoneNumber={selectedPhone}
          jid={selectedJid}
          chatContactId={selectedContactId}
          lastMessage={selectedLastMessage}
          lastMessageAt={selectedLastMessageAt}
          canMarkRead={canMarkRead}
          onStartChat={handleStartChat}
          onOccurrenceCreated={load}
          onMerged={handleMerged}
        />

        {pending && (
          <AssignChatModal
            chatName={pending.name}
            saving={assigning}
            error={assignError}
            inline
            onConfirm={handleAssignConfirm}
            onCancel={handleAssignCancel}
          />
        )}
      </div>
    </div>
  )
}
