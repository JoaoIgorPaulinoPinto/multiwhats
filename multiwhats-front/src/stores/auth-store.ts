"use client"

import { create } from "zustand"
import { api, type LoginResponse } from "../services/api"

interface UserInfo {
  id: number
  name: string
  role: string
}

interface AuthState {
  user: UserInfo | null
  token: string | null
  loading: boolean
  login: (name: string, password: string) => Promise<void>
  register: (name: string, password: string) => Promise<void>
  logout: () => Promise<void>
}

function getStoredUser(): UserInfo | null {
  if (typeof window === "undefined") return null
  const raw = localStorage.getItem("user")
  return raw ? JSON.parse(raw) : null
}

function getStoredToken(): string | null {
  if (typeof window === "undefined") return null
  return localStorage.getItem("token")
}

export const useAuthStore = create<AuthState>((set) => ({
  user: getStoredUser(),
  token: getStoredToken(),
  loading: false,

  login: async (name: string, password: string) => {
    console.log(`[Auth] login: "${name}"`)
    set({ loading: true })
    try {
      const res = await api.post<LoginResponse>("/api/auth/login", { name, password })
      console.log(`[Auth] login OK — user: ${res.user.name}, role: ${res.user.role}`)
      localStorage.setItem("token", res.token)
      localStorage.setItem("user", JSON.stringify(res.user))
      set({ token: res.token, user: res.user, loading: false })
    } catch (e) {
      console.error(`[Auth] login falhou:`, e)
      set({ loading: false })
      throw e
    }
  },

  register: async (name: string, password: string) => {
    console.log(`[Auth] register: "${name}"`)
    set({ loading: true })
    try {
      await api.post("/api/auth/register", { name, password })
      console.log(`[Auth] register OK`)
    } catch (e) {
      console.error(`[Auth] register falhou:`, e)
      throw e
    } finally {
      set({ loading: false })
    }
  },

  logout: async () => {
    console.log(`[Auth] logout`)
    try {
      await api.post("/api/auth/logout")
      console.log(`[Auth] logout OK`)
    } catch (e) {
      console.warn(`[Auth] logout ignorou erro:`, e)
    }
    localStorage.removeItem("token")
    localStorage.removeItem("user")
    set({ token: null, user: null })
  },
}))
