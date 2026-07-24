import type { MessageResponse } from "../types/chat"

export function formatTime(sentAt: string): string {
  if (!sentAt) return ""
  const date = new Date(sentAt)
  if (isNaN(date.getTime())) return ""
  return date.toLocaleTimeString("pt-BR", { hour: "2-digit", minute: "2-digit" })
}

export function formatDateSeparator(sentAt: string): string | null {
  if (!sentAt) return null
  const date = new Date(sentAt)
  if (isNaN(date.getTime())) return null
  const now = new Date()
  const diffMs = now.getTime() - date.getTime()
  const diffDays = Math.floor(diffMs / 86400000)
  if (diffDays === 0) return "Hoje"
  if (diffDays === 1) return "Ontem"
  return date.toLocaleDateString("pt-BR", { day: "2-digit", month: "2-digit", year: "numeric" })
}

export function shouldShowDateSeparator(current: MessageResponse, previous: MessageResponse | undefined): boolean {
  if (!previous) return true
  const currDate = new Date(current.sentAt).toDateString()
  const prevDate = new Date(previous.sentAt).toDateString()
  return currDate !== prevDate
}
