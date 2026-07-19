"use client"

import { useEffect } from "react"
import { useAuthStore } from "../stores/auth-store"

export function Hydrator() {
  const hydrate = useAuthStore((s) => s.hydrate)

  useEffect(() => {
    hydrate()
  }, [hydrate])

  return null
}
