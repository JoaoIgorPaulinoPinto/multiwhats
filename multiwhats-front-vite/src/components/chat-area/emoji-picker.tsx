import { useMemo, useRef, useState } from 'react'
import styles from './emoji-picker.module.css'

const EMOJI_GROUPS: { category: string; emojis: string[] }[] = [
  {
    category: 'Smileys',
    emojis: [
      '😀', '😁', '😂', '🤣', '😃', '😄', '😅', '😆', '😉', '😊',
      '😋', '😎', '😍', '😘', '🥰', '😗', '😙', '😚', '🙂', '🤗',
      '🤩', '🤔', '🤨', '😐', '😑', '😶', '😏', '😒', '🙄', '😬',
      '🤥', '😌', '😔', '😪', '🤤', '😴', '😷', '🤒', '🤕', '🤢',
      '🤮', '🤧', '🥵', '🥶', '🥴', '😵', '🤯', '🤠', '🥳', '😎',
      '😢', '😭', '😤', '😠', '😡', '🤬', '🤫', '🤭', '🫢', '🫣',
      '🤗', '😇', '🥱', '😬', '🙃', '🫠', '🥺', '😳', '😱', '😨',
    ],
  },
  {
    category: 'Gestos',
    emojis: [
      '👍', '👎', '👌', '✌️', '🤞', '🤟', '🤘', '🤙', '👈', '👉',
      '👆', '👇', '☝️', '✋', '🤚', '🖐️', '🖖', '👋', '🤝', '✍️',
      '💪', '🦾', '🖕', '🙏', '👏', '🙌', '🤲', '🤏', '👐', '💅',
    ],
  },
  {
    category: 'Corações',
    emojis: [
      '❤️', '🧡', '💛', '💚', '💙', '💜', '🖤', '🤍', '🤎', '💔',
      '❣️', '💕', '💞', '💓', '💗', '💖', '💘', '💝', '💟', '♥️',
      '💌', '💯', '💢', '💥', '💫', '💦', '💨', '🫶',
    ],
  },
  {
    category: 'Animais',
    emojis: [
      '🐶', '🐱', '🐭', '🐹', '🐰', '🦊', '🐻', '🐼', '🐨', '🐯',
      '🦁', '🐮', '🐷', '🐸', '🐵', '🐔', '🐧', '🐦', '🦆', '🦅',
      '🦉', '🦇', '🐺', '🐗', '🐴', '🦄', '🐝', '🐛', '🦋', '🐌',
      '🐞', '🐢', '🐍', '🦎', '🐙', '🦑', '🦐', '🦀', '🐡', '🐠',
      '🐬', '🐳', '🐋', '🦈', '🐊', '🐅', '🐆', '🦓', '🦍', '🐘',
    ],
  },
  {
    category: 'Comida',
    emojis: [
      '🍏', '🍎', '🍐', '🍊', '🍋', '🍌', '🍉', '🍇', '🍓', '🫐',
      '🍈', '🍒', '🍑', '🥭', '🍍', '🥥', '🥝', '🍅', '🥑', '🥦',
      '🥬', '🥒', '🌽', '🥕', '🧄', '🧅', '🥔', '🍠', '🥐', '🥯',
      '🍞', '🥖', '🥨', '🧀', '🥚', '🍳', '🥞', '🧇', '🥓', '🥩',
      '🍗', '🍖', '🌭', '🍔', '🍟', '🍕', '🥪', '🥙', '🌮', '🌯',
      '🍜', '🍝', '🍣', '🍤', '🍚', '🍙', '🍢', '🍡', '🍧', '🍨',
      '🍦', '🍰', '🎂', '🍮', '🍭', '🍬', '🍫', '🍿', '🍩', '🍪',
      '☕', '🍵', '🥤', '🍺', '🍻', '🥂', '🍷', '🥃', '🍹', '🧊',
    ],
  },
  {
    category: 'Símbolos',
    emojis: [
      '✅', '❌', '⭕', '❗', '❓', '💯', '🔴', '🟠', '🟡', '🟢',
      '🔵', '🟣', '⚪', '🟤', '⚫', '🔺', '🔻', '🔸', '🔹', '🔶',
      '🔷', '⭐', '🌟', '✨', '⚡', '🔥', '💧', '🌊', '🌈', '☀️',
      '🌙', '☁️', '⛅', '❄️', '💥', '💫', '🎉', '🎊', '🎈', '🎁',
      '🏆', '🥇', '🥈', '🥉', '🎯', '🚀', '✈️', '🛒', '💰', '💎',
    ],
  },
]

