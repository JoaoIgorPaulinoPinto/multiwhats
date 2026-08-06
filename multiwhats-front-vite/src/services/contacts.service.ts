import { api } from "./api"

export interface ContactResponse {
  id: number
  jid: string
  phoneNumber: string
  name: string | null
  pushName: string | null
  profilePicUrl: string | null
  isBlocked: boolean
  isGroup: boolean
  lastMessageAt: string | null
  clientId: number | null
  clientName: string | null
  groupId: number | null
  groupName: string | null
  createdByUserId: number | null
  createdAt: string
  lastUpdate: string
}

export const contactsService = {
  list() {
    return api.get<ContactResponse[]>("/api/contacts")
  },

  getById(id: number) {
    return api.get<ContactResponse>(`/api/contacts/${id}`)
  },

  create(data: { jid: string; phoneNumber: string; name?: string; pushName?: string }) {
    return api.post<ContactResponse>("/api/contacts", data)
  },

  update(id: number, data: { name?: string; pushName?: string; isBlocked?: boolean }) {
    return api.put<ContactResponse>(`/api/contacts/${id}`, data)
  },

  delete(id: number) {
    return api.delete(`/api/contacts/${id}`)
  },

  assign(id: number, clientId: number) {
    return api.patch(`/api/contacts/${id}/assign`, { clientId })
  },

  unassign(id: number) {
    return api.patch(`/api/contacts/${id}/unassign`)
  },
}
