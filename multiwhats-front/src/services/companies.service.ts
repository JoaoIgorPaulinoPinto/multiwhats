import { api } from "./api"

export type ClientStatus = "Active" | "Inactive"

export interface ClientResponse {
  id: number
  name: string
  mainPhoneNumber: string | null
  status: ClientStatus
  contactCount: number
  createdAt: string
  lastUpdate: string
}

export const companiesService = {
  list() {
    return api.get<ClientResponse[]>("/api/clients")
  },

  getById(id: number) {
    return api.get<ClientResponse>(`/api/clients/${id}`)
  },

  create(data: { name: string; mainPhoneNumber?: string | null }) {
    return api.post<ClientResponse>("/api/clients", data)
  },

  update(id: number, data: { name: string; mainPhoneNumber: string | null; status: string }) {
    return api.put<ClientResponse>(`/api/clients/${id}`, data)
  },

  delete(id: number) {
    return api.delete(`/api/clients/${id}`)
  },

  listContacts(clientId: number) {
    return api.get(`/api/clients/${clientId}/contacts`)
  },

  unassignContact(contactId: number) {
    return api.patch(`/api/contacts/${contactId}/unassign`)
  },
}
