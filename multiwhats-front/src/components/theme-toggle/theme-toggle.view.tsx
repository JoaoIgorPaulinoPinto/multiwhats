"use client"

import { Moon, Sun } from "lucide-react"
import { useThemeToggle } from "./theme-toggle.logic"
import styles from "./theme-toggle.module.css"

export function ThemeToggleView() {
  const { theme, toggle } = useThemeToggle()

  return (
    <button className={styles.toggle} onClick={toggle} aria-label="Alternar tema">
      {theme === "dark" ? <Sun size={18} /> : <Moon size={18} />}
    </button>
  )
}
