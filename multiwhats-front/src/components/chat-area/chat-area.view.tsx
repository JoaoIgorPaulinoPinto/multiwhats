"use client"

import { Paperclip, Send, Smile, UserPlus, X } from "lucide-react"
import { AvatarView } from "../avatar/avatar.view"
import { useChatArea } from "./chat-area.logic"
import styles from "./chat-area.module.css"

interface Props {
  chatId: number | null
  contactName?: string
  phoneNumber?: string
  jid: string
  chatContactId?: number | null
}

export function ChatAreaView({ chatId, contactName, phoneNumber, jid, chatContactId }: Props) {
  const {
    inputValue,
    setInputValue,
    messages,
    sendError,
    sendMessage,
    showSaveModal,
    formJid,
    formPhone,
    formName,
    formPushName,
    assignClientId,
    clients,
    saveLoading,
    saveError,
    setFormPhone,
    setFormName,
    setAssignClientId,
    openSaveModal,
    closeSaveModal,
    createContact,
  } = useChatArea(chatId, jid)

  return (
    <main className={styles.chat}>
      <header className={styles.chatHeader}>
        {chatId ? (
          <AvatarView name={contactName ?? `Contato ${chatId}`} size={36} />
        ) : (
          <AvatarView name="?" size={36} />
        )}
        <div className={styles.data}>
          <strong>{chatId ? contactName ?? `Contato #${chatId}` : "Nenhum contato"}</strong>
          <small>{chatId ? "Online" : "Selecione um contato"}</small>
        </div>
        {chatId && chatContactId === null && (
          <button
            className={styles.saveContactBtn}
            onClick={() => openSaveModal(phoneNumber ?? "", contactName ?? "")}
          >
            <UserPlus size={15} />
            Salvar em contatos
          </button>
        )}
      </header>

      <section className={styles.messages}>
        {messages.length === 0 ? (
          <div className={styles.empty}>
            {chatId ? "Buscando..." : "Selecione um contato para ver as mensagens"}
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
        <button><Smile size={20} /></button>
        <button><Paperclip size={20} /></button>
        <input
          placeholder="Digite uma mensagem..."
          value={inputValue}
          onChange={(e) => setInputValue(e.target.value)}
          onKeyDown={(e) => e.key === "Enter" && sendMessage()}
        />
        <button className={styles.send} onClick={sendMessage}><Send size={17} /></button>
      </footer>
      {sendError && <div className={styles.error}>{sendError}</div>}

      {showSaveModal && (
        <>
          <div className={styles.overlay} onClick={closeSaveModal} />
          <div className={styles.modal}>
            <div className={styles.modalHeader}>
              <h3>Salvar em contatos</h3>
              <button className={styles.closeBtn} onClick={closeSaveModal}>
                <X size={18} />
              </button>
            </div>

            <div className={styles.field}>
              <label>JID (WhatsApp ID)</label>
              <input value={formJid} readOnly className={styles.readOnly} />
            </div>

            <div className={styles.field}>
              <label>Telefone</label>
              <input value={formPhone} onChange={(e) => setFormPhone(e.target.value)} />
            </div>

            <div className={styles.field}>
              <label>Nome</label>
              <input value={formName} onChange={(e) => setFormName(e.target.value)} />
            </div>

            <div className={styles.field}>
              <label>Push Name (WhatsApp)</label>
              <input value={formPushName} readOnly className={styles.readOnly} />
            </div>

            <div className={styles.field}>
              <label>Empresa</label>
              <select
                className={styles.select}
                value={assignClientId ?? ""}
                onChange={(e) => setAssignClientId(e.target.value ? Number(e.target.value) : null)}
              >
                <option value="">Sem empresa</option>
                {clients.map((c) => (
                  <option key={c.id} value={c.id}>{c.name}</option>
                ))}
              </select>
            </div>

            {saveError && <div className={styles.saveError}>{saveError}</div>}

            <div className={styles.modalActions}>
              <button className={styles.cancelBtn} onClick={closeSaveModal}>Cancelar</button>
              <button className={styles.saveBtn} onClick={createContact} disabled={saveLoading}>
                {saveLoading ? "Salvando..." : "Salvar"}
              </button>
            </div>
          </div>
        </>
      )}
    </main>
  )
}
