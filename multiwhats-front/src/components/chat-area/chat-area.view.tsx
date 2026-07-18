"use client"

import { Paperclip, Send, Smile } from "lucide-react"
import { AvatarView } from "../avatar/avatar.view"
import { useChatArea } from "./chat-area.logic"
import styles from "./chat-area.module.css"

interface Props {
  contactId: number | null
  contactName?: string
}

export function ChatAreaView({ contactId, contactName }: Props) {
  const { inputValue, setInputValue, messages, sendMessage } = useChatArea(contactId)

  return (
    <main className={styles.chat}>
      <header className={styles.chatHeader}>
        {contactId ? (
          <AvatarView name={contactName ?? `Contato ${contactId}`} size={44} />
        ) : (
          <AvatarView name="?" size={44} />
        )}
        <div className={styles.data}>
          <strong>{contactId ? contactName ?? `Contato #${contactId}` : "Nenhum contato"}</strong>
          <small>{contactId ? "Online" : "Selecione um contato"}</small>
        </div>
      </header>

      <section className={styles.messages}>
        {messages.length === 0 ? (
          <div className={styles.empty}>
            {contactId ? "Buscando..." : "Selecione um contato para ver as mensagens"}
          </div>
        ) : (
          messages.map((msg) => (
            <div key={msg.id} className={msg.direction === "Incoming" ? styles.received : styles.sent}>
              <div className={styles.bubble}>{msg.body}</div>
            </div>
          ))
        )}
      </section>

      <footer className={styles.inputArea}>
        <button><Smile size={22} /></button>
        <button><Paperclip size={22} /></button>
        <input
          placeholder="Digite uma mensagem..."
          value={inputValue}
          onChange={(e) => setInputValue(e.target.value)}
          onKeyDown={(e) => e.key === "Enter" && sendMessage()}
        />
        <button className={styles.send} onClick={sendMessage}><Send size={20} /></button>
      </footer>
    </main>
  )
}
