"use client"

import { createContext, useContext, useState, useCallback, type ReactNode } from "react"
import { api, type LoginResponse } from "../services/api"

interface UserInfo {
  id: number
  name: string
  role: string
}

interface AuthContextType {
  user: UserInfo | null
  token: string | null
  loading: boolean
  login: (name: string, password: string) => Promise<void>
  register: (name: string, password: string) => Promise<void>
  logout: () => Promise<void>
}

const AuthContext = createContext<AuthContextType | null>(null)

function getStoredUser(): UserInfo | null {
  if (typeof window === "undefined") return null
  const raw = localStorage.getItem("user")
  return raw ? JSON.parse(raw) : null
}

function getStoredToken(): string | null {
  if (typeof window === "undefined") return null
  return localStorage.getItem("token")
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<UserInfo | null>(getStoredUser)
  const [token, setToken] = useState<string | null>(getStoredToken)
  const [loading, setLoading] = useState(false)

  const login = useCallback(async (name: string, password: string) => {
    console.log(`[Auth] login: "${name}"`)
    setLoading(true)
    try {
      const res = await api.post<LoginResponse>("/api/auth/login", { name, password })
      console.log(`[Auth] login OK — user: ${res.user.name}, role: ${res.user.role}`)
      localStorage.setItem("token", res.token)
      localStorage.setItem("user", JSON.stringify(res.user))
      setToken(res.token)
      setUser(res.user)
    } catch (e) {
      console.error(`[Auth] login falhou:`, e)
      throw e
    } finally {
      setLoading(false)
    }
  }, [])

  const register = useCallback(async (name: string, password: string) => {
    console.log(`[Auth] register: "${name}"`)
    setLoading(true)
    try {
      await api.post("/api/auth/register", { name, password })
      console.log(`[Auth] register OK`)
    } catch (e) {
      console.error(`[Auth] register falhou:`, e)
      throw e
    } finally {
      setLoading(false)
    }
  }, [])

  const logout = useCallback(async () => {
    console.log(`[Auth] logout`)
    try {
      await api.post("/api/auth/logout")
      console.log(`[Auth] logout OK`)
    } catch (e) {
      console.warn(`[Auth] logout ignorou erro:`, e)
    }
    localStorage.removeItem("token")
    localStorage.removeItem("user")
    setToken(null)
    setUser(null)
  }, [])

  return (
    <AuthContext.Provider value={{ user, token, loading, login, register, logout }}>
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth() {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error("useAuth must be used within AuthProvider")
  return ctx
}
