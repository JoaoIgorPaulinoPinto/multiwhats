"use client"

import { Pencil, Phone, Search, Trash2, X } from "lucide-react"
import { useContacts } from "./contacts.logic"
import { AvatarView } from "../../../components/avatar/avatar.view"
import styles from "./contacts.module.css"

export function ContactsView() {
  const {
    contacts,
    clients,
    loading,
    search,
    setSearch,
    editing,
    formName,
    formPushName,
    assignClientId,
    setFormName,
    setFormPushName,
    setAssignClientId,
    startEdit,
    cancelEdit,
    saveEdit,
    deleteContact,
  } = useContacts()

  const clientName = (id: number | null) =>
    id ? clients.find((c) => c.id === id)?.name ?? "—" : "—"

  return (
    <div className={styles.page}>
      <aside className={styles.sidebar}>
        <header className={styles.header}>
          <h2>Contatos</h2>
          <div className={styles.search}>
            <Search size={18} />
            <input placeholder="Pesquisar contato" value={search} onChange={(e) => setSearch(e.target.value)} />
          </div>
        </header>

        <section className={styles.list}>
          {loading ? (
            <p className={styles.loading}>Carregando...</p>
          ) : contacts.length === 0 ? (
            <p className={styles.loading}>Nenhum contato encontrado</p>
          ) : (
            contacts.map((contact) => (
              <div key={contact.id} className={styles.item}>
                {contact.profilePicUrl ? (
                  <img src={contact.profilePicUrl} alt="" className={styles.avatarImg} />
                ) : (
                  <AvatarView name={contact.name ?? contact.pushName ?? contact.phoneNumber} />
                )}
                <div className={styles.info}>
                  <strong>{contact.name ?? contact.pushName ?? contact.phoneNumber}</strong>
                  <span>{clientName(contact.clientId)}</span>
                </div>
                <button className={styles.editBtn} onClick={() => startEdit(contact)}>
                  <Pencil size={18} />
                </button>
                <button className={styles.deleteBtn} onClick={() => deleteContact(contact.id)}>
                  <Trash2 size={18} />
                </button>
              </div>
            ))
          )}
        </section>
      </aside>

      <main className={styles.empty}>
        <Phone size={48} />
        <h3>Selecione um contato</h3>
        <p>Escolha um contato ao lado para ver os detalhes</p>
      </main>

      {editing && (
        <>
          <div className={styles.overlay} onClick={cancelEdit} />
          <div className={styles.modal}>
            <div className={styles.modalHeader}>
              <h3>Editar contato</h3>
              <button className={styles.closeBtn} onClick={cancelEdit}>
                <X size={20} />
              </button>
            </div>
            <div className={styles.field}>
              <label>Nome</label>
              <input value={formName} onChange={(e) => setFormName(e.target.value)} />
            </div>
            <div className={styles.field}>
              <label>Push Name (WhatsApp)</label>
              <input value={formPushName} onChange={(e) => setFormPushName(e.target.value)} />
            </div>
            <div className={styles.field}>
              <label>Empresa</label>
              <select
                className={styles.select}
                value={assignClientId ?? ""}
                onChange={(e) => setAssignClientId(e.target.value ? Number(e.target.value) : null)}
              >
                <option value="">Sem empresa</option>
                {clients.map((c) => (
                  <option key={c.id} value={c.id}>{c.name}</option>
                ))}
              </select>
            </div>
            <div className={styles.modalActions}>
              <button className={styles.cancelBtn} onClick={cancelEdit}>Cancelar</button>
              <button className={styles.saveBtn} onClick={saveEdit}>Salvar</button>
            </div>
          </div>
        </>
      )}
    </div>
  )
}
