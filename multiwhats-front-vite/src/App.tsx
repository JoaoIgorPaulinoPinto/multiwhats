import { useEffect } from "react"
import { BrowserRouter, Navigate, Route, Routes, useNavigate } from "react-router-dom"
import { LoginView } from "./components/auth/login.view"
import { AuthenticatedLayout } from "./components/layout/authenticated-layout"
import { ChatsView } from "./components/screens/chats/chats.view"
import { CompaniesView } from "./components/screens/companies/companies.view"
import { ContactsView } from "./components/screens/contacts/contacts.view"
import { KanbanView } from "./components/screens/kanban/kanban.view"
import { ReportsView } from "./components/screens/reports/reports.view"
import { SettingsView } from "./components/screens/settings/settings.view"
import { useAuthStore } from "./stores/auth-store"
import { UnreadTracker } from "./unread-tracker"

function RootRedirect() {
  const user = useAuthStore((s) => s.user)
  const loading = useAuthStore((s) => s.loading)
  const navigate = useNavigate()

  useEffect(() => {
    if (loading) return
    if (user) {
      navigate("/chats", { replace: true })
    } else {
      navigate("/login", { replace: true })
    }
  }, [user, loading, navigate])
  return null
}

function LoginPage() {
  const user = useAuthStore((s) => s.user)
  const loading = useAuthStore((s) => s.loading)
  const navigate = useNavigate()

  useEffect(() => {
    if (loading) return
    if (user) {
      navigate("/chats", { replace: true })
    }
  }, [user, loading, navigate])

  if (loading) return null
  if (user) return null

  return <LoginView />
}

function ReportPage() {
  const user = useAuthStore((s) => s.user)
  const loading = useAuthStore((s) => s.loading)
  const navigate = useNavigate()

  useEffect(() => {
    if (loading) return
    if (user) {
      navigate("/relatorio", { replace: true })
    }
  }, [user, loading, navigate])

  if (loading) return null
  if (user) return null
  return <ReportsView />
}

export function App() {
  return (
    <BrowserRouter>
      <UnreadTracker />
      <Routes>
        <Route path="/" element={<RootRedirect />} />
        <Route path="/login" element={<LoginPage />} />
        <Route path="/report" element={<ReportPage />} />
        <Route element={<AuthenticatedLayout />}>
          <Route path="/chats" element={<ChatsView />} />
          <Route path="/contacts" element={<ContactsView />} />
          <Route path="/kanban" element={<KanbanView />} />
          <Route path="/companies" element={<CompaniesView />} />
          <Route path="/relatorio" element={<ReportsView />} />
          <Route path="/settings" element={<SettingsView />} />
          <Route path="/users" element={<Navigate to="/settings" replace />} />
        </Route>
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </BrowserRouter>
  )
}
