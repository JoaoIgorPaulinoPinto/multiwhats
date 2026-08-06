import { useEffect } from "react"

const CHUNK_ERROR_PATTERNS = [
  "Loading chunk",
  "Failed to fetch dynamically imported module",
  "ChunkLoadError",
  "Importing a module script failed",
]

// Recupera automaticamente de falhas de carregamento de chunk (ex.: deploy
// novo com hash de bundle antigo) recarregando a página uma única vez.
let reloadedOnce = false

function isChunkError(err: unknown): boolean {
  const msg = err instanceof Error ? `${err.name}: ${err.message}` : String(err)
  return CHUNK_ERROR_PATTERNS.some((p) => msg.includes(p))
}

function recoverFromChunkError() {
  if (reloadedOnce) return
  reloadedOnce = true
  window.location.reload()
}

export function ErrorCatcher() {
  useEffect(() => {
    function handleRejection(event: PromiseRejectionEvent) {
      if (isChunkError(event.reason)) recoverFromChunkError()
    }

    function handleError(event: ErrorEvent) {
      if (isChunkError(event.error)) recoverFromChunkError()
    }

    window.addEventListener("unhandledrejection", handleRejection)
    window.addEventListener("error", handleError)
    return () => {
      window.removeEventListener("unhandledrejection", handleRejection)
      window.removeEventListener("error", handleError)
    }
  }, [])

  return null
}
