"use client"

import { useEffect, useState, useCallback } from "react"
import { kanbanService, type OccurrenceResponse } from "../../../services/kanban.service"
import type { OccurrenceStatus, OccurrenceStatusNumeric } from "../../../types"

const STATUS_MAP: Record<number, OccurrenceStatus> = {
  0: "Open",
  1: "InProgress",
  2: "Resolved",
  3: "Closed",
}

export interface KanbanCard {
  id: number
  title: string
  description: string | null
  status: OccurrenceStatus
  priority: number
  chatName: string | null
  assignedToName: string | null
  createdByName: string | null
  createdAt: string
  lastUpdate: string
}

export interface KanbanColumn {
  id: OccurrenceStatus
  title: string
  cards: KanbanCard[]
}

const COLUMNS_ORDER: OccurrenceStatus[] = ["Open", "InProgress", "Resolved", "Closed"]

const COLUMN_LABELS: Record<OccurrenceStatus, string> = {
  Open: "Aberto",
  InProgress: "Em andamento",
  Resolved: "Resolvido",
  Closed: "Fechado",
}

export function useKanban() {
  const [occurrences, setOccurrences] = useState<OccurrenceResponse[]>([])
  const [loading, setLoading] = useState(true)

  const load = useCallback(() => {
    kanbanService
      .listOccurrences()
      .then((data) =>
        setOccurrences(
          data.map((o) => ({
            ...o,
            status: STATUS_MAP[o.status as unknown as number] ?? o.status,
          }))
        )
      )
      .catch((e) => console.error("[Kanban] erro ao carregar:", e))
      .finally(() => setLoading(false))
  }, [])

  useEffect(() => {
    load()
  }, [load])

  async function advanceStatus(id: number, direction: "Advance" | "Return") {
    try {
      const dirNum = direction === "Advance" ? 0 : 1
      const res = await kanbanService.advanceStatus(id, dirNum)
      setOccurrences((prev) =>
        prev.map((o) =>
          o.id === id
            ? { ...o, ...res.occurrence, status: STATUS_MAP[res.occurrence.status as unknown as number] ?? res.occurrence.status }
            : o
        )
      )
    } catch (e) {
      console.error("[Kanban] erro ao avançar/retornar ocorrência:", e)
      throw e
    }
  }

  async function deleteOccurrence(id: number) {
    try {
      await kanbanService.deleteOccurrence(id)
      setOccurrences((prev) => prev.filter((o) => o.id !== id))
    } catch (e) {
      console.error("[Kanban] erro ao deletar ocorrência:", e)
      throw e
    }
  }

  const columns: KanbanColumn[] = COLUMNS_ORDER.map((status) => ({
    id: status,
    title: COLUMN_LABELS[status],
    cards: occurrences
      .filter((o) => o.status === status)
      .map((o) => ({
        id: o.id,
        title: o.title,
        description: o.description,
        status: o.status,
        priority: o.priority,
        chatName: o.chatName,
        assignedToName: o.assignedToName,
        createdByName: o.createdByName,
        createdAt: o.createdAt,
        lastUpdate: o.lastUpdate,
      })),
  }))

  return { columns, loading, load, advanceStatus, deleteOccurrence, occurrences }
}
