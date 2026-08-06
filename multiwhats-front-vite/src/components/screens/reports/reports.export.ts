
import { jsPDF } from 'jspdf'
import autoTable from 'jspdf-autotable'
import * as XLSX from 'xlsx'
import {
  ENTITY_LABELS,
  resolveRowCell,
  type ReportColumnRef,
  type ReportEntityKey,
  type ReportRow,
} from './reports.logic'

function pad(n: number): string {
  return n.toString().padStart(2, '0')
}

function fileName(ext: string): string {
  const d = new Date()
  const stamp = `${d.getFullYear()}${pad(d.getMonth() + 1)}${pad(d.getDate())}-${pad(d.getHours())}${pad(d.getMinutes())}`
  return `relatorio-${stamp}.${ext}`
}

interface ExportMatrix {
  headRows: string[][]
  bodyRows: (string | number)[][]
}

function buildMatrix(
  rows: ReportRow[],
  tableColumns: ReportColumnRef[],
  mainEntity: ReportEntityKey,
): ExportMatrix {
  const entities = new Set(tableColumns.map((c) => c.entity))
  const headRows: string[][] = []
  if (entities.size > 1) {
    headRows.push(tableColumns.map((c) => ENTITY_LABELS[c.entity]))
    headRows.push(tableColumns.map((c) => c.label))
  } else {
    headRows.push(tableColumns.map((c) => c.label))
  }
  const bodyRows = rows.map((r) =>
    tableColumns.map((ref) => {
      const cell = resolveRowCell(r, ref, mainEntity)
      return cell.text === null ? '' : cell.text
    }),
  )
  return { headRows, bodyRows }
}

export function exportExcel(
  rows: ReportRow[],
  tableColumns: ReportColumnRef[],
  mainEntity: ReportEntityKey,
): void {
  const { headRows, bodyRows } = buildMatrix(rows, tableColumns, mainEntity)
  const ws = XLSX.utils.aoa_to_sheet([...headRows, ...bodyRows])
  ws['!cols'] = tableColumns.map(() => ({ wch: 24 }))
  const wb = XLSX.utils.book_new()
  XLSX.utils.book_append_sheet(wb, ws, 'Relatório')
  XLSX.writeFile(wb, fileName('xlsx'))
}

export function exportPdf(
  rows: ReportRow[],
  tableColumns: ReportColumnRef[],
  mainEntity: ReportEntityKey,
): void {
  const { headRows, bodyRows } = buildMatrix(rows, tableColumns, mainEntity)
  const orientation: 'portrait' | 'landscape' =
    tableColumns.length > 7 ? 'landscape' : 'portrait'
  const doc = new jsPDF({ orientation })
  doc.setFontSize(12)
  doc.text(`Relatório de ${ENTITY_LABELS[mainEntity]}`, 14, 12)
  autoTable(doc, {
    startY: 18,
    head: headRows,
    body: bodyRows,
    theme: 'striped',
    styles: { fontSize: 8, cellPadding: 2 },
    headStyles: { fillColor: [37, 211, 102], textColor: [255, 255, 255] },
  })
  doc.save(fileName('pdf'))
}
