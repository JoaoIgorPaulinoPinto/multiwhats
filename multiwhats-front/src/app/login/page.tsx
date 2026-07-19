"use client"

import { useRouter } from "next/navigation"
import { useEffect } from "react"
import { LoginView } from "../../components/auth/login.view"
import { useAuthStore } from "../../stores/auth-store"

export default function LoginPage() {
  const user = useAuthStore((s) => s.user)
  const loading = useAuthStore((s) => s.loading)
  const router = useRouter()

  useEffect(() => {
    if (loading) return
    if (user) {
      router.replace("/chats")
    }
  }, [user, loading, router])

  if (loading) return null
  if (user) return null

  return <LoginView />
}
