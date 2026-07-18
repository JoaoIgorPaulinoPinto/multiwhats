"use client"

import { useEffect, useState } from "react"
import { api, type Contact } from "../../services/api"

export function useChatSidebar() {
  const [search, setSearch] = useState("")
  const [contacts, setContacts] = useState<Contact[]>([])

  useEffect(() => {
    console.log(`[ChatSidebar] carregando contatos...`)
    api.get<Contact[]>("/api/contacts").then((c) => {
      console.log(`[ChatSidebar] ${c.length} contatos carregados`)
      setContacts(c)
    }).catch((e) => console.error(`[ChatSidebar] erro ao carregar contatos:`, e))
  }, [])

  const filtered = contacts.filter((c) => {
    const q = search.toLowerCase()
    const display = c.name ?? c.pushName ?? c.phoneNumber
    return display.toLowerCase().includes(q)
  })

  return { search, setSearch, contacts: filtered }
}
