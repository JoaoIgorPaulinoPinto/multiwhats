"use client"

import { Pencil, Save, UserCog, X, Shield } from "lucide-react"
import { useUsers } from "./users.logic"
import { ROLE_LABELS } from "../../../services/users.service"
import styles from "./users.module.css"

const ROLES: ("Support" | "Dev" | "Admin")[] = ["Support", "Dev", "Admin"]

function formatDate(iso: string): string {
  if (!iso) return "—"
  const d = new Date(iso)
  return d.toLocaleDateString("pt-BR")
}

export function UsersView({ embedded = false }: { embedded?: boolean }) {
  const { users, loading, saving, editingId, form, setForm, startEdit, cancelEdit, saveEdit } = useUsers()

  const body = (
    <>
      {loading ? (
        <div className={styles.skeletonList}>
          {Array.from({ length: 3 }).map((_, i) => (
            <div key={i} className="skeleton" style={{ height: 56, width: "100%" }} />
          ))}
        </div>
      ) : (
        <div className={styles.tableWrap}>
          <table className={styles.table}>
            <thead>
              <tr>
                <th>Nome</th>
                <th>Perfil</th>
                <th>Status</th>
                <th>Criado em</th>
                <th className={styles.colActions}></th>
              </tr>
            </thead>
            <tbody>
              {users.map((user) => (
                <tr key={user.id}>
                  <td className={styles.userCell}>
                    <div className={styles.avatar}>
                      <Shield size={16} />
                    </div>
                    <span className={styles.userName}>{user.name}</span>
                  </td>
                  <td>
                    <span className={`${styles.roleBadge} ${styles[user.role] ?? ""}`}>
                      {ROLE_LABELS[user.role] ?? user.role}
                    </span>
                  </td>
                  <td>
                    <span className={`${styles.statusBadge} ${user.isActive ? styles.active : styles.inactive}`}>
                      {user.isActive ? "Ativo" : "Inativo"}
                    </span>
                  </td>
                  <td className={styles.muted}>{formatDate(user.createdAt)}</td>
                  <td className={styles.colActions}>
                    <button
                      className={styles.editBtn}
                      onClick={() => (editingId === user.id ? cancelEdit() : startEdit(user))}
                      title={editingId === user.id ? "Cancelar" : "Editar"}
                    >
                      {editingId === user.id ? <X size={16} /> : <Pencil size={16} />}
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>

          {editingId !== null && form && (
            <div className={styles.editPanel}>
              <h3 className={styles.editTitle}>Editar usuário</h3>
              <div className={styles.editGrid}>
                <div className={styles.editField}>
                  <label className={styles.editLabel}>Nome</label>
                  <input
                    className={styles.editInput}
                    value={form.name}
                    onChange={(e) => setForm({ ...form, name: e.target.value })}
                  />
                </div>
                <div className={styles.editField}>
                  <label className={styles.editLabel}>Nova senha (deixe em branco para manter)</label>
                  <input
                    className={styles.editInput}
                    type="password"
                    value={form.newPassword}
                    onChange={(e) => setForm({ ...form, newPassword: e.target.value })}
                  />
                </div>
                <div className={styles.editField}>
                  <label className={styles.editLabel}>Perfil</label>
                  <select
                    className={styles.editInput}
                    value={form.role}
                    onChange={(e) => setForm({ ...form, role: e.target.value as typeof form.role })}
                  >
                    {ROLES.map((r) => (
                      <option key={r} value={r}>
                        {ROLE_LABELS[r]}
                      </option>
                    ))}
                  </select>
                </div>
                <label className={styles.editCheckbox}>
                  <input
                    type="checkbox"
                    checked={form.isActive}
                    onChange={(e) => setForm({ ...form, isActive: e.target.checked })}
                  />
                  Usuário ativo
                </label>
              </div>
              <div className={styles.editActions}>
                <button className={styles.saveBtn} onClick={() => saveEdit(editingId)} disabled={saving}>
                  {saving ? <span className="spinner" style={{ width: 16, height: 16 }} /> : <Save size={16} />}
                  {saving ? "Salvando..." : "Salvar"}
                </button>
                <button className={styles.cancelBtn} onClick={cancelEdit}>
                  Cancelar
                </button>
              </div>
            </div>
          )}
        </div>
      )}
    </>
  )

  if (embedded) {
    return <div className={styles.embedded}>{body}</div>
  }

  return (
    <div className={styles.page}>
      <header className={styles.header}>
        <div className={styles.headerRow}>
          <div className={styles.headerTitle}>
            <div className={styles.headerIcon}>
              <UserCog size={20} />
            </div>
            <div>
              <h2>Usuários</h2>
              <p className={styles.subtitle}>
                Gerencie nome, senha, perfil e status de cada usuário do
                sistema.
              </p>
            </div>
          </div>
        </div>
      </header>

      <section className={styles.content}>{body}</section>
    </div>
  )
}
