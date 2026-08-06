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

export function formatRelativeTime(seconds: number): string {
  if (!seconds || seconds < 60) return "agora mesmo"
  const minutes = Math.floor(seconds / 60)
  if (minutes < 60) return `${minutes} min atrás`
  const hours = Math.floor(minutes / 60)
  if (hours < 24) return `${hours} h atrás`
  const days = Math.floor(hours / 24)
  if (days < 30) return `${days} ${days === 1 ? "dia" : "dias"} atrás`
  const months = Math.floor(days / 30)
  if (months < 12) return `${months} ${months === 1 ? "mês" : "meses"} atrás`
  const years = Math.floor(months / 12)
  return `${years} ${years === 1 ? "ano" : "anos"} atrás`
}

export function formatRelativeToNow(dateStr: string): string {
  if (!dateStr) return ""
  const date = new Date(dateStr)
  if (isNaN(date.getTime())) return ""
  return formatRelativeTime(Math.max(0, (Date.now() - date.getTime()) / 1000))
}
