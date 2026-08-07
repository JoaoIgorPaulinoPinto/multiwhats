import {
  Check,
  CheckCheck,
  FileText,
  FileWarning,
  LogOut,
  Music,
  Paperclip,
  Send,
  Smile,
  Upload,
  UserPlus,
  X,
} from 'lucide-react'
import {
  useCallback,
  useEffect,
  useLayoutEffect,
  useRef,
  useState,
} from 'react'
import type { ChatListResponse } from '../../services/chats.service'
import {
  DELIVERY_STATUS_LABELS,
  isContactType,
  toNumericStatus,
  type DeliveryStatus,
} from '../../types'
import {
  formatDateSeparator,
  formatTime,
  shouldShowDateSeparator,
} from '../../utils/date-format'
import { formatMessageText } from '../../utils/message-formatter'
import { AvatarView } from '../avatar/avatar.view'
import { Lightbox } from '../lightbox/lightbox.view'
import { MessageMedia } from '../message-media/message-media.view'
import { OccurrenceModal, SaveContactModal } from './chat-area-modals'
import { useChatArea } from './chat-area.logic'
import styles from './chat-area.module.css'
import { ChatInfoPanel } from './chat-info-panel'
import { EmojiPicker } from './emoji-picker'

const SCROLL_THRESHOLD = 80

interface Props {
  chatId: number | null
  contactName?: string
  clientName?: string | null
  phoneNumber?: string
  jid: string
  lastMessage: string
  lastMessageAt?: string | null
  chatContactId?: number | null
  contactProfilePicUrl?: string | null
  canMarkRead?: boolean
  canOpenOccurrence?: boolean
  onStartChat?: (phone: string, name: string) => void
  onOccurrenceCreated?: () => void
  onFinishAttendance?: () => void
  onMerged?: (result: {
    survivorJid: string
    survivor?: ChatListResponse
  }) => void
}

function StatusTicks({ status }: { status: DeliveryStatus }) {
  const numeric = toNumericStatus(status)
  const title = DELIVERY_STATUS_LABELS[numeric] ?? ''
  let icon: React.ReactNode
  if (numeric === 3) icon = <CheckCheck size={13} className={styles.read} />
  else if (numeric === 2) icon = <CheckCheck size={13} />
  else if (numeric === 1) icon = <Check size={13} />
  else if (numeric === 4) icon = <X size={11} className={styles.failed} />
  else icon = <Check size={13} className={styles.pending} />
  return (
    <span className={styles.status} title={title}>
      {icon}
    </span>
  )
}

