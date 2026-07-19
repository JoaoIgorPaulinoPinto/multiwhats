"use client"

import { useState } from "react"
import { ChatAreaView } from "../../chat-area/chat-area.view"
import { ChatSidebarView } from "../../chat-sidebar/chat-sidebar.view"
import styles from "./chats.module.css"

export function ChatsView() {
  const [selectedId, setSelectedId] = useState<number | null>(null)
  const [selectedName, setSelectedName] = useState<string>("")
  const [selectedPhone, setSelectedPhone] = useState<string>("")

  function handleSelect(id: number, name: string, phoneNumber: string) {
    setSelectedId(id)
    setSelectedName(name)
    setSelectedPhone(phoneNumber)
  }

  return (
    <div className={styles.container}>
      <ChatSidebarView selectedId={selectedId} onSelect={handleSelect} />
      <ChatAreaView contactId={selectedId} contactName={selectedName} phoneNumber={selectedPhone} />
    </div>
  )
}
