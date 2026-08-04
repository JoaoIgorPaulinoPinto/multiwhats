"use client"

import { useMemo, useState } from "react"

export type ReportEntityKey =
  | "chats"
  | "contacts"
  | "companies"
  | "messages"
  | "occurrences"
  | "users"

export interface ReportEntityOption {
  key: ReportEntityKey
  label: string
}

export interface ReportConfig {
  entity: ReportEntityKey
  columns: string[]
}

export interface ReportRow {
  id: number
  cells: (string | number | null)[]
  dates: Record<string, string>
}

export interface SavedReport {
  id: number
  name: string
  entity: ReportEntityKey
  limit: number
}

export type ColumnType = "text" | "number" | "date"

export type TextFilterOp = "contains" | "startsWith" | "endsWith" | "equals"

export type NumberFilterOp = "gt" | "lt" | "eq" | "between"

export interface ColumnFilter {
  type: ColumnType
  textOp: TextFilterOp
  textValue: string
  numberOp: NumberFilterOp
  numberValue: string
  numberValue2: string
  dateFrom: string
  dateTo: string
}

export const ENTITY_OPTIONS: ReportEntityOption[] = [
  { key: "chats", label: "Conversas" },
  { key: "contacts", label: "Contatos" },
  { key: "companies", label: "Empresas" },
  { key: "messages", label: "Mensagens" },
  { key: "occurrences", label: "Ocorrências" },
  { key: "users", label: "Usuários" },
]

export const ENTITY_LABELS: Record<ReportEntityKey, string> = {
  chats: "Conversas",
  contacts: "Contatos",
  companies: "Empresas",
  messages: "Mensagens",
  occurrences: "Ocorrências",
  users: "Usuários",
}

function pad(n: number): string {
  return n.toString().padStart(2, "0")
}

