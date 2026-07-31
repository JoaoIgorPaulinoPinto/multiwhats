"use client"

import { useCallback, useEffect, useState } from "react"
import { usersService, type UserResponse, type UserRole } from "../../../services/users.service"
import { useToast } from "../../../components/toast/toast.provider"

export interface UserForm {
  name: string
  newPassword: string
  role: UserRole
  isActive: boolean
}

export function useUsers() {
  const { toast } = useToast()

  const [users, setUsers] = useState<UserResponse[]>([])
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [editingId, setEditingId] = useState<number | null>(null)
  const [form, setForm] = useState<UserForm | null>(null)

  const load = useCallback(() =>
    usersService.list().then((data) => setUsers(data)),
  [])

  useEffect(() => {
    load().catch((e) => console.error("[Users] erro ao carregar:", e)).finally(() => setLoading(false))
  }, [load])

  function startEdit(user: UserResponse) {
    setEditingId(user.id)
    setForm({
      name: user.name,
      newPassword: "",
      role: (user.role as UserRole) ?? "Support",
      isActive: user.isActive,
    })
  }

  function cancelEdit() {
    setEditingId(null)
    setForm(null)
  }

  async function saveEdit(id: number) {
    if (!form) return
    setSaving(true)
    try {
      await usersService.update(id, {
        name: form.name,
        newPassword: form.newPassword || undefined,
        role: form.role,
        isActive: form.isActive,
      })
      toast.success("Usuário atualizado")
      await load()
      cancelEdit()
    } catch (e) {
      console.error("[Users] erro ao salvar:", e)
      toast.error("Erro ao atualizar usuário")
    } finally {
      setSaving(false)
    }
  }

  return {
    users,
    loading,
    saving,
    editingId,
    form,
    setForm,
    startEdit,
    cancelEdit,
    saveEdit,
  }
}
