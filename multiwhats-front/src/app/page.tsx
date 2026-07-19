"use client"

import { useRouter } from "next/navigation"
import { useEffect } from "react"
import { useAuthStore } from "../stores/auth-store"

export default function Home() {
  const user = useAuthStore((s) => s.user)
  const loading = useAuthStore((s) => s.loading)
  const router = useRouter()

  useEffect(() => {
    if (loading) return
    if (user) {
      router.replace("/chats")
    } else {
      router.replace("/login")
    }
  }, [user, loading, router])
  return null
}
