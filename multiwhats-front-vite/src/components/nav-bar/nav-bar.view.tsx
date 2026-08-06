import { Building2, FileChartColumn, LayoutDashboard, MessageSquare, Settings, Users } from "lucide-react"
import { useLocation, useNavigate } from "react-router-dom"
import { useUnreadStore } from "../../stores/unread-store"
import { ProfilePopoverView } from "../profile-popover/profile-popover.view"
import styles from "./nav-bar.module.css"

const items: { path: string; icon: React.ReactNode; label: string }[] = [
  { path: "/chats", icon: <MessageSquare size={22} />, label: "Chats" },
  { path: "/contacts", icon: <Users size={22} />, label: "Contatos" },
  { path: "/kanban", icon: <LayoutDashboard size={22} />, label: "Kanban" },
  { path: "/companies", icon: <Building2 size={22} />, label: "Empresas" },
  { path: "/relatorio", icon: <FileChartColumn size={22} />, label: "Relatórios" },
  { path: "/settings", icon: <Settings size={22} />, label: "Configurações" },
]

export function NavBarView() {
  const pathname = useLocation().pathname
  const navigate = useNavigate()
  const unread = useUnreadStore((s) => s.total)

  return (
    <nav className={styles.nav}>
      <div className={styles.navTop}>
        <img src="/logo.png" alt="Logo" className={styles.logo} />
        {items.map((item) => (
          <button
            key={item.path}
            className={`${styles.navBtn} ${pathname === item.path ? styles.active : ""}`}
            onClick={() => navigate(item.path)}
            title={item.label}
          >
            {item.icon}
            {item.path === "/chats" && unread > 0 && (
              <span className={styles.badge}>{unread > 99 ? "99+" : unread}</span>
            )}
          </button>
        ))}
      </div>
      <div className={styles.navBottom}>
        <ProfilePopoverView />
      </div>
    </nav>
  )
}
