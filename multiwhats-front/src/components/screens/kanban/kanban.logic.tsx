"use client"

import { useEffect, useState } from "react"
import { kanbanService, type TaskResponse, type OccurrenceResponse } from "../../../services/kanban.service"

export interface KanbanCard {
  id: number
  title: string
  subtitle: string
  type: "task" | "occurrence"
  status: string
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

function buildColumns(tasks: TaskResponse[], occurrences: OccurrenceResponse[]): KanbanColumn[] {
  return ["todo", "progress", "done"].map((id) => {
    const taskCards: KanbanCard[] = tasks
      .filter((t) => TASK_COLUMNS[t.status] === id)
      .map((t) => ({
        id: t.id,
        title: t.title,
        subtitle: t.clientName ?? `Prioridade: ${t.priority}`,
        type: "task" as const,
        status: t.status,
      }))

    const occCards: KanbanCard[] = occurrences
      .filter((o) => OCC_COLUMNS[o.status] === id)
      .map((o) => ({
        id: o.id,
        title: o.title,
        subtitle: o.chatName ?? `Prioridade: ${o.priority}`,
        type: "occurrence" as const,
        status: o.status,
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

  useEffect(() => {
    console.log(`[Kanban] carregando tarefas e ocorrências...`)
    Promise.all([
      kanbanService.listTasks(),
      kanbanService.listOccurrences(),
    ])
      .then(([t, o]) => {
        console.log(`[Kanban] ${t.length} tarefas, ${o.length} ocorrências carregadas`)
        setTasks(t)
        setOccurrences(o)
      })
      .catch((e) => console.error(`[Kanban] erro ao carregar:`, e))
      .finally(() => setLoading(false))
  }, [])

  const columns = buildColumns(tasks, occurrences)

  return { columns, loading }
}
