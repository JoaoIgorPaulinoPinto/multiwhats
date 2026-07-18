import { Building2, LayoutDashboard, MessageSquare, Users } from "lucide-react"
import type { Screen } from "./nav-bar.logic"
import { ProfilePopoverView } from "../profile-popover/profile-popover.view"
import styles from "./nav-bar.module.css"

interface Props {
  activeScreen: Screen
  onNavigate: (screen: Screen) => void
}

const items: { screen: Screen; icon: React.ReactNode; label: string }[] = [
  { screen: "chats", icon: <MessageSquare size={22} />, label: "Chats" },
  { screen: "contacts", icon: <Users size={22} />, label: "Contatos" },
  { screen: "kanban", icon: <LayoutDashboard size={22} />, label: "Kanban" },
  { screen: "companies", icon: <Building2 size={22} />, label: "Empresas" },
]

export function NavBarView({ activeScreen, onNavigate }: Props) {
  return (
    <nav className={styles.nav}>
      <div className={styles.navTop}>
        {items.map((item) => (
          <button
            key={item.screen}
            className={`${styles.navBtn} ${activeScreen === item.screen ? styles.active : ""}`}
            onClick={() => onNavigate(item.screen)}
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
