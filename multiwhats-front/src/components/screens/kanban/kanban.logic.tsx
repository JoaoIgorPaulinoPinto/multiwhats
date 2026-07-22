"use client"

import { useEffect, useState, useCallback } from "react"
import { kanbanService, type TaskResponse, type OccurrenceResponse, type Priority, type OccurrenceStatus } from "../../../services/kanban.service"

export interface KanbanCard {
  id: number
  title: string
  subtitle: string
  type: "task" | "occurrence"
  status: string
  priority: number
  description: string | null
  assignedToName: string | null
  createdAt: string
}

export interface KanbanColumn {
  id: string
  title: string
  cards: KanbanCard[]
}

const TASK_COLUMNS: Record<string, string> = {
  Open: "todo",
  InProgress: "progress",
  Completed: "done",
  Cancelled: "done",
}

const OCC_COLUMNS: Record<string, string> = {
  Open: "todo",
  InProgress: "progress",
  Resolved: "done",
  Closed: "done",
}

const COLUMN_LABELS: Record<string, string> = {
  todo: "A fazer",
  progress: "Em andamento",
  done: "Concluído",
}

const PRIORITY_DISPLAY: Record<number, string> = {
  0: "Baixa",
  1: "Média",
  2: "Alta",
  3: "Urgente",
}

function buildColumns(tasks: TaskResponse[], occurrences: OccurrenceResponse[]): KanbanColumn[] {
  return ["todo", "progress", "done"].map((id) => {
    const taskCards: KanbanCard[] = tasks
      .filter((t) => TASK_COLUMNS[t.status] === id)
      .map((t) => ({
        id: t.id,
        title: t.title,
        subtitle: t.clientName ?? `Prioridade: ${PRIORITY_DISPLAY[t.priority] ?? t.priority}`,
        type: "task" as const,
        status: t.status,
        priority: t.priority,
        description: t.description,
        assignedToName: t.assignedToName,
        createdAt: t.createdAt,
      }))

    const occCards: KanbanCard[] = occurrences
      .filter((o) => OCC_COLUMNS[o.status] === id)
      .map((o) => ({
        id: o.id,
        title: o.title,
        subtitle: o.chatName ?? `Prioridade: ${PRIORITY_DISPLAY[o.priority] ?? o.priority}`,
        type: "occurrence" as const,
        status: o.status,
        priority: o.priority,
        description: o.description,
        assignedToName: o.assignedToName,
        createdAt: o.createdAt,
      }))

    return {
      id,
      title: COLUMN_LABELS[id],
      cards: [...taskCards, ...occCards],
    }
  })
}

export function useKanban() {
  const [tasks, setTasks] = useState<TaskResponse[]>([])
  const [occurrences, setOccurrences] = useState<OccurrenceResponse[]>([])
  const [loading, setLoading] = useState(true)

  const load = useCallback(() => {
    Promise.all([
      kanbanService.listTasks(),
      kanbanService.listOccurrences(),
    ])
      .then(([t, o]) => {
        setTasks(t)
        setOccurrences(o)
      })
      .catch((e) => console.error(`[Kanban] erro ao carregar:`, e))
      .finally(() => setLoading(false))
  }, [])

  useEffect(() => {
    load()
  }, [load])

  const STATUS_TO_INT: Record<string, number> = {
  Open: 0,
  InProgress: 1,
  Resolved: 2,
  Closed: 3,
}

async function changeOccurrenceStatus(id: number, newStatus: OccurrenceStatus) {
    try {
      await kanbanService.updateOccurrence(id, { status: STATUS_TO_INT[newStatus] })
      setOccurrences((prev) =>
        prev.map((o) => (o.id === id ? { ...o, status: newStatus } : o))
      )
    } catch (e) {
      console.error(`[Kanban] erro ao atualizar ocorrência:`, e)
    }
  }

  async function deleteOccurrence(id: number) {
    try {
      await kanbanService.deleteOccurrence(id)
      setOccurrences((prev) => prev.filter((o) => o.id !== id))
    } catch (e) {
      console.error(`[Kanban] erro ao deletar ocorrência:`, e)
    }
  }

  const columns = buildColumns(tasks, occurrences)

  return { columns, loading, load, changeOccurrenceStatus, deleteOccurrence, occurrences }
}
