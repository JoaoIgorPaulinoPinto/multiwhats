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
  const [registrationCode, setRegistrationCode] = useState("")
  const [error, setError] = useState("")

  async function handleSubmit() {
    setError("")
    try {
      if (mode === "login") {
        await login(name, password)
      } else {
        await register(name, password, registrationCode.trim() || undefined)
        await login(name, password)
      }
    } catch (e: unknown) {
      const message = e instanceof Error ? e.message : "Erro desconhecido"
      setError(mode === "login" ? `Falha no login: ${message}` : `Falha no cadastro: ${message}`)
    }
  }

  function toggleMode() {
    setMode(mode === "login" ? "register" : "login")
    setError("")
    setRegistrationCode("")
  }

  return { mode, name, password, registrationCode, error, loading, setName, setPassword, setRegistrationCode, handleSubmit, toggleMode }
}
