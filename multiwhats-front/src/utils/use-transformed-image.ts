"use client"

import { useState, useEffect } from "react"
import { transformToJpeg } from "./image-transform"

export function useTransformedImage(rawBase64: string | null, mime: string | null) {
  const [src, setSrc] = useState<string | null>(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    if (!rawBase64) {
      setLoading(false)
      return
    }

    let cancelled = false

    transformToJpeg(rawBase64, mime)
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
  }, [rawBase64, mime])

  return { src, loading }
}
