"use client"

import { useEffect } from "react"
import { useRouter } from "next/navigation"
import { useAuth } from "../../contexts/auth-context"
import { LoginView } from "../../components/auth/login.view"

export default function LoginPage() {
  const { user, loading } = useAuth()
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
