
import {
  Check,
  ChevronDown,
  FileChartColumn,
  FileSpreadsheet,
  FileText,
  ListFilter,
  RotateCcw,
  Save,
  Trash2,
  X,
} from 'lucide-react'
import { useEffect, useRef, useState } from 'react'
import { exportExcel, exportPdf } from './reports.export'
import {
  ENTITY_LABELS,
  createDefaultFilter,
  getColumnKey,
  resolveRowCell,
  useReports,
  type ColumnFilter,
  type ColumnType,
  type ReportColumnRef,
  type ReportEntityKey,
  type ReportRow,
} from './reports.logic'
import styles from './reports.module.css'

const TEXT_OP_LABELS: Record<string, string> = {
  contains: 'Contém',
  startsWith: 'Começa com',
  endsWith: 'Termina com',
  equals: 'Igual a',
}

const NUMBER_OP_LABELS: Record<string, string> = {
  gt: 'Maior que',
  lt: 'Menor que',
  eq: 'Igual a',
  between: 'Entre',
}

function renderCell(
  row: ReportRow,
  ref: ReportColumnRef,
  mainEntity: ReportEntityKey,
): React.ReactNode {
  const resolved = resolveRowCell(row, ref, mainEntity)
  if (resolved.isDate) {
    return <span className={styles.dateCell}>{resolved.text}</span>
  }
  if (resolved.text === null) {
    return <span className={styles.muted}>—</span>
  }
  if (typeof resolved.text === 'number') {
    return <span className={styles.numCell}>{resolved.text}</span>
  }
  return <span className={styles.textCell}>{resolved.text}</span>
}

interface DropdownProps {
  column: ReportColumnRef
  type: ColumnType
  filter: ColumnFilter | null
  position: { top: number; left: number }
  onApply: (filter: ColumnFilter) => void
  onClear: () => void
  onClose: () => void
}

function ColumnFilterDropdown({
  column,
  type,
  filter,
  position,
  onApply,
  onClear,
  onClose,
}: DropdownProps) {
  const [draft, setDraft] = useState<ColumnFilter>(
    () => filter ?? createDefaultFilter(type),
  )
  const rootRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    function handleMouseDown(e: MouseEvent) {
      if (rootRef.current && !rootRef.current.contains(e.target as Node))
        onClose()
    }
    function handleKey(e: KeyboardEvent) {
      if (e.key === 'Escape') onClose()
    }
    document.addEventListener('mousedown', handleMouseDown)
    document.addEventListener('keydown', handleKey)
    return () => {
      document.removeEventListener('mousedown', handleMouseDown)
      document.removeEventListener('keydown', handleKey)
    }
  }, [onClose])

  const setText = (value: string) =>
    setDraft((d) => ({ ...d, textValue: value }))
  const setNumber = (value: string) =>
    setDraft((d) => ({ ...d, numberValue: value }))
  const setNumber2 = (value: string) =>
    setDraft((d) => ({ ...d, numberValue2: value }))
  const setFrom = (value: string) =>
    setDraft((d) => ({ ...d, dateFrom: value }))
  const setTo = (value: string) => setDraft((d) => ({ ...d, dateTo: value }))

  return (
    <div
      ref={rootRef}
      className={`${styles.filterDropdown} fadeIn`}
      style={{ top: position.top, left: position.left }}
    >
      <div className={styles.filterDropdownTitle}>
        <span>
          {ENTITY_LABELS[column.entity]} · {column.label}
        </span>
        <span className={styles.filterDropdownType}>
          {type === 'date'
            ? 'Data e hora'
            : type === 'number'
              ? 'Número'
              : 'Texto'}
        </span>
      </div>

      {type === 'text' && (
        <>
          <div className={styles.filterField}>
            <label className={styles.filterLabel}>Filtro</label>
            <select
              className={styles.filterInput}
              value={draft.textOp}
              onChange={(e) =>
                setDraft((d) => ({
                  ...d,
                  textOp: e.target.value as ColumnFilter['textOp'],
                }))
              }
            >
              {Object.entries(TEXT_OP_LABELS).map(([op, label]) => (
                <option key={op} value={op}>
                  {label}
                </option>
              ))}
            </select>
          </div>
          <div className={styles.filterField}>
            <label className={styles.filterLabel}>Valor</label>
            <input
              className={styles.filterInput}
              value={draft.textValue}
              onChange={(e) => setText(e.target.value)}
              placeholder="Digite o texto..."
            />
          </div>
        </>
      )}

      {type === 'number' && (
        <>
          <div className={styles.filterField}>
            <label className={styles.filterLabel}>Filtro</label>
            <select
              className={styles.filterInput}
              value={draft.numberOp}
              onChange={(e) =>
                setDraft((d) => ({
                  ...d,
                  numberOp: e.target.value as ColumnFilter['numberOp'],
                }))
              }
            >
              {Object.entries(NUMBER_OP_LABELS).map(([op, label]) => (
                <option key={op} value={op}>
                  {label}
                </option>
              ))}
            </select>
          </div>
          <div className={styles.filterField}>
            <label className={styles.filterLabel}>
              {draft.numberOp === 'between' ? 'Mínimo' : 'Valor'}
            </label>
            <input
              className={styles.filterInput}
              type="number"
              value={draft.numberValue}
              onChange={(e) => setNumber(e.target.value)}
              placeholder="0"
            />
          </div>
          {draft.numberOp === 'between' && (
            <div className={styles.filterField}>
              <label className={styles.filterLabel}>Máximo</label>
              <input
                className={styles.filterInput}
                type="number"
                value={draft.numberValue2}
                onChange={(e) => setNumber2(e.target.value)}
                placeholder="0"
              />
            </div>
          )}
        </>
      )}

      {type === 'date' && (
        <>
          <div className={styles.filterField}>
            <label className={styles.filterLabel}>De</label>
            <input
              className={styles.filterInput}
              type="date"
              value={draft.dateFrom}
              onChange={(e) => setFrom(e.target.value)}
            />
          </div>
          <div className={styles.filterField}>
            <label className={styles.filterLabel}>Até</label>
            <input
              className={styles.filterInput}
              type="date"
              value={draft.dateTo}
              onChange={(e) => setTo(e.target.value)}
            />
          </div>
        </>
      )}

      <div className={styles.filterActions}>
        <button className={styles.filterClear} onClick={onClear}>
          <RotateCcw size={13} />
          Limpar
        </button>
        <button className={styles.filterApply} onClick={() => onApply(draft)}>
          <Check size={13} />
          Aplicar
        </button>
      </div>
    </div>
  )
}

