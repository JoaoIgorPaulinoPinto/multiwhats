
import { useEffect, useCallback } from "react"
import { X } from "lucide-react"
import styles from "./lightbox.module.css"

interface Props {
  src: string
  alt?: string
  onClose: () => void
}

export function Lightbox({ src, alt, onClose }: Props) {
  const handleKeyDown = useCallback((e: KeyboardEvent) => {
    if (e.key === "Escape") onClose()
  }, [onClose])

  useEffect(() => {
    document.addEventListener("keydown", handleKeyDown)
    return () => document.removeEventListener("keydown", handleKeyDown)
  }, [handleKeyDown])

  return (
    <div className={styles.overlay} onClick={onClose}>
      <button className={styles.closeBtn} onClick={onClose}>
        <X size={20} />
      </button>
      <div className={styles.content} onClick={(e) => e.stopPropagation()}>
        <img src={src} alt={alt ?? ""} />
      </div>
    </div>
  )
}