const RECENT_KEY = 'multiwhats:recent-emojis'

function getRecent(): string[] {
  try {
    return JSON.parse(localStorage.getItem(RECENT_KEY) ?? '[]')
  } catch {
    return []
  }
}

function saveRecent(emojis: string[]) {
  try {
    localStorage.setItem(RECENT_KEY, JSON.stringify(emojis.slice(0, 24)))
  } catch {
    /* noop */
  }
}

interface Props {
  onSelect: (emoji: string) => void
  onClose: () => void
  pickerRef: React.RefObject<HTMLDivElement | null>
}

export function EmojiPicker({ onSelect, onClose, pickerRef }: Props) {
  const [activeGroup, setActiveGroup] = useState(0)
  const [recent] = useState<string[]>(getRecent)
  const [search, setSearch] = useState('')
  const inputRef = useRef<HTMLInputElement>(null)

  const filtered = useMemo(() => {
    if (!search.trim()) return null
    const q = search.trim().toLowerCase()
    const result: string[] = []
    for (const group of EMOJI_GROUPS) {
      for (const emoji of group.emojis) {
        if (emoji.toLowerCase().includes(q) && !result.includes(emoji)) {
          result.push(emoji)
        }
      }
    }
    return result
  }, [search])

  const pick = (emoji: string) => {
    const next = [emoji, ...recent.filter((e) => e !== emoji)]
    saveRecent(next)
    onSelect(emoji)
  }

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'Escape') onClose()
  }

  return (
    <div
      ref={pickerRef}
      className={styles.picker}
      onKeyDown={handleKeyDown}
      onClick={(e) => e.stopPropagation()}
    >
      <input
        ref={inputRef}
        className={styles.search}
        placeholder="Buscar emoji..."
        value={search}
        onChange={(e) => setSearch(e.target.value)}
      />

      <div className={styles.body}>
        {filtered ? (
          filtered.length > 0 ? (
            <div className={styles.grid}>
              {filtered.map((emoji) => (
                <button
                  key={emoji}
                  type="button"
                  className={styles.emojiBtn}
                  onClick={() => pick(emoji)}
                >
                  {emoji}
                </button>
              ))}
            </div>
          ) : (
            <div className={styles.empty}>Nenhum emoji encontrado</div>
          )
        ) : (
          <>
            {recent.length > 0 && (
              <>
                <div className={styles.category}>Recentes</div>
                <div className={styles.grid}>
                  {recent.map((emoji) => (
                    <button
                      key={emoji}
                      type="button"
                      className={styles.emojiBtn}
                      onClick={() => pick(emoji)}
                    >
                      {emoji}
                    </button>
                  ))}
                </div>
              </>
            )}
            <div className={styles.category}>
              {EMOJI_GROUPS[activeGroup].category}
            </div>
            <div className={styles.grid}>
              {EMOJI_GROUPS[activeGroup].emojis.map((emoji) => (
                <button
                  key={emoji}
                  type="button"
                  className={styles.emojiBtn}
                  onClick={() => pick(emoji)}
                >
                  {emoji}
                </button>
              ))}
            </div>
          </>
        )}
      </div>

      {!filtered && (
        <div className={styles.tabs}>
          {EMOJI_GROUPS.map((group, i) => (
            <button
              key={group.category}
              type="button"
              className={i === activeGroup ? styles.tabActive : styles.tab}
              onClick={() => setActiveGroup(i)}
            >
              {group.emojis[0]}
            </button>
          ))}
        </div>
      )}
    </div>
  )
}
