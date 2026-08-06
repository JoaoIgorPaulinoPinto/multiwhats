
import {
  Check,
  CheckCircle2,
  ChevronDown,
  Copy,
  KeyRound,
  RefreshCw,
  RotateCcw,
  Save,
  Search,
  Settings,
  Settings2,
  TicketPlus,
  UserCog,
  X,
  XCircle,
} from 'lucide-react'
import { useState } from 'react'
import type { RegistrationCodeResponse } from '../../../services/auth.service'
import type { SystemParameterResponse } from '../../../services/settings.service'
import { useAuthStore } from '../../../stores/auth-store'
import { UsersView } from '../users/users.view'
import {
  AMERICA_TIMEZONES,
  GROUP_ICONS,
  GROUP_LABELS,
  LIST_OPTIONS,
  PARAM_HINTS,
  PARAM_LABELS,
  timeZoneLabel,
  TYPE_LABELS,
  useSettings,
} from './settings.logic'
import styles from './settings.module.css'

type Tab = 'params' | 'users'

function inputType(param: SystemParameterResponse): 'text' | 'number' | 'time' {
  if (param.type === 'Int') return 'number'
  if (param.key === 'Business:OpenTime' || param.key === 'Business:CloseTime')
    return 'time'
  return 'text'
}

function isLongText(param: SystemParameterResponse): boolean {
  return param.type === 'String' && param.key.endsWith('Message')
}

function isTimezone(param: SystemParameterResponse): boolean {
  return param.key === 'Business:Timezone'
}

function isListSelect(param: SystemParameterResponse): boolean {
  return !!LIST_OPTIONS[param.key]
}

function currentListValue(
  values: Record<string, string>,
  param: SystemParameterResponse,
): string[] {
  const display = values[param.key] ?? ''
  if (!display) return []
  return display
    .split(',')
    .map((s) => s.trim())
    .filter(Boolean)
}

function fieldLabel(param: SystemParameterResponse): string {
  return PARAM_LABELS[param.key] ?? param.key
}

function formatExpiry(expiresAt: string): string {
  const d = new Date(expiresAt)
  if (isNaN(d.getTime())) return expiresAt
  return d.toLocaleString('pt-BR', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  })
}

function codeStatus(c: RegistrationCodeResponse): {
  label: string
  valid: boolean
} {
  if (c.isUsed) return { label: 'Usado', valid: false }
  if (new Date(c.expiresAt).getTime() <= Date.now())
    return { label: 'Expirado', valid: false }
  return { label: 'Válido', valid: true }
}

function codeInfo(c: RegistrationCodeResponse): string {
  if (c.isUsed) return 'Usado — não pode mais ser utilizado'
  if (new Date(c.expiresAt).getTime() <= Date.now())
    return `Expirou em ${formatExpiry(c.expiresAt)}`
  return `Válido até ${formatExpiry(c.expiresAt)}`
}

