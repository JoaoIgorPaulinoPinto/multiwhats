'use client'
import { Building2, LayoutDashboard, MessageSquare, Users } from "lucide-react"
import { usePathname, useRouter } from "next/navigation"
import { ProfilePopoverView } from "../profile-popover/profile-popover.view"
import styles from "./nav-bar.module.css"

const items: { path: string; icon: React.ReactNode; label: string }[] = [
  { path: "/chats", icon: <MessageSquare size={22} />, label: "Chats" },
  { path: "/contacts", icon: <Users size={22} />, label: "Contatos" },
  { path: "/kanban", icon: <LayoutDashboard size={22} />, label: "Kanban" },
  { path: "/companies", icon: <Building2 size={22} />, label: "Empresas" },
]

export function NavBarView() {
  const pathname = usePathname()
  const router = useRouter()

  return (
    <nav className={styles.nav}>
      <div className={styles.navTop}>
        {items.map((item) => (
          <button
            key={item.path}
            className={`${styles.navBtn} ${pathname === item.path ? styles.active : ""}`}
            onClick={() => router.push(item.path)}
            title={item.label}
          >
            {item.icon}
          </button>
        ))}
      </div>
      <div className={styles.navBottom}>
        <ProfilePopoverView />
      </div>
    </nav>
  )
}
