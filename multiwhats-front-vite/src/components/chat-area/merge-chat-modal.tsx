
import { useEffect, useMemo, useState } from "react"
import { Search } from "lucide-react"
import { Modal, ModalField } from "../modal/modal.view"
import { useToast } from "../toast/toast.provider"
import { chatsService, type ChatListResponse } from "../../services/chats.service"
import { AvatarView } from "../avatar/avatar.view"
import styles from "./merge-chat-modal.module.css"

type MergeDirection = "keepCurrent" | "intoTarget"

interface MergeChatModalProps {
  current: { id: number; name: string; jid: string }
  onClose: () => void
  onMerged: (result: { survivorJid: string; survivor?: ChatListResponse }) => void
}

function chatDisplayName(chat: ChatListResponse): string {
  return (
    chat.contactName ??
    chat.name ??
    chat.phoneNumber ??
    `Chat #${chat.id}`
  )
}

export function MergeChatModal({ current, onClose, onMerged }: MergeChatModalProps) {
  const { toast } = useToast()
  const [chats, setChats] = useState<ChatListResponse[]>([])
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [search, setSearch] = useState("")
  const [targetJid, setTargetJid] = useState<string | null>(null)
  const [direction, setDirection] = useState<MergeDirection>("keepCurrent")

  useEffect(() => {
    chatsService
      .listAllChats()
      .then((res) => {
        setChats(res.items.filter((c) => c.jid !== current.jid))
      })
      .catch(() => toast.error("Erro ao carregar conversas"))
      .finally(() => setLoading(false))
  }, [current.jid, toast])

  const filtered = useMemo(() => {
    const q = search.toLowerCase().trim()
    if (!q) return chats
    return chats.filter((c) => chatDisplayName(c).toLowerCase().includes(q))
  }, [chats, search])

  const target = targetJid ? chats.find((c) => c.jid === targetJid) : undefined

  async function handleConfirm() {
    if (!target) return
    setSaving(true)
    setError(null)
    try {
      const mergeJid = direction === "intoTarget" ? current.jid : target.jid
      const toJid = direction === "intoTarget" ? target.jid : current.jid
      await chatsService.mergeChats(mergeJid, toJid)
      toast.success("Conversas mescladas com sucesso.")
      onMerged(
        direction === "intoTarget"
          ? { survivorJid: target.jid, survivor: target }
          : { survivorJid: current.jid },
      )
      onClose()
    } catch (e) {
      const message = e instanceof Error ? e.message : "Erro ao mesclar conversas"
      setError(message)
      toast.error(message)
    } finally {
      setSaving(false)
    }
  }

  return (
    <Modal title="Mesclar conversas" onClose={onClose} error={error}>
      {loading ? (
        <p className={styles.loading}>
          <span className="spinner spinnerDark" /> Carregando conversas...
        </p>
      ) : (
        <>
          <ModalField label="Escolha a outra conversa">
            <div className={styles.searchBox}>
              <Search size={14} />
              <input
                value={search}
                onChange={(e) => setSearch(e.target.value)}
                placeholder="Buscar conversa..."
                autoFocus
              />
            </div>
            <div className={styles.chatList}>
              {filtered.length === 0 ? (
                <p className={styles.empty}>Nenhuma conversa encontrada.</p>
              ) : (
                filtered.map((chat) => {
                  const name = chatDisplayName(chat)
                  const selected = chat.jid === targetJid
                  return (
                    <button
                      key={chat.id}
                      type="button"
                      className={`${styles.chatItem} ${selected ? styles.chatItemActive : ""}`}
                      onClick={() => setTargetJid(chat.jid)}
                    >
                      <AvatarView name={name} size={30} />
                      <div className={styles.chatInfo}>
                        <strong className={styles.chatName}>{name}</strong>
                        <span className={styles.chatPhone}>
                          {chat.phoneNumber ?? ""}
                        </span>
                      </div>
                    </button>
                  )
                })
              )}
            </div>
          </ModalField>

          {target && (
            <div className={styles.direction}>
              <label className={styles.directionLabel}>
                <input
                  type="radio"
                  name="direction"
                  checked={direction === "keepCurrent"}
                  onChange={() => setDirection("keepCurrent")}
                />
                <span>
                  Mesclar <strong>{chatDisplayName(target)}</strong> dentro de{" "}
                  <strong>{current.name}</strong>
                </span>
              </label>
              <label className={styles.directionLabel}>
                <input
                  type="radio"
                  name="direction"
                  checked={direction === "intoTarget"}
                  onChange={() => setDirection("intoTarget")}
                />
                <span>
                  Mesclar <strong>{current.name}</strong> dentro de{" "}
                  <strong>{chatDisplayName(target)}</strong>
                </span>
              </label>
            </div>
          )}
        </>
      )}

      <div className={styles.actions}>
        <button className={styles.cancelBtn} onClick={onClose} disabled={saving}>
          Cancelar
        </button>
        <button
          className={styles.saveBtn}
          onClick={handleConfirm}
          disabled={!target || saving}
        >
          {saving ? "Mesclando..." : "Mesclar"}
        </button>
      </div>
    </Modal>
  )
}
