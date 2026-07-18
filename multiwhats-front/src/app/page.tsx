"use client"

import { useRouter } from "next/navigation"
import { useEffect } from "react"
import { useAuth } from "../contexts/auth-context"

export default function Home() {
  const { user, loading } = useAuth()
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
