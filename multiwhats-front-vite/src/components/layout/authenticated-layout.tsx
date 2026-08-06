import { useEffect } from "react"
import { Outlet, useNavigate } from "react-router-dom"
import { useAuthStore } from "../../stores/auth-store"
import { NavBarView } from "../nav-bar/nav-bar.view"
import styles from "./authenticated-layout.module.css"

export function AuthenticatedLayout() {
  const user = useAuthStore((s) => s.user)
  const loading = useAuthStore((s) => s.loading)
  const navigate = useNavigate()

  useEffect(() => {
    if (loading) return
    if (!user) {
      navigate("/login", { replace: true })
    }
  }, [user, loading, navigate])

  if (loading) return null
  if (!user) return null

  return (
    <div className={styles.layout}>
      <NavBarView />
      <Outlet />
    </div>
  )
}
