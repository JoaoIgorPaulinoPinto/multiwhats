import { create } from "zustand"

interface UnreadState {
  counted: Record<string, number>
  total: number
  chatCount: number
  perChat: Record<number, number>
  addIncoming: (messageKey: string, chatId: number) => void
  removeRead: (messageKey: string) => void
  clearChat: (chatId: number) => void
  seed: (counted: Record<string, number>) => void
  clearAll: () => void
}

function compute(counted: Record<string, number>) {
  let total = 0
  const perChat: Record<number, number> = {}
  for (const chatId of Object.values(counted)) {
    perChat[chatId] = (perChat[chatId] ?? 0) + 1
    total += 1
  }
  const chatCount = Object.keys(perChat).length
  return { total, chatCount, perChat }
}

export const useUnreadStore = create<UnreadState>((set) => ({
  counted: {},
  total: 0,
  chatCount: 0,
  perChat: {},

  addIncoming: (messageKey, chatId) => {
    if (!messageKey || chatId == null) return
    set((s) => {
      if (s.counted[messageKey]) return s
      const counted = { ...s.counted, [messageKey]: chatId }
      return { counted, ...compute(counted) }
    })
  },

  removeRead: (messageKey) => {
    if (!messageKey) return
    set((s) => {
      if (!(messageKey in s.counted)) return s
      const counted = { ...s.counted }
      delete counted[messageKey]
      return { counted, ...compute(counted) }
    })
  },

  clearChat: (chatId) => {
    set((s) => {
      if (!s.perChat[chatId]) return s
      const counted: Record<string, number> = {}
      for (const [key, id] of Object.entries(s.counted)) {
        if (id !== chatId) counted[key] = id
      }
      return { counted, ...compute(counted) }
    })
  },

  seed: (counted) => {
    set((s) => {
      const validChatIds = new Set<number>(Object.values(counted))
      const merged: Record<string, number> = {}
      for (const [key, id] of Object.entries(counted)) {
        if (key) merged[key] = id
      }
      for (const [key, id] of Object.entries(s.counted)) {
        if (merged[key] === undefined && validChatIds.has(id)) merged[key] = id
      }
      return { counted: merged, ...compute(merged) }
    })
  },

  clearAll: () => set({ counted: {}, total: 0, chatCount: 0, perChat: {} }),
}))
