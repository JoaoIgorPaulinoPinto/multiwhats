"use client"

import { useState } from "react"
import { useAuthStore } from "../../stores/auth-store"

export function useLogin() {
  const login = useAuthStore((s) => s.login)
  const register = useAuthStore((s) => s.register)
  const loading = useAuthStore((s) => s.loading)
  const [mode, setMode] = useState<"login" | "register">("login")
  const [name, setName] = useState("")
  const [password, setPassword] = useState("")
  const [error, setError] = useState("")

  async function handleSubmit() {
    setError("")
    try {
      if (mode === "login") {
        await login(name, password)
      } else {
        await register(name, password)
        await login(name, password)
      }
    } catch (e: unknown) {
      setError(e instanceof Error ? e.message : "Erro desconhecido")
    }
  }

  function toggleMode() {
    setMode(mode === "login" ? "register" : "login")
    setError("")
  }

  return { mode, name, password, error, loading, setName, setPassword, handleSubmit, toggleMode }
}