export function ChatAreaView({
  chatId,
  contactName,
  clientName,
  phoneNumber,
  jid,
  chatContactId,
  contactProfilePicUrl,
  lastMessage,
  lastMessageAt,
  canMarkRead = false,
  canOpenOccurrence = false,
  onStartChat,
  onOccurrenceCreated,
  onFinishAttendance,
  onMerged,
}: Props) {
  const {
    inputValue,
    setInputValue,
    messages,
    sendingCount,
    sendError,
    sendMessage,
    loadingMore,
    hasMore,
    loadMoreMessages,
    selectedFile,
    mediaPreview,
    mediaType,
    fileInputRef,
    handleFileSelect,
    handleFileDrop,
    clearMedia,
    saveContact,
    occurrence,
  } = useChatArea(
    chatId,
    jid,
    lastMessage,
    lastMessageAt,
    onOccurrenceCreated,
    canMarkRead,
  )

  const [showInfoPanel, setShowInfoPanel] = useState(false)
  const [lightboxSrc, setLightboxSrc] = useState<string | null>(null)
  const [lightboxAlt, setLightboxAlt] = useState<string>('')
  const [isDragging, setIsDragging] = useState(false)
  const [showEmojiPicker, setShowEmojiPicker] = useState(false)
  const dragCounter = useRef(0)
  const messagesRef = useRef<HTMLElement>(null)
  const restoreTopRef = useRef(false)
  const textareaRef = useRef<HTMLTextAreaElement>(null)
  const pickerRef = useRef<HTMLDivElement>(null)

  const handleMessagesScroll = useCallback(() => {
    const el = messagesRef.current
    if (!el || loadingMore || !hasMore) return
    if (el.scrollTop >= el.scrollHeight - el.clientHeight - SCROLL_THRESHOLD) {
      restoreTopRef.current = true
      loadMoreMessages()
    }
  }, [loadingMore, hasMore, loadMoreMessages])

  useLayoutEffect(() => {
    const el = messagesRef.current
    if (!restoreTopRef.current) return
    restoreTopRef.current = false
    if (el) el.scrollTop = el.scrollHeight - el.clientHeight
  }, [messages])

  useEffect(() => {
    if (messages.length === 0 && messagesRef.current) {
      messagesRef.current.scrollTop = 0
    }
  }, [messages])

  const items = messages
    .map((msg, idx) => ({ msg, prev: idx > 0 ? messages[idx - 1] : undefined }))
    .reverse()

  const handleImageClick = useCallback((src: string, alt: string) => {
    setLightboxSrc(src)
    setLightboxAlt(alt)
  }, [])

  const onDragEnter = useCallback((e: React.DragEvent) => {
    e.preventDefault()
    e.stopPropagation()
    dragCounter.current++
    if (e.dataTransfer.types.includes('Files')) setIsDragging(true)
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

  const onDrop = useCallback(
    (e: React.DragEvent) => {
      e.preventDefault()
      e.stopPropagation()
      dragCounter.current = 0
      setIsDragging(false)
      const file = e.dataTransfer.files?.[0]
      if (file) handleFileDrop(file)
    },
    [handleFileDrop],
  )

  const onPaste = useCallback(
    (e: React.ClipboardEvent) => {
      const items = e.clipboardData?.items
      if (!items) return
      for (let i = 0; i < items.length; i++) {
        if (items[i].kind === 'file') {
          const file = items[i].getAsFile()
          if (file) {
            e.preventDefault()
            handleFileDrop(file)
            return
          }
        }
      }
    },
    [handleFileDrop],
  )

  const handleEmojiSelect = useCallback(
    (emoji: string) => {
      const el = textareaRef.current
      if (el) {
        const start = el.selectionStart ?? inputValue.length
        const end = el.selectionEnd ?? inputValue.length
        const next = inputValue.slice(0, start) + emoji + inputValue.slice(end)
        setInputValue(next)
        requestAnimationFrame(() => {
          const pos = start + emoji.length
          el.focus()
          el.setSelectionRange(pos, pos)
        })
      } else {
        setInputValue((prev) => prev + emoji)
      }
    },
    [inputValue, setInputValue],
  )

  useEffect(() => {
    if (!showEmojiPicker) return
    const onPointerDown = (e: MouseEvent | TouchEvent) => {
      if (
        pickerRef.current &&
        !pickerRef.current.contains(e.target as Node) &&
        !(e.target as HTMLElement).closest('[data-emoji-toggle]')
      ) {
        setShowEmojiPicker(false)
      }
    }
    const onKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Escape') setShowEmojiPicker(false)
    }
    document.addEventListener('mousedown', onPointerDown)
    document.addEventListener('touchstart', onPointerDown)
    document.addEventListener('keydown', onKeyDown)
    return () => {
      document.removeEventListener('mousedown', onPointerDown)
      document.removeEventListener('touchstart', onPointerDown)
      document.removeEventListener('keydown', onKeyDown)
    }
  }, [showEmojiPicker])

  useEffect(() => {
    if (!chatId) setShowEmojiPicker(false)
  }, [chatId])

  return (
    <main
      className={styles.chat}
      onDragEnter={onDragEnter}
      onDragLeave={onDragLeave}
      onDragOver={onDragOver}
      onDrop={onDrop}
      onPaste={onPaste}
    >
      <header
        className={styles.chatHeader}
        onClick={() => chatId && setShowInfoPanel(true)}
      >
        {chatId ? (
          <AvatarView
            name={contactName ?? `Contato ${chatId}`}
            size={36}
            src={contactProfilePicUrl}
          />
        ) : (
          <AvatarView name="?" size={36} />
        )}
        <div
          className={styles.data}
          style={{ cursor: chatId ? 'pointer' : undefined }}
        >
          <strong>
            {chatId ? (contactName ?? clientName) : 'Nenhum contato'}
          </strong>
          <small>
            {chatId ? (clientName ?? 'Online') : 'Selecione um contato'}
          </small>
        </div>
        {chatId && !chatContactId && (
          <button
            className={styles.saveContactBtn}
            onClick={() =>
              saveContact.openModal(phoneNumber ?? '', contactName ?? '')
            }
          >
            <UserPlus size={15} />
            Salvar em contatos
          </button>
        )}
        {chatId && canOpenOccurrence && (
          <button className={styles.occBtn} onClick={occurrence.openModal}>
            <FileWarning size={15} />
            Abrir Ocorrência
          </button>
        )}
        {chatId && canMarkRead && (
          <button className={styles.finishBtn} onClick={onFinishAttendance}>
            <LogOut size={15} />
            Finalizar Atendimento
          </button>
        )}
      </header>
      <section
        className={styles.messages}
        ref={messagesRef}
        onScroll={handleMessagesScroll}
      >
        {sendingCount > 0 &&
          Array.from({ length: sendingCount }).map((_, i) => (
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
            ) : (
              'Selecione um contato para ver as mensagens'
            )}
          </div>
        ) : (
          items.map(({ msg, prev }) => {
            const showDate = shouldShowDateSeparator(msg, prev)
            return (
              <div key={msg.id}>
                {showDate && (
                  <div className={styles.dateSeparator}>
                    <span>{formatDateSeparator(msg.sentAt)}</span>
                  </div>
                )}
                <div
                  className={
                    msg.direction === 0 ? styles.received : styles.sent
                  }
                >
                  <div className={styles.messageRow}>
                    <div
                      className={
                        msg.mediaUrl && !msg.body
                          ? `${styles.bubble} ${styles.bubbleMediaOnly}`
                          : styles.bubble
                      }
                    >
                      {msg.isGroup &&
                        msg.direction === 0 &&
                        msg.authorName && (
                          <span className={styles.author}>
                            {msg.authorName}
                          </span>
                        )}
                      {msg.mediaUrl || isContactType(msg.type) ? (
                        <MessageMedia
                          msg={msg}
                          onStartChat={onStartChat}
                          onImageClick={handleImageClick}
                        />
                      ) : null}

                      {!isContactType(msg.type) && msg.body && (
                        <div>{formatMessageText(msg.body)}</div>
                      )}
                    </div>
                    <div className={styles.messageMeta}>
                      <span className={styles.timestamp}>
                        {formatTime(msg.sentAt)}
                      </span>
                      {msg.direction === 1 ? (
                        <StatusTicks status={msg.deliveryStatus} />
                      ) : (
                        ''
                      )}
                    </div>
                  </div>
                </div>
              </div>
            )
          })
        )}
        {loadingMore && (
          <div className={styles.loadingOlder}>
            <span className="spinner spinnerDark" />
            Carregando mensagens...
          </div>
        )}
      </section>
      {mediaPreview && (
        <div className={styles.mediaPreviewBar}>
          <div className={styles.mediaPreviewContent}>
            {mediaType === 'Image' || mediaType === 'Sticker' ? (
              <img
                src={mediaPreview}
                alt="Preview"
                className={styles.mediaPreviewImg}
              />
            ) : mediaType === 'Video' ? (
              <video src={mediaPreview} className={styles.mediaPreviewImg} />
            ) : mediaType === 'Audio' ? (
              <Music size={20} />
            ) : (
              <FileText size={20} />
            )}
            <span className={styles.mediaPreviewName}>
              {selectedFile?.name}
            </span>
            <button className={styles.mediaPreviewClear} onClick={clearMedia}>
              <X size={14} />
            </button>
          </div>
        </div>
      )}
      {chatId != null && (
        <footer className={styles.inputArea}>
          {showEmojiPicker && (
            <EmojiPicker
              pickerRef={pickerRef}
              onSelect={handleEmojiSelect}
              onClose={() => setShowEmojiPicker(false)}
            />
          )}
          <button
            data-emoji-toggle
            className={showEmojiPicker ? styles.emojiToggleActive : undefined}
            onClick={() => setShowEmojiPicker((v) => !v)}
            title="Emojis"
          >
            <Smile size={20} />
          </button>
          <input
            type="file"
            ref={fileInputRef}
            onChange={handleFileSelect}
            accept="image/*,video/*,audio/*,.pdf,.doc,.docx,.txt"
            style={{ display: 'none' }}
          />
          <button onClick={() => fileInputRef.current?.click()}>
            <Paperclip size={20} />
          </button>
          <textarea
            ref={textareaRef}
            placeholder="Digite uma mensagem..."
            rows={1}
            value={inputValue}
            onChange={(e) => setInputValue(e.target.value)}
            onKeyDown={(e) => {
              if (e.key === 'Enter' && !e.ctrlKey && !e.shiftKey) {
                e.preventDefault()
                sendMessage()
              }
            }}
            onInput={(e) => {
              const el = e.currentTarget
              el.style.height = 'auto'
              el.style.height = Math.min(el.scrollHeight, 120) + 'px'
            }}
            style={{ resize: 'none' }}
          />
          <button
            className={styles.send}
            onClick={sendMessage}
            disabled={sendingCount > 0 || (!inputValue.trim() && !selectedFile)}
          >
            <Send size={17} />
          </button>
        </footer>
      )}
      {isDragging && (
        <div className={styles.dropOverlay}>
          <div className={styles.dropContent}>
            <Upload size={40} />
            <span>Solte o arquivo aqui</span>
          </div>
        </div>
      )}
      {lightboxSrc && (
        <Lightbox
          src={lightboxSrc}
          alt={lightboxAlt}
          onClose={() => setLightboxSrc(null)}
        />
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
          phoneNumber={phoneNumber ?? ''}
          jid={jid}
          onClose={() => setShowInfoPanel(false)}
          onMerged={(result) => {
            setShowInfoPanel(false)
            onMerged?.(result)
          }}
        />
      )}
    </main>
  )
}
