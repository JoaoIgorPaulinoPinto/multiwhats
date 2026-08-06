
import { Image, Film, Music, FileText, Sticker, Paperclip, User, MessageSquare, Download } from "lucide-react"
import { useTransformedImage } from "../../utils/use-transformed-image"
import { toDataUrl } from "../../utils/media"
import { parseVCard } from "../../utils/vcard"
import type { MessageType } from "../../types"
import { isContactType } from "../../types"
import styles from "../chat-area/chat-area.module.css"
import vcardStyles from "./vcard-contact.module.css"

import { MESSAGE_TYPE_MAP } from "../../types"

function MediaIcon({ type }: { type: MessageType }) {
  const typeName = typeof type === "number" ? (MESSAGE_TYPE_MAP[type] ?? "Unknown") : type
  switch (typeName) {
    case "Image": return <Image size={14} />
    case "Video": return <Film size={14} />
    case "Audio": return <Music size={14} />
    case "Sticker": return <Sticker size={14} />
    case "Document": return <FileText size={14} />
    default: return <Paperclip size={14} />
  }
}

export function MessageImage({ raw, mime, alt, style, onClick }: { raw: string; mime: string | null; alt: string; style?: React.CSSProperties; onClick?: () => void }) {
  const { src, loading, ref } = useTransformedImage(raw, mime)
  if (loading) return <div ref={ref} className="skeleton" style={{ width: 200, height: 150, borderRadius: 6 }} />
  if (!src) return null
  return <img src={src} alt={alt} loading="lazy" style={style} onClick={onClick} />
}

interface MessageMediaProps {
  msg: {
    type: MessageType
    mediaUrl: string | null
    mediaMimeType: string | null
    mediaFilename: string | null
    mediaCaption: string | null
    mediaSize: number | null
    body: string | null
  }
  onStartChat?: (phone: string, name: string) => void
  onImageClick?: (src: string, alt: string) => void
}

function VCardContact({ raw, body, onStartChat }: { raw: string | null; body: string | null; onStartChat?: (phone: string, name: string) => void }) {
  const vcardText = raw && raw.startsWith("BEGIN:VCARD") ? raw : body
  const parsed = vcardText ? parseVCard(vcardText) : null

  if (!parsed) {
    return (
      <div className={styles.mediaFile}>
        <User size={14} />
        <span>Contato</span>
      </div>
    )
  }

  return (
    <div className={vcardStyles.vcard}>
      <div className={vcardStyles.avatar}>
        {parsed.firstName[0]?.toUpperCase() || "C"}
      </div>
      <div className={vcardStyles.info}>
        <span className={vcardStyles.name}>{parsed.name}</span>
        {parsed.phone && <span className={vcardStyles.phone}>{parsed.phone}</span>}
      </div>
      {parsed.phone && onStartChat && (
        <button
          className={vcardStyles.startBtn}
          onClick={(e) => {
            e.stopPropagation()
            onStartChat(parsed.phone, parsed.name)
          }}
          title="Iniciar chat"
        >
          <MessageSquare size={14} />
        </button>
      )}
    </div>
  )
}

function isDocumentType(type: MessageType): boolean {
  return type === "Document" || type === 4
}

function formatFileSize(bytes: number | null): string {
  if (!bytes) return ""
  if (bytes < 1024) return `${bytes} B`
  if (bytes < 1048576) return `${(bytes / 1024).toFixed(1)} KB`
  return `${(bytes / 1048576).toFixed(1)} MB`
}

export function MessageMedia({ msg, onStartChat }: MessageMediaProps) {
  if (isContactType(msg.type)) {
    return <VCardContact raw={msg.mediaUrl} body={msg.body} onStartChat={onStartChat} />
  }

  if (!msg.mediaUrl) return null

  if (isDocumentType(msg.type)) {
    return (
      <div className={styles.mediaDocument}>
        <div className={styles.mediaDocumentIcon}>
          <FileText size={18} />
        </div>
        <div className={styles.mediaDocumentInfo}>
          <span className={styles.mediaDocumentName}>{msg.mediaFilename || msg.body || "Documento"}</span>
          {msg.mediaSize && <span className={styles.mediaDocumentSize}>{formatFileSize(msg.mediaSize)}</span>}
        </div>
        <a href={toDataUrl(msg.mediaUrl, msg.mediaMimeType)} download={msg.mediaFilename || "documento"} onClick={(e) => e.stopPropagation()}>
          <Download size={16} style={{ color: "var(--primary)", flexShrink: 0 }} />
        </a>
      </div>
    )
  }

  const isImage = msg.type === "Image" || msg.mediaMimeType?.startsWith("image/")
  const isVideo = msg.type === "Video" || msg.mediaMimeType?.startsWith("video/")
  const isAudio = msg.type === "Audio" || msg.mediaMimeType?.startsWith("audio/")
  const isSticker = msg.type === "Sticker" || msg.mediaMimeType?.startsWith("image/webp")

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
      </div>
    )
  }

  if (isVideo) {
    return (
      <div className={styles.mediaVideo}>
        <video src={toDataUrl(msg.mediaUrl, msg.mediaMimeType)} controls style={{ maxWidth: 300, maxHeight: 300, borderRadius: 6 }} />
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