export function SettingsView() {
  const {
    filteredGroups,
    groupOrder,
    loading,
    saving,
    reloading,
    values,
    dirty,
    hasChanges,
    validationErrors,
    hasErrors,
    query,
    setQuery,
    collapsed,
    toggleGroup,
    setAllCollapsed,
    setValue,
    revertValue,
    save,
    reload,
    generatingCode,
    generatedCodes,
    generateCode,
  } = useSettings()

  const user = useAuthStore((s) => s.user)
  const canGenerateCode = user?.role === 'Admin' || user?.role === 'Dev'

  const [tab, setTab] = useState<Tab>('params')
  const [copiedCode, setCopiedCode] = useState<string | null>(null)
  const showParams = tab === 'params'
  const activeDirtyCount = Object.keys(dirty).filter((k) => dirty[k]).length

  function copyCode(code: string) {
    navigator.clipboard.writeText(code).catch(() => {})
    setCopiedCode(code)
    setTimeout(() => setCopiedCode(null), 2000)
  }

  function toggleListItem(param: SystemParameterResponse, item: string) {
    const current = currentListValue(values, param)
    const next = current.includes(item)
      ? current.filter((x) => x !== item)
      : [...current, item]
    setValue(param.key, next.join(', '))
  }

  return (
    <div className={styles.page}>
      <header className={styles.header}>
        <div className={styles.headerRow}>
          <div className={styles.headerTitle}>
            <div className={styles.headerIcon}>
              <Settings size={20} />
            </div>
            <div>
              <h2>Configurações</h2>
              <p className={styles.subtitle}>
                Configure os parâmetros do sistema e gerencie os usuários.
              </p>
            </div>
          </div>
          {showParams && (
            <div className={styles.headerActions}>
              <button
                className={styles.reloadBtn}
                onClick={reload}
                disabled={reloading}
              >
                <RefreshCw size={18} className={reloading ? styles.spin : ''} />
                {reloading ? 'Recarregando...' : 'Recarregar cache'}
              </button>
              <button
                className={styles.saveBtn}
                onClick={save}
                disabled={saving || !hasChanges || hasErrors}
                title={
                  hasErrors
                    ? 'Existem campos com valores inválidos'
                    : !hasChanges
                      ? 'Nenhuma alteração'
                      : 'Salvar alterações'
                }
              >
                {saving ? (
                  <span className="spinner" style={{ width: 16, height: 16 }} />
                ) : (
                  <Save size={18} />
                )}
                {saving ? 'Salvando...' : 'Salvar alterações'}
                {!saving && hasChanges && (
                  <span className={styles.saveCount}>{activeDirtyCount}</span>
                )}
              </button>
            </div>
          )}
        </div>

        <div className={styles.tabs}>
          <button
            className={`${styles.tab} ${showParams ? styles.tabActive : ''}`}
            onClick={() => setTab('params')}
          >
            <Settings2 size={15} />
            Parâmetros do sistema
          </button>
          <button
            className={`${styles.tab} ${!showParams ? styles.tabActive : ''}`}
            onClick={() => setTab('users')}
          >
            <UserCog size={15} />
            Usuários
          </button>
        </div>

        {showParams && !loading && (
          <div className={styles.toolbar}>
            <div className={styles.search}>
              <Search size={15} />
              <input
                placeholder="Pesquisar por nome, chave ou grupo"
                value={query}
                onChange={(e) => setQuery(e.target.value)}
              />
              {query && (
                <button
                  className={styles.clearBtn}
                  onClick={() => setQuery('')}
                  title="Limpar pesquisa"
                >
                  <X size={14} />
                </button>
              )}
            </div>
            <div className={styles.toolbarActions}>
              <button
                className={styles.textBtn}
                onClick={() => setAllCollapsed(false)}
              >
                Expandir todos
              </button>
              <button
                className={styles.textBtn}
                onClick={() => setAllCollapsed(true)}
              >
                Recolher todos
              </button>
            </div>
          </div>
        )}
      </header>

      <section className={styles.content}>
        {showParams ? (
          loading ? (
            <div className={styles.skeletonList}>
              {Array.from({ length: 3 }).map((_, i) => (
                <div key={i} className={styles.skeletonGroup}>
                  <div
                    className="skeleton"
                    style={{ height: 16, width: 160 }}
                  />
                  {Array.from({ length: 2 }).map((_, j) => (
                    <div key={j} className={styles.skeletonCard}>
                      <div
                        style={{
                          flex: 1,
                          display: 'flex',
                          flexDirection: 'column',
                          gap: 6,
                        }}
                      >
                        <div
                          className="skeleton"
                          style={{ height: 14, width: '40%' }}
                        />
                        <div
                          className="skeleton"
                          style={{ height: 34, width: '100%' }}
                        />
                      </div>
                    </div>
                  ))}
                </div>
              ))}
            </div>
          ) : (
            <>
              {canGenerateCode && (
                <section className={`${styles.formCard} ${styles.codeCard}`}>
                  <div className={styles.groupTitleRow}>
                    <KeyRound size={16} className={styles.groupIcon} />
                    <h3 className={styles.groupTitle}>
                      Criar usuários com código de permissão
                    </h3>
                  </div>
                  <p className={styles.fieldDesc}>
                    Gere um código para compartilhar com quem precisa criar uma
                    conta no sistema. Cada código é válido por apenas uma
                    utilização e expira após o prazo definido em Autenticação.
                  </p>
                  <div className={styles.codeActions}>
                    <button
                      className={styles.generateBtn}
                      onClick={generateCode}
                      disabled={generatingCode}
                    >
                      {generatingCode ? (
                        <span
                          className="spinner"
                          style={{ width: 14, height: 14 }}
                        />
                      ) : (
                        <TicketPlus size={15} />
                      )}
                      {generatingCode
                        ? 'Gerando...'
                        : 'Gerar código de permissão'}
                    </button>
                    {generatedCodes.length > 0 && (
                      <div className={styles.codeResultList}>
                        {generatedCodes.map((c) => {
                          const status = codeStatus(c)
                          return (
                            <div key={c.id} className={styles.codeResult}>
                              <span className={styles.codeValue}>{c.code}</span>
                              <span
                                className={`${styles.codeStatus} ${status.valid ? styles.codeStatusValid : styles.codeStatusInvalid}`}
                              >
                                {status.valid ? (
                                  <CheckCircle2 size={12} />
                                ) : (
                                  <XCircle size={12} />
                                )}
                                {status.label}
                              </span>
                              <span className={styles.codeExpiry}>
                                {codeInfo(c)}
                              </span>
                              <button
                                className={styles.copyBtn}
                                onClick={() => copyCode(c.code)}
                              >
                                {copiedCode === c.code ? (
                                  <Check size={14} />
                                ) : (
                                  <Copy size={14} />
                                )}
                                {copiedCode === c.code ? 'Copiado' : 'Copiar'}
                              </button>
                            </div>
                          )
                        })}
                      </div>
                    )}
                  </div>
                </section>
              )}

              {query && (
                <p className={styles.resultsInfo}>
                  {Object.values(filteredGroups).reduce(
                    (acc, items) => acc + items.length,
                    0,
                  )}{' '}
                  resultado(s) para “{query}”
                </p>
              )}
              <div className={styles.formStack}>
                {groupOrder.map((group) => {
                  const items = filteredGroups[group]
                  if (!items || items.length === 0) return null
                  const Icon = GROUP_ICONS[group] ?? Settings2
                  const isCollapsed = collapsed.has(group)
                  return (
                    <section key={group} className={styles.formCard}>
                      <button
                        className={styles.groupHeader}
                        onClick={() => toggleGroup(group)}
                      >
                        <span className={styles.groupTitleRow}>
                          <Icon size={16} className={styles.groupIcon} />
                          <h3 className={styles.groupTitle}>
                            {GROUP_LABELS[group] ?? group}
                          </h3>
                          <span className={styles.groupCount}>
                            {items.length}{' '}
                            {items.length === 1 ? 'parâmetro' : 'parâmetros'}
                          </span>
                        </span>
                        <ChevronDown
                          size={16}
                          className={`${styles.chevron} ${isCollapsed ? styles.chevronCollapsed : ''}`}
                        />
                      </button>
                      {!isCollapsed && (
                        <div className={styles.fields}>
                          {items.map((param) => {
                            const key = param.key
                            const value = values[key] ?? ''
                            const isDirty = !!dirty[key]
                            const error = validationErrors[key]
                            const hint = PARAM_HINTS[key] ?? param.description
                            const label = fieldLabel(param)

                            if (param.type === 'Bool') {
                              const checked = value === 'true'
                              return (
                                <div key={key} className={styles.switchField}>
                                  <div className={styles.switchText}>
                                    <div className={styles.fieldLabelRow}>
                                      <label
                                        className={styles.fieldLabel}
                                        title={key}
                                      >
                                        {label}
                                      </label>
                                      {isDirty && (
                                        <span className={styles.dirtyBadge}>
                                          Alterado
                                        </span>
                                      )}
                                    </div>
                                    {hint && (
                                      <p className={styles.fieldDesc}>{hint}</p>
                                    )}
                                  </div>
                                  <label className={styles.switchRow}>
                                    <input
                                      type="checkbox"
                                      className={styles.switchInput}
                                      checked={checked}
                                      onChange={(e) =>
                                        setValue(
                                          key,
                                          e.target.checked ? 'true' : 'false',
                                        )
                                      }
                                    />
                                    <span className={styles.switchTrack}>
                                      <span className={styles.switchThumb} />
                                    </span>
                                  </label>
                                </div>
                              )
                            }

                            if (isListSelect(param)) {
                              const selected = currentListValue(values, param)
                              return (
                                <div
                                  key={key}
                                  className={`${styles.field} ${error ? styles.fieldInvalid : ''}`}
                                >
                                  <div className={styles.fieldLabelRow}>
                                    <label
                                      className={styles.fieldLabel}
                                      title={key}
                                    >
                                      {label}
                                    </label>
                                    <span className={styles.fieldMeta}>
                                      {isDirty && (
                                        <button
                                          className={styles.revertBtn}
                                          onClick={() => revertValue(key)}
                                          title="Reverter alteração"
                                        >
                                          <RotateCcw size={13} />
                                          Reverter
                                        </button>
                                      )}
                                      <span className={styles.typeBadge}>
                                        {TYPE_LABELS[param.type] ?? param.type}
                                      </span>
                                      {error && (
                                        <span className={styles.errorBadge}>
                                          Inválido
                                        </span>
                                      )}
                                    </span>
                                  </div>
                                  <div className={styles.chipList}>
                                    {(LIST_OPTIONS[param.key] ?? []).map(
                                      (opt) => {
                                        const isSelected = selected.includes(
                                          opt.value,
                                        )
                                        return (
                                          <button
                                            key={opt.value}
                                            type="button"
                                            className={`${styles.chip} ${isSelected ? styles.chipSelected : ''}`}
                                            onClick={() =>
                                              toggleListItem(param, opt.value)
                                            }
                                          >
                                            {isSelected && <Check size={12} />}
                                            {opt.label}
                                          </button>
                                        )
                                      },
                                    )}
                                  </div>
                                  {error ? (
                                    <p className={styles.fieldError}>{error}</p>
                                  ) : (
                                    hint && (
                                      <p className={styles.fieldDesc}>{hint}</p>
                                    )
                                  )}
                                </div>
                              )
                            }

                            if (isTimezone(param)) {
                              return (
                                <div
                                  key={key}
                                  className={`${styles.field} ${error ? styles.fieldInvalid : ''}`}
                                >
                                  <div className={styles.fieldLabelRow}>
                                    <label
                                      className={styles.fieldLabel}
                                      title={key}
                                    >
                                      {label}
                                    </label>
                                    <span className={styles.fieldMeta}>
                                      {isDirty && (
                                        <button
                                          className={styles.revertBtn}
                                          onClick={() => revertValue(key)}
                                          title="Reverter alteração"
                                        >
                                          <RotateCcw size={13} />
                                          Reverter
                                        </button>
                                      )}
                                      <span className={styles.typeBadge}>
                                        {TYPE_LABELS[param.type] ?? param.type}
                                      </span>
                                      {error && (
                                        <span className={styles.errorBadge}>
                                          Inválido
                                        </span>
                                      )}
                                    </span>
                                  </div>
                                  <select
                                    className={styles.fieldInput}
                                    value={value}
                                    onChange={(e) =>
                                      setValue(key, e.target.value)
                                    }
                                  >
                                    {AMERICA_TIMEZONES.map((tz) => (
                                      <option key={tz} value={tz}>
                                        {timeZoneLabel(tz)}
                                      </option>
                                    ))}
                                  </select>
                                  {error ? (
                                    <p className={styles.fieldError}>{error}</p>
                                  ) : (
                                    hint && (
                                      <p className={styles.fieldDesc}>{hint}</p>
                                    )
                                  )}
                                </div>
                              )
                            }

                            return (
                              <div
                                key={key}
                                className={`${styles.field} ${error ? styles.fieldInvalid : ''}`}
                              >
                                <div className={styles.fieldLabelRow}>
                                  <label
                                    className={styles.fieldLabel}
                                    title={key}
                                  >
                                    {label}
                                  </label>
                                  <span className={styles.fieldMeta}>
                                    {isDirty && (
                                      <button
                                        className={styles.revertBtn}
                                        onClick={() => revertValue(key)}
                                        title="Reverter alteração"
                                      >
                                        <RotateCcw size={13} />
                                        Reverter
                                      </button>
                                    )}
                                    <span className={styles.typeBadge}>
                                      {TYPE_LABELS[param.type] ?? param.type}
                                    </span>
                                    {error && (
                                      <span className={styles.errorBadge}>
                                        Inválido
                                      </span>
                                    )}
                                  </span>
                                </div>

                                {isLongText(param) ? (
                                  <textarea
                                    className={styles.fieldInput}
                                    rows={3}
                                    value={value}
                                    onChange={(e) =>
                                      setValue(key, e.target.value)
                                    }
                                  />
                                ) : (
                                  <input
                                    className={styles.fieldInput}
                                    type={inputType(param)}
                                    value={value}
                                    onChange={(e) =>
                                      setValue(key, e.target.value)
                                    }
                                  />
                                )}

                                {error ? (
                                  <p className={styles.fieldError}>{error}</p>
                                ) : (
                                  hint && (
                                    <p className={styles.fieldDesc}>{hint}</p>
                                  )
                                )}
                              </div>
                            )
                          })}
                        </div>
                      )}
                    </section>
                  )
                })}

                {Object.keys(filteredGroups).length === 0 && (
                  <div className={styles.emptyState}>
                    <Search size={20} />
                    <p>Nenhum parâmetro encontrado para “{query}”.</p>
                  </div>
                )}
              </div>

              {hasChanges && !hasErrors && (
                <div className={styles.changesBar}>
                  <CheckCircle2 size={15} />
                  <span>
                    {activeDirtyCount}{' '}
                    {activeDirtyCount === 1
                      ? 'alteração pendente'
                      : 'alterações pendentes'}{' '}
                    — clique em “Salvar alterações”.
                  </span>
                </div>
              )}
            </>
          )
        ) : (
          <UsersView embedded />
        )}
      </section>
    </div>
  )
}
