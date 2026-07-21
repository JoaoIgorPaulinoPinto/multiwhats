"use client"

import { createContext, useCallback, useContext, useRef, useState } from "react"
import { CheckCircle, AlertCircle, Info } from "lucide-react"
import styles from "./toast.module.css"

interface Toast {
  id: number
  message: string
  type: "success" | "error" | "info"
  exiting: boolean
}

interface ToastContextValue {
  toast: {
    success: (message: string) => void
    error: (message: string) => void
    info: (message: string) => void
  }
}

const ToastContext = createContext<ToastContextValue | null>(null)

export function useToast(): ToastContextValue {
  const ctx = useContext(ToastContext)
  if (!ctx) throw new Error("useToast must be used within ToastProvider")
  return ctx
}

const ICONS = {
  success: <CheckCircle size={18} />,
  error: <AlertCircle size={18} />,
  info: <Info size={18} />,
}

const DURATION = 3500

export function ToastProvider({ children }: { children: React.ReactNode }) {
  const [toasts, setToasts] = useState<Toast[]>([])
  const idRef = useRef(0)

  const remove = useCallback((id: number) => {
    setToasts((prev) =>
      prev.map((t) => (t.id === id ? { ...t, exiting: true } : t))
    )
    setTimeout(() => {
      setToasts((prev) => prev.filter((t) => t.id !== id))
    }, 250)
  }, [])

  const show = useCallback(
    (message: string, type: Toast["type"]) => {
      const id = ++idRef.current
      setToasts((prev) => [...prev, { id, message, type, exiting: false }])
      setTimeout(() => remove(id), DURATION)
    },
    [remove]
  )

  const toast = {
    success: (msg: string) => show(msg, "success"),
    error: (msg: string) => show(msg, "error"),
    info: (msg: string) => show(msg, "info"),
  }

  return (
    <ToastContext.Provider value={{ toast }}>
      {children}
      <div className={styles.container}>
        {toasts.map((t) => (
          <div
            key={t.id}
            className={`${styles.toast} ${styles[t.type]} ${t.exiting ? styles.toastExit : ""}`}
          >
            <span className={styles.icon}>{ICONS[t.type]}</span>
            {t.message}
          </div>
        ))}
      </div>
    </ToastContext.Provider>
  )
}
