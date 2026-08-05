"use client"

import { useEffect } from "react"

export default function Error({
  error,
  reset,
}: {
  error: Error & { digest?: string }
  reset: () => void
}) {
  useEffect(() => {
    console.error("[Page] erro de renderização:", error)
  }, [error])

  return (
    <div
      style={{
        display: "grid",
        placeItems: "center",
        minHeight: "100vh",
        padding: 24,
        textAlign: "center",
        fontFamily: "system-ui, sans-serif",
        background: "#121212",
        color: "#f5f5f5",
      }}
    >
      <div>
        <h1>Algo deu errado</h1>
        <p>Não foi possível carregar esta página.</p>
        <button
          onClick={() => reset()}
          style={{
            marginTop: 16,
            padding: "10px 18px",
            borderRadius: 8,
            border: "none",
            background: "#00a884",
            color: "#fff",
            cursor: "pointer",
            fontSize: 14,
          }}
        >
          Tentar novamente
        </button>
      </div>
    </div>
  )
}
