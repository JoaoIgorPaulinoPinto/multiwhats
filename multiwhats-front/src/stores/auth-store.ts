"use client"

import { create } from "zustand"
import { authService, type UserResponse, type LoginResponse } from "../services/auth.service"

interface AuthState {
  user: UserResponse | null
  token: string | null
  loading: boolean
  error: string | null
  login: (name: string, password: string) => Promise<void>
  register: (name: string, password: string) => Promise<void>
  logout: () => Promise<void>
  hydrate: () => void
}

export const useAuthStore = create<AuthState>((set) => ({
  user: null,
  token: null,
  loading: false,
  error: null,

  hydrate: () => {
    const rawUser = localStorage.getItem("user")
    const rawToken = localStorage.getItem("token")
    set({
      user: rawUser ? JSON.parse(rawUser) : null,
      token: rawToken ?? null,
    })
  },

  login: async (name: string, password: string) => {
    set({ loading: true, error: null })
    try {
      const res: LoginResponse = await authService.login(name, password)
      localStorage.setItem("token", res.token)
      localStorage.setItem("user", JSON.stringify(res.user))
      set({ token: res.token, user: res.user, loading: false })
    } catch (e) {
      const message = e instanceof Error ? e.message : "Erro ao fazer login"
      set({ loading: false, error: message })
      throw e
    }
  },

  register: async (name: string, password: string) => {
    set({ loading: true, error: null })
    try {
      await authService.register(name, password)
      set({ loading: false })
    } catch (e) {
      const message = e instanceof Error ? e.message : "Erro ao registrar"
      set({ loading: false, error: message })
      throw e
    }
  },

  logout: async () => {
    try {
      await authService.logout()
    } catch {
      // limpa mesmo se a API falhar
    }
    localStorage.removeItem("token")
    localStorage.removeItem("user")
    set({ token: null, user: null, error: null })
  },
}))
