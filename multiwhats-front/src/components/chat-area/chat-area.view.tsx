"use client"

import { Check, CheckCheck, FileWarning, Image, Paperclip, Send, Smile, UserPlus, X, Film, Music, FileText, Sticker } from "lucide-react"
import { AvatarView } from "../avatar/avatar.view"
import { useChatArea } from "./chat-area.logic"
import { useTransformedImage } from "../../utils/use-transformed-image"
import styles from "./chat-area.module.css"
import type { MessageType } from "../../services/chats.service"

interface Props {
  chatId: number | null
  contactName?: string
  phoneNumber?: string
  jid: string
  lastMessage: string
  lastMessageAt?: string | null
  chatContactId?: number | null
}

function MediaIcon({ type }: { type: MessageType }) {
  switch (type) {
    case "Image": return <Image size={14} />
    case "Video": return <Film size={14} />
    case "Audio": return <Music size={14} />
    case "Sticker": return <Sticker size={14} />
    case "Document": return <FileText size={14} />
    default: return <Paperclip size={14} />
  }
}

function toDataUrl(raw: string, mime: string | null): string {
  if (raw.startsWith("data:")) return raw
  const m = mime || guessMime(raw)
  return `data:${m};base64,${raw}`
}

function guessMime(raw: string): string {
  if (raw.startsWith("/9j/")) return "image/jpeg"
  if (raw.startsWith("iVBOR")) return "image/png"
  if (raw.startsWith("UklGR")) return "image/webp"
  if (raw.startsWith("R0lGO")) return "image/gif"
  if (raw.startsWith("JVBER")) return "application/pdf"
  if (raw.startsWith("UEsD")) return "application/zip"
  return "application/octet-stream"
}

function MessageImage({ raw, mime, alt, style }: { raw: string; mime: string | null; alt: string; style?: React.CSSProperties }) {
  const { src, loading } = useTransformedImage(raw, mime)
  if (loading) return <div className="skeleton" style={{ width: 200, height: 150, borderRadius: 6 }} />
  if (!src) return null
  return <img src={src} alt={alt} loading="lazy" style={style} />
}

function MessageMedia({ msg }: { msg: { type: MessageType; mediaUrl: string | null; mediaMimeType: string | null; mediaFilename: string | null; mediaCaption: string | null; body: string | null } }) {
  if (!msg.mediaUrl) return null

  const isImage = msg.type === "Image" || msg.mediaMimeType?.startsWith("image/")
  const isVideo = msg.type === "Video" || msg.mediaMimeType?.startsWith("video/")
  const isAudio = msg.type === "Audio" || msg.mediaMimeType?.startsWith("audio/")
  const isSticker = msg.type === "Sticker"

  if (isSticker) {
    return (
      <div className={styles.mediaSticker}>
        <MessageImage raw={msg.mediaUrl} mime={msg.mediaMimeType} alt="Sticker" style={{ maxWidth: 180, maxHeight: 180 }} />
      </div>
    )
  }

  if (isImage) {
    return (
      <div className={styles.mediaImage}>
        <MessageImage raw={msg.mediaUrl} mime={msg.mediaMimeType} alt={msg.mediaCaption || "Imagem"} style={{ maxWidth: 600, maxHeight: 600, borderRadius: 6 }} />
        {msg.mediaCaption && <p className={styles.mediaCaption}>{msg.mediaCaption}</p>}
      </div>
    )
  }

  if (isVideo) {
    return (
      <div className={styles.mediaVideo}>
        <video src={toDataUrl(msg.mediaUrl, msg.mediaMimeType)} controls style={{ maxWidth: 300, maxHeight: 300, borderRadius: 6 }} />
        {msg.mediaCaption && <p className={styles.mediaCaption}>{msg.mediaCaption}</p>}
      </div>
    )
  }

  if (isAudio) {
    return (
      <div className={styles.mediaAudio}>
        <audio src={toDataUrl(msg.mediaUrl, msg.mediaMimeType)} controls style={{ width: 240 }} />
      </div>
    )
  }

  return (
    <div className={styles.mediaFile}>
      <MediaIcon type={msg.type} />
      <span>{msg.mediaFilename || msg.body || "Arquivo"}</span>
    </div>
  )
}

