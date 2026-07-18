"use client"

import { useState } from "react"
import { AuthProvider, useAuth } from "../contexts/auth-context"
import { NavBarView } from "../components/nav-bar/nav-bar.view"
import type { Screen } from "../components/nav-bar/nav-bar.logic"
import { LoginView } from "../components/auth/login.view"
import { ChatsView } from "../components/screens/chats/chats.view"
import { ContactsView } from "../components/screens/contacts/contacts.view"
import { KanbanView } from "../components/screens/kanban/kanban.view"
import { CompaniesView } from "../components/screens/companies/companies.view"
import styles from "./page.module.css"

function App() {
  const { user } = useAuth()
  const [screen, setScreen] = useState<Screen>("chats")

  if (!user) return <LoginView />

  return (
    <div className={styles.page}>
      <NavBarView activeScreen={screen} onNavigate={setScreen} />
      {screen === "chats" && <ChatsView />}
      {screen === "contacts" && <ContactsView />}
      {screen === "kanban" && <KanbanView />}
      {screen === "companies" && <CompaniesView />}
    </div>
  )
}

export default function Home() {
  return (
    <AuthProvider>
      <App />
    </AuthProvider>
  )
}
