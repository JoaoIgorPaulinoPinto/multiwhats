"use client"

import { LogOut, User } from "lucide-react"
import { ThemeToggleView } from "../theme-toggle/theme-toggle.view"
import { useProfilePopover } from "./profile-popover.logic"
import { useAuthStore } from "../../stores/auth-store"
import { AvatarView } from "../avatar/avatar.view"
import styles from "./profile-popover.module.css"

export function ProfilePopoverView() {
  const { open, toggle, close } = useProfilePopover()
  const user = useAuthStore((s) => s.user)
  const logout = useAuthStore((s) => s.logout)

  return (
    <div className={styles.wrapper}>
      <button className={styles.trigger} onClick={toggle} aria-label="Perfil">
        <User size={22} />
      </button>

      {open && (
        <>
          <div className={styles.overlay} onClick={close} />
          <div className={styles.popover}>
            <AvatarView name={user?.name ?? "?"} size={64} fontSize={28} />
            <div className={styles.userDatail}>
              <strong className={styles.name}>{user?.name ?? "Usuário"}</strong>
              <span className={styles.role}>{user?.role ?? "Support"}</span>
            </div>
            <div className={styles.toggleRow}>
              <ThemeToggleView />
            </div>
            <button className={styles.logout} onClick={logout}>
              <LogOut size={16} />
              Sair
            </button>
          </div>
        </>
      )}
    </div>
  )
}
