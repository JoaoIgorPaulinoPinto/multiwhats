
import { useEffect, useRef, useState } from "react"
import { transformToJpegQueued } from "./image-transform"

const canIntersectObserve = () =>
  typeof window !== "undefined" && "IntersectionObserver" in window

export function useTransformedImage(rawBase64: string | null, mime: string | null) {
  const [src, setSrc] = useState<string | null>(null)
  const [loading, setLoading] = useState(() => !!rawBase64)
  const [visible, setVisible] = useState(() => !canIntersectObserve())
  const ref = useRef<HTMLDivElement | null>(null)

  useEffect(() => {
    if (!rawBase64 || visible || !canIntersectObserve()) return
    const node = ref.current
    if (!node) return
    const observer = new IntersectionObserver(
      (entries) => {
        if (entries.some((entry) => entry.isIntersecting)) {
          setVisible(true)
          observer.disconnect()
        }
      },
      { rootMargin: "200px" },
    )
    observer.observe(node)
    return () => observer.disconnect()
  }, [rawBase64, visible])

  useEffect(() => {
    if (!rawBase64 || !visible) return

    let cancelled = false

    transformToJpegQueued(rawBase64, mime)
      .then((result) => {
        if (!cancelled) setSrc(result)
      })
      .catch(() => {
        if (!cancelled) {
          const fallback = rawBase64.startsWith("data:")
            ? rawBase64
            : `data:${mime || "image/jpeg"};base64,${rawBase64}`
          setSrc(fallback)
        }
      })
      .finally(() => {
        if (!cancelled) setLoading(false)
      })

    return () => { cancelled = true }
  }, [rawBase64, mime, visible])

  return { src, loading, ref }
}
