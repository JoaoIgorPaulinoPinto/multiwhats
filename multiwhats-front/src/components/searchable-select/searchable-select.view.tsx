"use client"

import { Check, ChevronDown, Search } from "lucide-react"
import { useEffect, useMemo, useRef, useState } from "react"
import styles from "./searchable-select.module.css"

export interface SearchableOption {
  id: number
  name: string
}

interface Props {
  options: SearchableOption[]
  value: number | null
  onChange: (v: number | null) => void
  emptyLabel?: string
  placeholder?: string
  disabled?: boolean
}

export function SearchableSelect({
  options,
  value,
  onChange,
  emptyLabel = "Sem empresa",
  placeholder = "Selecione...",
  disabled = false,
}: Props) {
  const [open, setOpen] = useState(false)
  const [query, setQuery] = useState("")
  const rootRef = useRef<HTMLDivElement>(null)
  const inputRef = useRef<HTMLInputElement>(null)

  const selected = options.find((o) => o.id === value) ?? null

  const filtered = useMemo(() => {
    const q = query.trim().toLowerCase()
    if (!q) return options
    return options.filter((o) => o.name.toLowerCase().includes(q))
  }, [options, query])

  useEffect(() => {
    function handleClick(e: MouseEvent) {
      if (rootRef.current && !rootRef.current.contains(e.target as Node)) {
        setOpen(false)
      }
    }
    function handleKey(e: KeyboardEvent) {
      if (e.key === "Escape") setOpen(false)
    }
    document.addEventListener("mousedown", handleClick)
    document.addEventListener("keydown", handleKey)
    return () => {
      document.removeEventListener("mousedown", handleClick)
      document.removeEventListener("keydown", handleKey)
    }
  }, [])

  useEffect(() => {
    if (open) {
      inputRef.current?.focus()
    }
  }, [open])

  function toggle() {
    setOpen((prev) => {
      const next = !prev
      if (next) setQuery("")
      return next
    })
  }

  function select(v: number | null) {
    onChange(v)
    setOpen(false)
  }

  return (
    <div className={styles.root} ref={rootRef}>
      <button
        type="button"
        className={styles.trigger}
        onClick={toggle}
        disabled={disabled}
      >
        <span className={selected ? styles.value : styles.placeholder}>
          {selected ? selected.name : placeholder}
        </span>
        <ChevronDown size={15} className={styles.chevron} />
      </button>

      {open && (
        <div className={styles.dropdown}>
          <div className={styles.searchBox}>
            <Search size={14} />
            <input
              ref={inputRef}
              value={query}
              onChange={(e) => setQuery(e.target.value)}
              placeholder="Pesquisar..."
            />
          </div>
          <div className={styles.list}>
            <button
              type="button"
              className={`${styles.item} ${value === null ? styles.active : ""}`}
              onClick={() => select(null)}
            >
              <span>{emptyLabel}</span>
              {value === null && <Check size={14} />}
            </button>
            {filtered.map((o) => (
              <button
                key={o.id}
                type="button"
                className={`${styles.item} ${value === o.id ? styles.active : ""}`}
                onClick={() => select(o.id)}
              >
                <span>{o.name}</span>
                {value === o.id && <Check size={14} />}
              </button>
            ))}
            {filtered.length === 0 && (
              <span className={styles.empty}>Nenhum resultado</span>
            )}
          </div>
        </div>
      )}
    </div>
  )
}