export function ReportsView() {
  const {
    entities,
    entity,
    handleEntityChange,
    relatedEntities,
    relatedOptions,
    toggleRelatedEntity,
    activeEntities,
    tableColumns,
    columnEntity,
    setColumnEntity,
    panelColumns,
    panelSelected,
    toggleColumn,
    selectAllColumns,
    clearColumns,
    columnType,
    filters,
    applyColumnFilter,
    clearColumnFilter,
    hasColumnFilter,
    limit,
    setLimit,
    limitOptions,
    savedReports,
    savedReportId,
    handleSavedReportChange,
    saveCurrentReport,
    deleteSavedReport,
    clearFilters,
    rows,
    total,
    shown,
  } = useReports()

  const [openColumn, setOpenColumn] = useState<ReportColumnRef | null>(null)
  const [openPos, setOpenPos] = useState({ top: 0, left: 0 })
  const [openRelated, setOpenRelated] = useState(false)
  const relatedRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    if (!openRelated) return
    function handleMouseDown(e: MouseEvent) {
      if (relatedRef.current && !relatedRef.current.contains(e.target as Node))
        setOpenRelated(false)
    }
    function handleKey(e: KeyboardEvent) {
      if (e.key === 'Escape') setOpenRelated(false)
    }
    document.addEventListener('mousedown', handleMouseDown)
    document.addEventListener('keydown', handleKey)
    return () => {
      document.removeEventListener('mousedown', handleMouseDown)
      document.removeEventListener('keydown', handleKey)
    }
  }, [openRelated])

  function toggleColumnFilter(ref: ReportColumnRef, th: HTMLElement | null) {
    if (
      openColumn !== null &&
      openColumn.entity === ref.entity &&
      openColumn.label === ref.label
    ) {
      setOpenColumn(null)
      return
    }
    if (th) {
      const rect = th.getBoundingClientRect()
      const width = 300
      const left = Math.max(
        8,
        Math.min(rect.left, window.innerWidth - width - 8),
      )
      setOpenPos({ top: rect.bottom + 4, left })
    }
    setOpenColumn(ref)
  }

  return (
    <div className={styles.page}>
      <header className={styles.header}>
        <div className={styles.headerRow}>
          <div className={styles.headerTitle}>
            <div className={styles.headerIcon}>
              <FileChartColumn size={20} />
            </div>
            <div>
              <h2>Relatórios</h2>
              <p className={styles.subtitle}>
                Monte consultas com filtros e exporte os dados das entidades do
                sistema.
              </p>
            </div>
          </div>
          <div className={styles.headerActions}>
            <button
              className={styles.exportBtn}
              onClick={() => exportExcel(rows, tableColumns, entity)}
              disabled={rows.length === 0 || tableColumns.length === 0}
              title="Exportar para Excel"
            >
              <FileSpreadsheet size={16} />
              Excel
            </button>
            <button
              className={styles.exportBtn}
              onClick={() => exportPdf(rows, tableColumns, entity)}
              disabled={rows.length === 0 || tableColumns.length === 0}
              title="Exportar para PDF"
            >
              <FileText size={16} />
              PDF
            </button>
            <button
              className={styles.saveBtn}
              onClick={saveCurrentReport}
              title="Salvar a configuração atual como relatório"
            >
              <Save size={16} />
              Salvar relatório
            </button>
          </div>
        </div>
      </header>

      <section className={styles.filters}>
        <div className={styles.filterGrid}>
          <div className={styles.field}>
            <label className={styles.label}>Entidade</label>
            <select
              className={styles.select}
              value={entity}
              onChange={(e) => {
                setOpenColumn(null)
                setOpenRelated(false)
                handleEntityChange(e.target.value as ReportEntityKey)
              }}
            >
              {entities.map((opt) => (
                <option key={opt.key} value={opt.key}>
                  {opt.label}
                </option>
              ))}
            </select>
          </div>

          <div className={styles.field}>
            <label className={styles.label}>Entidades relacionadas</label>
            <div className={styles.multiSelect} ref={relatedRef}>
              <button
                type="button"
                className={`${styles.multiTrigger} ${relatedEntities.length > 0 ? styles.multiTriggerActive : ''}`}
                onClick={() => setOpenRelated((v) => !v)}
              >
                <span className={styles.multiValue}>
                  {relatedEntities.length === 0
                    ? 'Nenhuma'
                    : relatedEntities.map((k) => ENTITY_LABELS[k]).join(', ')}
                </span>
                <ChevronDown size={15} className={styles.multiChevron} />
              </button>
              {openRelated && (
                <div className={styles.multiDropdown}>
                  {relatedOptions.length === 0 ? (
                    <p className={styles.multiEmpty}>
                      Esta entidade não possui relacionamentos.
                    </p>
                  ) : (
                    relatedOptions.map((opt) => {
                      const checked = relatedEntities.includes(opt)
                      return (
                        <div key={opt} className={styles.columnItemRow}>
                          <label
                            className={`${styles.columnItem} ${checked ? styles.columnItemChecked : ''}`}
                          >
                            <input
                              type="checkbox"
                              checked={checked}
                              onChange={() => toggleRelatedEntity(opt)}
                            />
                            <span className={styles.columnItemLabel}>
                              {ENTITY_LABELS[opt]}
                            </span>
                          </label>
                        </div>
                      )
                    })
                  )}
                </div>
              )}
            </div>
          </div>

          <div className={styles.field}>
            <label className={styles.label}>Relatório salvo</label>
            <div className={styles.selectWrap}>
              <select
                className={styles.select}
                value={savedReportId ?? ''}
                onChange={(e) => handleSavedReportChange(e.target.value)}
              >
                <option value="">Nenhum</option>
                {savedReports.map((r) => (
                  <option key={r.id} value={r.id}>
                    {r.name}
                  </option>
                ))}
              </select>
              {savedReportId !== null && (
                <button
                  className={styles.removeReport}
                  title="Remover relatório salvo"
                  onClick={() => deleteSavedReport(savedReportId)}
                >
                  <Trash2 size={14} />
                </button>
              )}
            </div>
          </div>

          <div className={styles.field}>
            <label className={styles.label}>Limite de linhas</label>
            <select
              className={styles.select}
              value={limit}
              onChange={(e) => setLimit(Number(e.target.value))}
            >
              {limitOptions.map((n) => (
                <option key={n} value={n}>
                  {n} linhas
                </option>
              ))}
            </select>
          </div>

          <div className={styles.field}>
            <label className={styles.label}>&nbsp;</label>
            <button className={styles.clearBtn} onClick={clearFilters}>
              <RotateCcw size={14} />
              Limpar
            </button>
          </div>
        </div>
      </section>

      <section className={styles.content}>
        <div className={styles.summary}>
          <span>
            Mostrando <strong>{shown}</strong> de <strong>{total}</strong>{' '}
            registros
          </span>
          <span className={styles.summaryEntity}>
            <FileText size={14} />
            {activeEntities.map((e) => ENTITY_LABELS[e]).join(' + ')}
          </span>
        </div>

        <div className={styles.split}>
          <div className={styles.tableWrap}>
            <table className={styles.table}>
              <thead>
                <tr>
                  {tableColumns.map((ref) => {
                    const active = hasColumnFilter(ref)
                    return (
                      <th
                        key={getColumnKey(ref)}
                        className={styles.filterableTh}
                      >
                        <button
                          type="button"
                          className={`${styles.thBtn} ${active ? styles.thBtnActive : ''}`}
                          title={`Filtrar ${ref.label}`}
                          onClick={(e) =>
                            toggleColumnFilter(
                              ref,
                              e.currentTarget.parentElement,
                            )
                          }
                        >
                          {ref.entity !== entity && (
                            <span className={styles.thEntityBadge}>
                              {ENTITY_LABELS[ref.entity]}
                            </span>
                          )}
                          <span className={styles.thBtnRow}>
                            <span>{ref.label}</span>
                            <ListFilter size={13} />
                          </span>
                        </button>
                      </th>
                    )
                  })}
                </tr>
              </thead>
              <tbody>
                {rows.length === 0 ? (
                  <tr>
                    <td
                      className={styles.emptyCell}
                      colSpan={tableColumns.length || 1}
                    >
                      Nenhum registro encontrado para os filtros selecionados.
                    </td>
                  </tr>
                ) : (
                  rows.map((row) => (
                    <tr key={row.id}>
                      {tableColumns.map((ref, idx) => (
                        <td key={idx}>{renderCell(row, ref, entity)}</td>
                      ))}
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>

          <aside className={styles.columnPanel}>
            <div className={styles.columnPanelHeader}>
              <h3>Colunas do relatório</h3>
              <span className={styles.columnCount}>
                {panelSelected.length} de {panelColumns.length}
              </span>
            </div>
            <div className={styles.columnPanelEntity}>
              <label className={styles.label}>Entidade</label>
              <select
                className={styles.select}
                value={columnEntity}
                onChange={(e) =>
                  setColumnEntity(e.target.value as ReportEntityKey)
                }
              >
                {activeEntities.map((e) => (
                  <option key={e} value={e}>
                    {ENTITY_LABELS[e]}
                  </option>
                ))}
              </select>
            </div>
            <div className={styles.columnPanelActions}>
              <button className={styles.panelAction} onClick={selectAllColumns}>
                <Check size={13} />
                Marcar todas
              </button>
              <button className={styles.panelAction} onClick={clearColumns}>
                <X size={13} />
                Limpar
              </button>
            </div>
            <div className={styles.columnList}>
              {panelColumns.map((col) => {
                const checked = panelSelected.includes(col)
                return (
                  <div key={col} className={styles.columnItemRow}>
                    <label
                      className={`${styles.columnItem} ${checked ? styles.columnItemChecked : ''}`}
                    >
                      <input
                        type="checkbox"
                        checked={checked}
                        onChange={() => toggleColumn(col)}
                      />
                      <span className={styles.columnItemLabel}>{col}</span>
                    </label>
                  </div>
                )
              })}
            </div>
            {panelSelected.length === 0 && (
              <p className={styles.columnEmpty}>
                Selecione ao menos uma coluna para exibir.
              </p>
            )}
          </aside>
        </div>
      </section>

      {openColumn !== null && (
        <ColumnFilterDropdown
          column={openColumn}
          type={columnType(openColumn)}
          filter={filters[getColumnKey(openColumn)] ?? null}
          position={openPos}
          onApply={(f) => {
            applyColumnFilter(openColumn, f)
            setOpenColumn(null)
          }}
          onClear={() => {
            clearColumnFilter(openColumn)
            setOpenColumn(null)
          }}
          onClose={() => setOpenColumn(null)}
        />
      )}
    </div>
  )
}