export function ChatAreaView({ chatId, contactName, phoneNumber, jid, chatContactId, lastMessage}: Props) {
  const {
    inputValue,
    setInputValue,
    messages,
    sending,
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
    showOccModal,
    occTitle,
    setOccTitle,
    occDescription,
    setOccDescription,
    occPriority,
    setOccPriority,
    occLoading,
    occError,
    openOccModal,
    closeOccModal,
    createOccurrence,
    selectedFile,
    mediaPreview,
    mediaType,
    fileInputRef,
    handleFileSelect,
    clearMedia,
  } = useChatArea(chatId, jid, lastMessage)

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
        {chatId && !chatContactId && (
          <button
            className={styles.saveContactBtn}
            onClick={() => openSaveModal(phoneNumber ?? "", contactName ?? "")}
          >
            <UserPlus size={15} />
            Salvar em contatos
          </button>
        )}
        {chatId && (
          <button
            className={styles.occBtn}
            onClick={openOccModal}
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
          messages.map((msg) => (
            <div key={msg.id} className={msg.direction === 0 ? styles.received : styles.sent}>
              <div className={styles.bubble}>
                {msg.mediaUrl ? (
                  <MessageMedia msg={msg} />
                ) : null}
                {msg.body && <div>{msg.body}</div>}
              </div>
              {msg.direction === 1 && (
                <span className={styles.status}>
                  {msg.deliveryStatus === "Read" || msg.deliveryStatus === 3 ? (
                    <CheckCheck size={14} className={styles.read} />
                  ) : msg.deliveryStatus === "Delivered" || msg.deliveryStatus === 2 ? (
                    <CheckCheck size={14} />
                  ) : msg.deliveryStatus === "Sent" || msg.deliveryStatus === 1 ? (
                    <Check size={14} />
                  ) : msg.deliveryStatus === "Failed" ? (
                    <X size={12} className={styles.failed} />
                  ) : (
                    <Check size={14} className={styles.pending} />
                  )}
                </span>
              )}
            </div>
          ))
        )}
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
        <input
          placeholder="Digite uma mensagem..."
          value={inputValue}
          onChange={(e) => setInputValue(e.target.value)}
          onKeyDown={(e) => e.key === "Enter" && sendMessage()}
        />
        <button
          className={styles.send}
          onClick={sendMessage}
          disabled={sending || (!inputValue.trim() && !selectedFile)}
        >
          {sending ? <span className="spinner" /> : <Send size={17} />}
        </button>
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

      {showOccModal && (
        <>
          <div className={styles.overlay} onClick={closeOccModal} />
          <div className={styles.modal}>
            <div className={styles.modalHeader}>
              <h3>Abrir Ocorrência</h3>
              <button className={styles.closeBtn} onClick={closeOccModal}>
                <X size={18} />
              </button>
            </div>

            <div className={styles.field}>
              <label>Título *</label>
              <input
                value={occTitle}
                onChange={(e) => setOccTitle(e.target.value)}
                placeholder="Ex: Problema com boleto"
              />
            </div>

            <div className={styles.field}>
              <label>Descrição</label>
              <textarea
                value={occDescription}
                onChange={(e) => setOccDescription(e.target.value)}
                placeholder="Descreva o problema..."
                rows={3}
                style={{ resize: "vertical" }}
              />
            </div>

            <div className={styles.field}>
              <label>Prioridade</label>
              <select
                className={styles.select}
                value={occPriority}
                onChange={(e) => setOccPriority(Number(e.target.value))}
              >
                <option value={0}>Baixa</option>
                <option value={1}>Média</option>
                <option value={2}>Alta</option>
                <option value={3}>Urgente</option>
              </select>
            </div>

            {occError && <div className={styles.saveError}>{occError}</div>}

            <div className={styles.modalActions}>
              <button className={styles.cancelBtn} onClick={closeOccModal}>Cancelar</button>
              <button className={styles.saveBtn} onClick={createOccurrence} disabled={occLoading || !occTitle.trim()}>
                {occLoading ? "Criando..." : "Criar Ocorrência"}
              </button>
            </div>
          </div>
        </>
      )}
    </main>
  )
}
