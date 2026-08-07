
import { useCallback, useEffect, useRef, useState } from "react"
import { ZoomIn, ZoomOut, X, RotateCcw } from "lucide-react"
import styles from "./lightbox.module.css"

interface Props {
  src: string
  alt?: string
  onClose: () => void
}

const MIN_SCALE = 0.5
const MAX_SCALE = 5

export function Lightbox({ src, alt, onClose }: Props) {
  const [scale, setScale] = useState(1)
  const [translate, setTranslate] = useState({ x: 0, y: 0 })
  const [dragging, setDragging] = useState(false)
  const [transition, setTransition] = useState(true)
  const dragStart = useRef<{ x: number; y: number; tx: number; ty: number } | null>(null)

  const clampScale = (value: number) => Math.min(MAX_SCALE, Math.max(MIN_SCALE, value))

  const handleKeyDown = useCallback(
    (e: KeyboardEvent) => {
      if (e.key === "Escape") onClose()
      if (e.key === "=" || e.key === "+") {
        e.preventDefault()
        setScale((s) => clampScale(s * 1.25))
        setTranslate({ x: 0, y: 0 })
      }
      if (e.key === "-") {
        e.preventDefault()
        setScale((s) => clampScale(s / 1.25))
        setTranslate({ x: 0, y: 0 })
      }
      if (e.key === "0") {
        e.preventDefault()
        setScale(1)
        setTranslate({ x: 0, y: 0 })
      }
    },
    [onClose],
  )

  useEffect(() => {
    document.addEventListener("keydown", handleKeyDown)
    return () => document.removeEventListener("keydown", handleKeyDown)
  }, [handleKeyDown])

  const zoomBy = (factor: number) => {
    setScale((s) => clampScale(s * factor))
    setTranslate({ x: 0, y: 0 })
  }

  const reset = () => {
    setScale(1)
    setTranslate({ x: 0, y: 0 })
  }

  const handleWheel = (e: React.WheelEvent) => {
    e.preventDefault()
    const factor = e.deltaY < 0 ? 1.15 : 1 / 1.15
    setScale((s) => clampScale(s * factor))
    setTranslate({ x: 0, y: 0 })
  }

  const handleDoubleClick = () => {
    if (scale > 1.01) reset()
    else setScale(2)
    setTransition(true)
  }

  const handleMouseDown = (e: React.MouseEvent) => {
    if (scale <= 1.01) return
    e.preventDefault()
    setDragging(true)
    setTransition(false)
    dragStart.current = { x: e.clientX, y: e.clientY, tx: translate.x, ty: translate.y }
  }

  const handleMouseMove = (e: React.MouseEvent) => {
    if (!dragging || !dragStart.current) return
    const dx = e.clientX - dragStart.current.x
    const dy = e.clientY - dragStart.current.y
    setTranslate({ x: dragStart.current.tx + dx, y: dragStart.current.ty + dy })
  }

  const stopDrag = () => {
    setDragging(false)
    dragStart.current = null
    setTransition(true)
  }

  return (
    <div className={styles.overlay} onClick={onClose}>
      <button className={styles.closeBtn} onClick={onClose}>
        <X size={20} />
      </button>
      <div
        className={styles.stage}
        onClick={(e) => e.stopPropagation()}
        onWheel={handleWheel}
        onMouseDown={handleMouseDown}
        onMouseMove={handleMouseMove}
        onMouseUp={stopDrag}
        onMouseLeave={stopDrag}
        onDoubleClick={handleDoubleClick}
      >
        <img
          src={src}
          alt={alt ?? ""}
          className={styles.img}
          draggable={false}
          style={{
            transform: `translate(${translate.x}px, ${translate.y}px) scale(${scale})`,
            cursor: scale > 1.01 ? (dragging ? "grabbing" : "grab") : "default",
            transition: transition ? "transform 0.15s ease" : "none",
          }}
        />
      </div>
      <div className={styles.toolbar} onClick={(e) => e.stopPropagation()}>
        <button onClick={() => zoomBy(1 / 1.25)} title="Diminuir zoom">
          <ZoomOut size={18} />
        </button>
        <span className={styles.zoomLabel}>{Math.round(scale * 100)}%</span>
        <button onClick={() => zoomBy(1.25)} title="Aumentar zoom">
          <ZoomIn size={18} />
        </button>
        <button onClick={reset} title="Redefinir (0)">
          <RotateCcw size={18} />
        </button>
      </div>
    </div>
  )
}
