"use client"

import { useState } from "react"
import { ChatAreaView } from "../../chat-area/chat-area.view"
import { ChatSidebarView } from "../../chat-sidebar/chat-sidebar.view"
import styles from "./chats.module.css"

export function ChatsView() {
  const [selectedId, setSelectedId] = useState<number | null>(null)
  const [selectedName, setSelectedName] = useState<string>("")
  const [selectedPhone, setSelectedPhone] = useState<string>("")
  const [selectedJid, setSelectedJid] = useState<string>("")
  const [selectedLastMessage, setSelectedLastMessage] = useState<string>("")
  const [selectedContactId, setSelectedContactId] = useState<number | null>(null)

  function handleSelect(id: number, name: string, phoneNumber: string, jid: string, contactId: number | null, lastMessage: string) {
    setSelectedId(id)
    setSelectedName(name)
    setSelectedPhone(phoneNumber)
    setSelectedJid(jid)
    setSelectedContactId(contactId)
    setSelectedLastMessage(lastMessage)
  }

  return (
    <div className={styles.container}>
      <ChatSidebarView selectedId={selectedId} onSelect={handleSelect} />
      <ChatAreaView
        chatId={selectedId}
        contactName={selectedName}
        phoneNumber={selectedPhone}
        jid={selectedJid}
        chatContactId={selectedContactId}
        lastMessage={selectedLastMessage}
      />
    </div>
  )
}
