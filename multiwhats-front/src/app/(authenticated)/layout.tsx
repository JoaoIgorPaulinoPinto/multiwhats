"use client"

import { useEffect } from "react"
import { useRouter } from "next/navigation"
import { useAuthStore } from "../../stores/auth-store"
import { NavBarView } from "../../components/nav-bar/nav-bar.view"
import styles from "./layout.module.css"

export default function AuthenticatedLayout({
  children,
}: {
  children: React.ReactNode
}) {
  const user = useAuthStore((s) => s.user)
  const loading = useAuthStore((s) => s.loading)
  const router = useRouter()

  useEffect(() => {
    if (loading) return
    if (!user) {
      router.replace("/login")
    }
  }, [user, loading, router])

  if (loading) return null
  if (!user) return null

  return (
    <div className={styles.layout}>
      <NavBarView />
      {children}
    </div>
  )
}