function atDaysAgo(days: number, hour: number, minute: number): string {
  const d = new Date()
  d.setDate(d.getDate() - days)
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(hour)}:${pad(minute)}:00`
}

export function formatDateTime(iso: string): string {
  if (!iso) return "—"
  const d = new Date(iso)
  if (Number.isNaN(d.getTime())) return "—"
  return `${pad(d.getDate())}/${pad(d.getMonth() + 1)}/${d.getFullYear()} ${pad(d.getHours())}:${pad(d.getMinutes())}`
}

const CONFIGS: Record<ReportEntityKey, ReportConfig> = {
  chats: {
    entity: "chats",
    columns: ["Chat", "Número", "Empresa", "Última mensagem", "Criada em", "Última msg", "Mensagens", "Ocorrências", "Responsável"],
  },
  contacts: {
    entity: "contacts",
    columns: ["Nome", "PushName", "Telefone", "JID", "Empresa", "Criado em"],
  },
  companies: {
    entity: "companies",
    columns: ["Empresa", "Telefone", "Status", "Contatos", "Criada em"],
  },
  messages: {
    entity: "messages",
    columns: ["Conversa", "Contato", "Direção", "Tipo", "Status", "Mensagem", "Criada em", "Enviada em"],
  },
  occurrences: {
    entity: "occurrences",
    columns: ["Título", "Conversa", "Contato", "Status", "Prioridade", "Responsável", "Criada em"],
  },
  users: {
    entity: "users",
    columns: ["Nome", "Perfil", "Status", "Criado em"],
  },
}

const MOCK_ROWS: Record<ReportEntityKey, ReportRow[]> = {
  chats: [
    { id: 1, cells: ["Atendimento Loja Centro", "5511987654321", "Timontec", "Boa tarde! Segue o boleto", null, null, 124, 2, "João Silva"], dates: { createdAt: atDaysAgo(45, 9, 12), lastMessageAt: atDaysAgo(0, 14, 30) } },
    { id: 2, cells: ["Suporte Técnico", "5511981234567", "TechSoft", "O problema foi resolvido?", null, null, 87, 4, "Maria Oliveira"], dates: { createdAt: atDaysAgo(60, 8, 5), lastMessageAt: atDaysAgo(1, 11, 2) } },
    { id: 3, cells: ["Vendas WhatsApp", "5511976543210", "Moda & Cia", "Pode fazer o envio hoje?", null, null, 210, 0, "Carlos Souza"], dates: { createdAt: atDaysAgo(30, 10, 0), lastMessageAt: atDaysAgo(2, 16, 45) } },
    { id: 4, cells: ["Financeiro", "5511988887777", "Timontec", "Boleto pago com sucesso", null, null, 56, 1, "Ana Lima"], dates: { createdAt: atDaysAgo(15, 13, 20), lastMessageAt: atDaysAgo(3, 9, 15) } },
    { id: 5, cells: ["Pós-venda", "5511993332222", "EletroMax", "Obrigado pelo retorno!", null, null, 43, 0, "João Silva"], dates: { createdAt: atDaysAgo(90, 14, 0), lastMessageAt: atDaysAgo(5, 18, 10) } },
    { id: 6, cells: ["Logística", "5511984445555", "Transporta Rápido", "Entrega agendada para amanhã", null, null, 312, 3, "Maria Oliveira"], dates: { createdAt: atDaysAgo(120, 7, 30), lastMessageAt: atDaysAgo(7, 10, 0) } },
    { id: 7, cells: ["Comercial", "5511977778888", "Papelaria Central", "Enviei a proposta", null, null, 98, 0, "Carlos Souza"], dates: { createdAt: atDaysAgo(20, 11, 0), lastMessageAt: atDaysAgo(8, 15, 40) } },
    { id: 8, cells: ["SAC", "5511981112222", "Timontec", "Abrimos uma ocorrência para você", null, null, 500, 12, "Ana Lima"], dates: { createdAt: atDaysAgo(200, 9, 0), lastMessageAt: atDaysAgo(10, 12, 0) } },
    { id: 9, cells: ["Atendimento Premium", "5511995554444", "Prime Consulting", "Vou verificar com o financeiro", null, null, 22, 1, "João Silva"], dates: { createdAt: atDaysAgo(10, 16, 0), lastMessageAt: atDaysAgo(12, 9, 30) } },
    { id: 10, cells: ["Reclamações", "5511986667777", "TechSoft", "Lamentamos o transtorno", null, null, 76, 8, "Maria Oliveira"], dates: { createdAt: atDaysAgo(75, 10, 0), lastMessageAt: atDaysAgo(15, 14, 0) } },
    { id: 11, cells: ["Novos Leads", "5511990001111", "Moda & Cia", "Gostaria de um orçamento", null, null, 5, 0, "Carlos Souza"], dates: { createdAt: atDaysAgo(3, 9, 0), lastMessageAt: atDaysAgo(1, 8, 20) } },
    { id: 12, cells: ["Cobrança", "5511971110000", "EletroMax", "Pode negociar o valor?", null, null, 34, 2, "Ana Lima"], dates: { createdAt: atDaysAgo(12, 15, 0), lastMessageAt: atDaysAgo(2, 10, 5) } },
  ],
  contacts: [
    { id: 1, cells: ["Maria Silva", "Maria", "5511987654321", "5511987654321@c.us", "Timontec", null], dates: { createdAt: atDaysAgo(40, 9, 0) } },
    { id: 2, cells: ["José Santos", "Zé", "5511981234567", "5511981234567@c.us", "TechSoft", null], dates: { createdAt: atDaysAgo(55, 10, 0) } },
    { id: 3, cells: ["Ana Costa", "Ana", "5511976543210", "5511976543210@c.us", "Moda & Cia", null], dates: { createdAt: atDaysAgo(28, 11, 0) } },
    { id: 4, cells: ["Pedro Rocha", "Pedro", "5511988887777", "5511988887777@c.us", "Timontec", null], dates: { createdAt: atDaysAgo(14, 13, 0) } },
    { id: 5, cells: ["Lucas Almeida", "—", "5511993332222", "5511993332222@c.us", "EletroMax", null], dates: { createdAt: atDaysAgo(88, 9, 0) } },
    { id: 6, cells: ["Fernanda Dias", "Fe", "5511984445555", "5511984445555@c.us", "Transporta Rápido", null], dates: { createdAt: atDaysAgo(118, 8, 0) } },
    { id: 7, cells: ["Roberto Nunes", "—", "5511977778888", "5511977778888@c.us", "Papelaria Central", null], dates: { createdAt: atDaysAgo(18, 15, 0) } },
    { id: 8, cells: ["Camila Prado", "Camila", "5511981112222", "5511981112222@c.us", "Timontec", null], dates: { createdAt: atDaysAgo(198, 10, 0) } },
    { id: 9, cells: ["Bruno Martins", "Bruno", "5511995554444", "5511995554444@c.us", "Prime Consulting", null], dates: { createdAt: atDaysAgo(8, 14, 0) } },
    { id: 10, cells: ["Larissa Gomes", "—", "5511986667777", "5511986667777@c.us", "TechSoft", null], dates: { createdAt: atDaysAgo(72, 9, 0) } },
    { id: 11, cells: ["Rafael Lima", "Rafa", "5511990001111", "5511990001111@c.us", "Sem empresa", null], dates: { createdAt: atDaysAgo(2, 11, 0) } },
    { id: 12, cells: ["Beatriz Castro", "Bia", "5511971110000", "5511971110000@c.us", "EletroMax", null], dates: { createdAt: atDaysAgo(10, 12, 0) } },
  ],
  companies: [
    { id: 1, cells: ["Timontec", "5515999999999", "Ativa", 5, null], dates: { createdAt: atDaysAgo(300, 9, 0) } },
    { id: 2, cells: ["TechSoft", "5511980000000", "Ativa", 3, null], dates: { createdAt: atDaysAgo(240, 10, 0) } },
    { id: 3, cells: ["Moda & Cia", "5511970000000", "Ativa", 4, null], dates: { createdAt: atDaysAgo(180, 11, 0) } },
    { id: 4, cells: ["EletroMax", "5511960000000", "Inativa", 2, null], dates: { createdAt: atDaysAgo(120, 9, 0) } },
    { id: 5, cells: ["Transporta Rápido", "5511950000000", "Ativa", 1, null], dates: { createdAt: atDaysAgo(90, 10, 0) } },
    { id: 6, cells: ["Papelaria Central", "5511940000000", "Ativa", 2, null], dates: { createdAt: atDaysAgo(60, 11, 0) } },
    { id: 7, cells: ["Prime Consulting", "5511930000000", "Inativa", 1, null], dates: { createdAt: atDaysAgo(30, 9, 0) } },
  ],
  messages: [
    { id: 1, cells: ["Atendimento Loja Centro", "Maria Silva", "Enviada", "Texto", "Lida", "Boa tarde! Segue o boleto", null, null], dates: { createdAt: atDaysAgo(0, 14, 30), sentAt: atDaysAgo(0, 14, 30), receivedAt: atDaysAgo(0, 14, 29) } },
    { id: 2, cells: ["Suporte Técnico", "José Santos", "Recebida", "Texto", "Lida", "Não consigo acessar o painel", null, null], dates: { createdAt: atDaysAgo(1, 11, 2), sentAt: atDaysAgo(1, 11, 1), receivedAt: atDaysAgo(1, 11, 2) } },
    { id: 3, cells: ["Vendas WhatsApp", "Ana Costa", "Enviada", "Imagem", "Entregue", "Aqui está o catálogo", null, null], dates: { createdAt: atDaysAgo(2, 16, 45), sentAt: atDaysAgo(2, 16, 45), receivedAt: atDaysAgo(2, 16, 44) } },
    { id: 4, cells: ["Financeiro", "Pedro Rocha", "Enviada", "Documento", "Lida", "Boleto em anexo", null, null], dates: { createdAt: atDaysAgo(3, 9, 15), sentAt: atDaysAgo(3, 9, 15), receivedAt: atDaysAgo(3, 9, 14) } },
    { id: 5, cells: ["Pós-venda", "Lucas Almeida", "Recebida", "Áudio", "Lida", "Áudio de 0:34", null, null], dates: { createdAt: atDaysAgo(5, 18, 10), sentAt: atDaysAgo(5, 18, 9), receivedAt: atDaysAgo(5, 18, 10) } },
    { id: 6, cells: ["Logística", "Fernanda Dias", "Enviada", "Texto", "Falhou", "Acompanhe o rastreio", null, null], dates: { createdAt: atDaysAgo(7, 10, 0), sentAt: atDaysAgo(7, 10, 0), receivedAt: atDaysAgo(7, 10, 0) } },
    { id: 7, cells: ["Comercial", "Roberto Nunes", "Enviada", "Documento", "Entregue", "Proposta comercial.pdf", null, null], dates: { createdAt: atDaysAgo(8, 15, 40), sentAt: atDaysAgo(8, 15, 40), receivedAt: atDaysAgo(8, 15, 39) } },
    { id: 8, cells: ["SAC", "Camila Prado", "Recebida", "Texto", "Lida", "Quero falar com um supervisor", null, null], dates: { createdAt: atDaysAgo(10, 12, 0), sentAt: atDaysAgo(10, 11, 59), receivedAt: atDaysAgo(10, 12, 0) } },
    { id: 9, cells: ["Atendimento Premium", "Bruno Martins", "Enviada", "Vídeo", "Lida", "Vídeo de apresentação", null, null], dates: { createdAt: atDaysAgo(12, 9, 30), sentAt: atDaysAgo(12, 9, 30), receivedAt: atDaysAgo(12, 9, 29) } },
    { id: 10, cells: ["Reclamações", "Larissa Gomes", "Recebida", "Texto", "Entregue", "Atrasou a entrega novamente", null, null], dates: { createdAt: atDaysAgo(15, 14, 0), sentAt: atDaysAgo(15, 13, 59), receivedAt: atDaysAgo(15, 14, 0) } },
    { id: 11, cells: ["Novos Leads", "Rafael Lima", "Recebida", "Sticker", "Lida", "Sticker animado", null, null], dates: { createdAt: atDaysAgo(1, 8, 20), sentAt: atDaysAgo(1, 8, 19), receivedAt: atDaysAgo(1, 8, 20) } },
    { id: 12, cells: ["Cobrança", "Beatriz Castro", "Enviada", "Texto", "Pendente", "Pode negociar o valor?", null, null], dates: { createdAt: atDaysAgo(2, 10, 5), sentAt: atDaysAgo(2, 10, 5), receivedAt: atDaysAgo(2, 10, 5) } },
    { id: 13, cells: ["Atendimento Loja Centro", "Maria Silva", "Recebida", "Texto", "Lida", "Qual o prazo de entrega?", null, null], dates: { createdAt: atDaysAgo(6, 9, 10), sentAt: atDaysAgo(6, 9, 9), receivedAt: atDaysAgo(6, 9, 10) } },
    { id: 14, cells: ["Suporte Técnico", "José Santos", "Enviada", "Imagem", "Entregue", "Tela do erro", null, null], dates: { createdAt: atDaysAgo(4, 17, 25), sentAt: atDaysAgo(4, 17, 25), receivedAt: atDaysAgo(4, 17, 24) } },
  ],
  occurrences: [
    { id: 1, cells: ["Boleto não gerado", "Atendimento Loja Centro", "Maria Silva", "Aberto", "Alta", "João Silva", null], dates: { createdAt: atDaysAgo(5, 10, 0) } },
    { id: 2, cells: ["Erro no sistema", "Suporte Técnico", "José Santos", "Em andamento", "Média", "Maria Oliveira", null], dates: { createdAt: atDaysAgo(10, 9, 0) } },
    { id: 3, cells: ["Dúvida sobre fatura", "Financeiro", "Pedro Rocha", "Resolvida", "Baixa", "Ana Lima", null], dates: { createdAt: atDaysAgo(4, 14, 0) } },
    { id: 4, cells: ["Atraso na entrega", "Logística", "Fernanda Dias", "Em andamento", "Urgente", "Maria Oliveira", null], dates: { createdAt: atDaysAgo(7, 8, 0) } },
    { id: 5, cells: ["Reembolso pendente", "Pós-venda", "Lucas Almeida", "Aberto", "Alta", "João Silva", null], dates: { createdAt: atDaysAgo(2, 16, 0) } },
    { id: 6, cells: ["Produto com defeito", "Reclamações", "Larissa Gomes", "Fechada", "Média", "Maria Oliveira", null], dates: { createdAt: atDaysAgo(20, 11, 0) } },
    { id: 7, cells: ["Cancelamento de pedido", "SAC", "Camila Prado", "Resolvida", "Baixa", "Ana Lima", null], dates: { createdAt: atDaysAgo(30, 9, 0) } },
    { id: 8, cells: ["Segunda via de contrato", "Cobrança", "Beatriz Castro", "Fechada", "Baixa", "Ana Lima", null], dates: { createdAt: atDaysAgo(15, 13, 0) } },
    { id: 9, cells: ["Dúvida sobre garantia", "Atendimento Premium", "Bruno Martins", "Em andamento", "Média", "João Silva", null], dates: { createdAt: atDaysAgo(1, 10, 0) } },
    { id: 10, cells: ["Atualização de cadastro", "Comercial", "Roberto Nunes", "Aberto", "Baixa", "Carlos Souza", null], dates: { createdAt: atDaysAgo(0, 12, 0) } },
  ],
  users: [
    { id: 1, cells: ["João Silva", "Administrador", "Ativo", null], dates: { createdAt: atDaysAgo(365, 9, 0) } },
    { id: 2, cells: ["Maria Oliveira", "Desenvolvedor", "Ativo", null], dates: { createdAt: atDaysAgo(300, 10, 0) } },
    { id: 3, cells: ["Carlos Souza", "Suporte", "Ativo", null], dates: { createdAt: atDaysAgo(200, 11, 0) } },
    { id: 4, cells: ["Ana Lima", "Suporte", "Ativo", null], dates: { createdAt: atDaysAgo(150, 9, 0) } },
    { id: 5, cells: ["Pedro Henrique", "Suporte", "Inativo", null], dates: { createdAt: atDaysAgo(100, 10, 0) } },
    { id: 6, cells: ["Luana Freitas", "Desenvolvedor", "Ativo", null], dates: { createdAt: atDaysAgo(50, 11, 0) } },
  ],
}

const DATE_COLUMN_KEYS: Record<ReportEntityKey, string[]> = {
  chats: ["", "", "", "", "createdAt", "lastMessageAt", "", "", ""],
  contacts: ["", "", "", "", "", "createdAt"],
  companies: ["", "", "", "", "createdAt"],
  messages: ["", "", "", "", "", "", "createdAt", "sentAt"],
  occurrences: ["", "", "", "", "", "", "createdAt"],
  users: ["", "", "", "createdAt"],
}

const NUMERIC_COLUMNS: Record<ReportEntityKey, string[]> = {
  chats: ["Mensagens", "Ocorrências"],
  contacts: [],
  companies: ["Contatos"],
  messages: [],
  occurrences: [],
  users: [],
}

function getColumnType(entity: ReportEntityKey, label: string): ColumnType {
  const idx = CONFIGS[entity].columns.indexOf(label)
  if (DATE_COLUMN_KEYS[entity][idx]) return "date"
  if (NUMERIC_COLUMNS[entity]?.includes(label)) return "number"
  return "text"
}

function columnFilterActive(filter: ColumnFilter): boolean {
  return (
    filter.textValue !== "" ||
    filter.numberValue !== "" ||
    filter.dateFrom !== "" ||
    filter.dateTo !== ""
  )
}

export function createDefaultFilter(type: ColumnType): ColumnFilter {
  return {
    type,
    textOp: "contains",
    textValue: "",
    numberOp: "gt",
    numberValue: "",
    numberValue2: "",
    dateFrom: "",
    dateTo: "",
  }
}

const LIMIT_OPTIONS = [10, 25, 50, 100]

function buildInitialReports(): SavedReport[] {
  return [
    { id: 1, name: "Mensagens enviadas hoje", entity: "messages", limit: 50 },
    { id: 2, name: "Conversas sem resposta (7 dias)", entity: "chats", limit: 25 },
    { id: 3, name: "Ocorrências abertas", entity: "occurrences", limit: 50 },
    { id: 4, name: "Contatos do último mês", entity: "contacts", limit: 100 },
    { id: 5, name: "Relatório mensal de atendimento", entity: "chats", limit: 100 },
  ]
}

export function useReports() {
  const [entity, setEntity] = useState<ReportEntityKey>("messages")
  const [savedReports, setSavedReports] = useState<SavedReport[]>(() => buildInitialReports())
  const [savedReportId, setSavedReportId] = useState<number | null>(null)
  const [limit, setLimit] = useState(25)
  const [selectedByEntity, setSelectedByEntity] = useState<Partial<Record<ReportEntityKey, string[]>>>({})
  const [filters, setFilters] = useState<Record<string, ColumnFilter>>({})

  const config = CONFIGS[entity]

  const visibleColumns = selectedByEntity[entity] ?? config.columns

  function handleEntityChange(next: ReportEntityKey) {
    setEntity(next)
    setSavedReportId(null)
    setFilters({})
  }

  function toggleColumn(label: string) {
    setSelectedByEntity((prev) => {
      const current = prev[entity] ?? config.columns
      const next = current.includes(label)
        ? current.filter((l) => l !== label)
        : [...current, label]
      return { ...prev, [entity]: next }
    })
  }

  function selectAllColumns() {
    setSelectedByEntity((prev) => ({ ...prev, [entity]: [...config.columns] }))
  }

  function clearColumns() {
    setSelectedByEntity((prev) => ({ ...prev, [entity]: [] }))
  }

  function applyColumnFilter(label: string, filter: ColumnFilter) {
    setFilters((prev) => ({ ...prev, [label]: filter }))
  }

  function clearColumnFilter(label: string) {
    setFilters((prev) => {
      const next = { ...prev }
      delete next[label]
      return next
    })
  }

  const columnType = (label: string): ColumnType => getColumnType(entity, label)

  const hasColumnFilter = (label: string): boolean => {
    const filter = filters[label]
    return filter ? columnFilterActive(filter) : false
  }

  function handleSavedReportChange(idStr: string) {
    const id = idStr ? Number(idStr) : null
    setSavedReportId(id)
    const report = savedReports.find((r) => r.id === id)
    if (!report) return
    setEntity(report.entity)
    setLimit(report.limit)
  }

  function saveCurrentReport() {
    const report: SavedReport = {
      id: Date.now(),
      name: `${ENTITY_LABELS[entity]} · ${limit} linhas`,
      entity,
      limit,
    }
    setSavedReports((prev) => [...prev, report])
    setSavedReportId(report.id)
  }

  function deleteSavedReport(id: number) {
    setSavedReports((prev) => prev.filter((r) => r.id !== id))
    if (savedReportId === id) setSavedReportId(null)
  }

  function clearFilters() {
    setSavedReportId(null)
    setLimit(25)
  }

  const all = MOCK_ROWS[entity]

  const filtered = useMemo(() => {
    let rows = all
    for (const [label, filter] of Object.entries(filters)) {
      const idx = CONFIGS[entity].columns.indexOf(label)
      if (idx < 0) continue
      const type = getColumnType(entity, label)
      if (type === "text" && filter.textValue.trim() !== "") {
        const q = filter.textValue.trim().toLowerCase()
        rows = rows.filter((r) => {
          const cell = r.cells[idx]
          if (cell === null || typeof cell === "number") return false
          const v = cell.toLowerCase()
          switch (filter.textOp) {
            case "contains":
              return v.includes(q)
            case "startsWith":
              return v.startsWith(q)
            case "endsWith":
              return v.endsWith(q)
            case "equals":
              return v === q
          }
        })
      } else if (type === "number" && filter.numberValue !== "") {
        const v = Number(filter.numberValue)
        rows = rows.filter((r) => {
          const cell = r.cells[idx]
          if (typeof cell !== "number") return false
          switch (filter.numberOp) {
            case "gt":
              return cell > v
            case "lt":
              return cell < v
            case "eq":
              return cell === v
            case "between": {
              const v2 = filter.numberValue2 === "" ? Number.POSITIVE_INFINITY : Number(filter.numberValue2)
              return cell >= v && cell <= v2
            }
          }
        })
      } else if (type === "date") {
        const dateKey = DATE_COLUMN_KEYS[entity][idx]
        if (filter.dateFrom) {
          const from = `${filter.dateFrom}T00:00:00`
          rows = rows.filter((r) => (r.dates[dateKey] ?? "") >= from)
        }
        if (filter.dateTo) {
          const to = `${filter.dateTo}T23:59:59`
          rows = rows.filter((r) => (r.dates[dateKey] ?? "") <= to)
        }
      }
    }
    return rows
  }, [all, entity, filters])

  const rows = filtered.slice(0, limit)

  return {
    entities: ENTITY_OPTIONS,
    entity,
    handleEntityChange,
    config,
    columns: config.columns,
    visibleColumns,
    toggleColumn,
    selectAllColumns,
    clearColumns,
    dateColumnKeys: DATE_COLUMN_KEYS[entity],
    columnType,
    filters,
    applyColumnFilter,
    clearColumnFilter,
    hasColumnFilter,
    limit,
    setLimit,
    limitOptions: LIMIT_OPTIONS,
    savedReports,
    savedReportId,
    handleSavedReportChange,
    saveCurrentReport,
    deleteSavedReport,
    clearFilters,
    rows,
    total: filtered.length,
    shown: rows.length,
  }
}
