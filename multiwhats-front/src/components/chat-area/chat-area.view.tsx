"use client"

import { useState, useCallback, useRef } from "react"
import { Check, CheckCheck, FileWarning, Paperclip, Send, Smile, UserPlus, X, Music, FileText, Upload } from "lucide-react"
import { AvatarView } from "../avatar/avatar.view"
import { useChatArea } from "./chat-area.logic"
import { SaveContactModal, OccurrenceModal } from "./chat-area-modals"
import { MessageMedia } from "../message-media/message-media.view"
import { Lightbox } from "../lightbox/lightbox.view"
import { ChatInfoPanel } from "./chat-info-panel"
import { formatTime, formatDateSeparator, shouldShowDateSeparator } from "../../utils/date-format"
import { formatMessageText } from "../../utils/message-formatter"
import { isContactType } from "../../types"
import { detectMediaType } from "../../utils/media"
import styles from "./chat-area.module.css"

interface Props {
  chatId: number | null
  contactName?: string
  phoneNumber?: string
  jid: string
  lastMessage: string
  lastMessageAt?: string | null
  chatContactId?: number | null
  onStartChat?: (phone: string, name: string) => void
  onOccurrenceCreated?: () => void
}

export function ChatAreaView({ chatId, contactName, phoneNumber, jid, chatContactId, lastMessage, lastMessageAt, onStartChat, onOccurrenceCreated }: Props) {
  const {
    inputValue,
    setInputValue,
    messages,
    sendingCount,
    sendError,
    sendMessage,
    selectedFile,
    mediaPreview,
    mediaType,
    fileInputRef,
    handleFileSelect,
    handleFileDrop,
    clearMedia,
    saveContact,
    occurrence,
  } = useChatArea(chatId, jid, lastMessage, lastMessageAt, onOccurrenceCreated)

  const [showInfoPanel, setShowInfoPanel] = useState(false)
  const [lightboxSrc, setLightboxSrc] = useState<string | null>(null)
  const [lightboxAlt, setLightboxAlt] = useState<string>("")
  const [isDragging, setIsDragging] = useState(false)
  const dragCounter = useRef(0)

  const handleImageClick = useCallback((src: string, alt: string) => {
    setLightboxSrc(src)
    setLightboxAlt(alt)
  }, [])

  const onDragEnter = useCallback((e: React.DragEvent) => {
    e.preventDefault()
    e.stopPropagation()
    dragCounter.current++
    if (e.dataTransfer.types.includes("Files")) setIsDragging(true)
  }, [])

  const onDragLeave = useCallback((e: React.DragEvent) => {
    e.preventDefault()
    e.stopPropagation()
    dragCounter.current--
    if (dragCounter.current === 0) setIsDragging(false)
  }, [])

  const onDragOver = useCallback((e: React.DragEvent) => {
    e.preventDefault()
    e.stopPropagation()
  }, [])

  const onDrop = useCallback((e: React.DragEvent) => {
    e.preventDefault()
    e.stopPropagation()
    dragCounter.current = 0
    setIsDragging(false)
    const file = e.dataTransfer.files?.[0]
    if (file) handleFileDrop(file)
  }, [handleFileDrop])

  const onPaste = useCallback((e: React.ClipboardEvent) => {
    const items = e.clipboardData?.items
    if (!items) return
    for (let i = 0; i < items.length; i++) {
      if (items[i].kind === "file") {
        const file = items[i].getAsFile()
        if (file) {
          e.preventDefault()
          handleFileDrop(file)
          return
        }
      }
    }
  }, [handleFileDrop])

  return (
    <main
      className={styles.chat}
      onDragEnter={onDragEnter}
      onDragLeave={onDragLeave}
      onDragOver={onDragOver}
      onDrop={onDrop}
      onPaste={onPaste}
    >

      <header className={styles.chatHeader}>
        {chatId ? (
          <AvatarView name={contactName ?? `Contato ${chatId}`} size={36} />
        ) : (
          <AvatarView name="?" size={36} />
        )}
        <div className={styles.data} onClick={() => chatId && setShowInfoPanel(true)} style={{ cursor: chatId ? "pointer" : undefined }}>
          <strong>{chatId ? contactName ?? `Contato #${chatId}` : "Nenhum contato"}</strong>
          <small>{chatId ? "Online" : "Selecione um contato"}</small>
        </div>
        {chatId && !chatContactId && (
          <button
            className={styles.saveContactBtn}
            onClick={() => saveContact.openModal(phoneNumber ?? "", contactName ?? "")}
          >
            <UserPlus size={15} />
            Salvar em contatos
          </button>
        )}
        {chatId && (
          <button
            className={styles.occBtn}
            onClick={occurrence.openModal}
          >
            <FileWarning size={15} />
            Abrir Ocorrência
          </button>
        )}
      </header>

      <section className={styles.messages}>
        {messages.length === 0 ? (
          <div className={styles.empty}>
            {chatId ? (
              <>
                <div className={styles.loadingMessages}>
                  <span className="spinner spinnerDark" />
                  Buscando mensagens...
                </div>
                <div className={styles.received}>
                  <div className={styles.bubble}>{lastMessage}</div>
                </div>
              </>
            ) : "Selecione um contato para ver as mensagens"}
          </div>
        ) : (
          messages.map((msg, idx) => {
            const prev = idx > 0 ? messages[idx - 1] : undefined
            const showDate = shouldShowDateSeparator(msg, prev)
            return (
              <div key={msg.id}>
                {showDate && (
                  <div className={styles.dateSeparator}>
                    <span>{formatDateSeparator(msg.sentAt)}</span>
                  </div>
                )}
                <div className={msg.direction === 0 ? styles.received : styles.sent}>
                  <div className={styles.messageRow}>
                    <div className={msg.mediaUrl && !msg.body ? `${styles.bubble} ${styles.bubbleMediaOnly}` : styles.bubble}>
                      {msg.mediaUrl || isContactType(msg.type) ? <MessageMedia msg={msg} onStartChat={onStartChat} onImageClick={handleImageClick} /> : null}
                     
                      { !isContactType(msg.type) &&  msg.body && <div>{formatMessageText(msg.body)}</div>}
                    </div>
                    <div className={styles.messageMeta}>
                      <span className={styles.timestamp}>{formatTime(msg.sentAt)}</span>
                      {msg.direction === 1 && (
                        <span className={styles.status}>
                          {msg.deliveryStatus === "Read" || msg.deliveryStatus === 3 ? (
                            <CheckCheck size={13} className={styles.read} />
                          ) : msg.deliveryStatus === "Delivered" || msg.deliveryStatus === 2 ? (
                            <CheckCheck size={13} />
                          ) : msg.deliveryStatus === "Sent" || msg.deliveryStatus === 1 ? (
                            <Check size={13} />
                          ) : msg.deliveryStatus === "Failed" ? (
                            <X size={11} className={styles.failed} />
                          ) : (
                            <Check size={13} className={styles.pending} />
                          )}
                        </span>
                      )}
                    </div>
                  </div>
                </div>
              </div>
            )
          })
        )}
        {sendingCount > 0 && Array.from({ length: sendingCount }).map((_, i) => (
          <div key={`sending-${i}`} className={styles.sent}>
            <div className={styles.messageRow}>
              <div className={`${styles.bubble} ${styles.sendingBubble}`}>
                <span className={styles.sendingDots}>
                  <span className={styles.dot} />
                  <span className={styles.dot} />
                  <span className={styles.dot} />
                </span>
              </div>
            </div>
          </div>
        ))}
      </section>

      {mediaPreview && (
        <div className={styles.mediaPreviewBar}>
          <div className={styles.mediaPreviewContent}>
            {mediaType === "Image" || mediaType === "Sticker" ? (
              <img src={mediaPreview} alt="Preview" className={styles.mediaPreviewImg} />
            ) : mediaType === "Video" ? (
              <video src={mediaPreview} className={styles.mediaPreviewImg} />
            ) : mediaType === "Audio" ? (
              <Music size={20} />
            ) : (
              <FileText size={20} />
            )}
            <span className={styles.mediaPreviewName}>{selectedFile?.name}</span>
            <button className={styles.mediaPreviewClear} onClick={clearMedia}>
              <X size={14} />
            </button>
          </div>
        </div>
      )}

      <footer className={styles.inputArea}>
        <button><Smile size={20} /></button>
        <input
          type="file"
          ref={fileInputRef}
          onChange={handleFileSelect}
          accept="image/*,video/*,audio/*,.pdf,.doc,.docx,.txt"
          style={{ display: "none" }}
        />
        <button onClick={() => fileInputRef.current?.click()}>
          <Paperclip size={20} />
        </button>
        <textarea
          placeholder="Digite uma mensagem..."
          rows={1}
          value={inputValue}
          onChange={(e) => setInputValue(e.target.value)}
          onKeyDown={(e) => {
            if (e.key === "Enter" && !e.ctrlKey && !e.shiftKey) {
              e.preventDefault()
              sendMessage()
            }
          }}
          onInput={(e) => {
            const el = e.currentTarget
            el.style.height = "auto"
            el.style.height = Math.min(el.scrollHeight, 120) + "px"
          }}
          style={{ resize: "none" }}
        />
        <button
          className={styles.send}
          onClick={sendMessage}
          disabled={sendingCount > 0 || (!inputValue.trim() && !selectedFile)}
        >
          <Send size={17} />
        </button>
      </footer>
      {isDragging && (
        <div className={styles.dropOverlay}>
          <div className={styles.dropContent}>
            <Upload size={40} />
            <span>Solte o arquivo aqui</span>
          </div>
        </div>
      )}

      {lightboxSrc && (
        <Lightbox src={lightboxSrc} alt={lightboxAlt} onClose={() => setLightboxSrc(null)} />
      )}

      {sendError && <div className={styles.error}>{sendError}</div>}

      {saveContact.showModal && (
        <SaveContactModal
          formJid={saveContact.formJid}
          formPhone={saveContact.formPhone}
          formName={saveContact.formName}
          formPushName={saveContact.formPushName}
          assignClientId={saveContact.assignClientId}
          clients={saveContact.clients}
          saving={saveContact.saving}
          error={saveContact.error}
          setFormPhone={saveContact.setFormPhone}
          setFormName={saveContact.setFormName}
          setAssignClientId={saveContact.setAssignClientId}
          onClose={saveContact.closeModal}
          onSave={saveContact.createContact}
        />
      )}

      {occurrence.showModal && (
        <OccurrenceModal
          title={occurrence.title}
          setTitle={occurrence.setTitle}
          description={occurrence.description}
          setDescription={occurrence.setDescription}
          priority={occurrence.priority}
          setPriority={occurrence.setPriority}
          saving={occurrence.saving}
          error={occurrence.error}
          onClose={occurrence.closeModal}
          onSave={occurrence.createOccurrence}
        />
      )}

      {showInfoPanel && chatId && (
        <ChatInfoPanel
          chatId={chatId}
          contactName={contactName ?? `Contato #${chatId}`}
          phoneNumber={phoneNumber ?? ""}
          jid={jid}
          onClose={() => setShowInfoPanel(false)}
        />
      )}
    </main>
  )
}
