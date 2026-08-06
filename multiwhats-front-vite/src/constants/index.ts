import type { OccurrenceStatus, Priority } from "../types"

export const PRIORITY_COLORS: Record<Priority, string> = {
  0: "#6b7280",
  1: "#d97706",
  2: "#ea580c",
  3: "#dc2626",
}

export const PRIORITY_LABELS: Record<Priority, string> = {
  0: "Baixa",
  1: "Média",
  2: "Alta",
  3: "Urgente",
}

export const OCCURRENCE_STATUS_LABELS: Record<OccurrenceStatus, string> = {
  Open: "Aberta",
  InProgress: "Em andamento",
  Resolved: "Resolvida",
  Closed: "Fechada",
}

export const OCCURRENCE_STATUS_COLORS: Record<OccurrenceStatus, string> = {
  Open: "#d97706",
  InProgress: "#2563eb",
  Resolved: "#16a34a",
  Closed: "#6b7280",
}

export const OCCURRENCE_STATUS_OPTIONS: {
  value: OccurrenceStatus
  label: string
  next?: OccurrenceStatus
  nextLabel?: string
  prev?: OccurrenceStatus
  prevLabel?: string
}[] = [
  { value: "Open", label: "Aberto", next: "InProgress", nextLabel: "Iniciar" },
  { value: "InProgress", label: "Em andamento", next: "Resolved", nextLabel: "Resolver", prev: "Open", prevLabel: "Voltar p/ Aberto" },
  { value: "Resolved", label: "Resolvido", next: "Closed", nextLabel: "Fechar", prev: "InProgress", prevLabel: "Voltar p/ Em andamento" },
  { value: "Closed", label: "Fechado", prev: "Resolved", prevLabel: "Voltar p/ Resolvido" },
]

export const OCCURRENCE_STATUS_INT: Record<OccurrenceStatus, number> = {
  Open: 0,
  InProgress: 1,
  Resolved: 2,
  Closed: 3,
}
