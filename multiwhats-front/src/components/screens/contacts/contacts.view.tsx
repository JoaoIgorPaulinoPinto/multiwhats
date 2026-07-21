"use client"

import { Info, Pencil, Phone, Search, Trash2, X } from "lucide-react"
import { useContacts } from "./contacts.logic"
import { AvatarView } from "../../../components/avatar/avatar.view"
import styles from "./contacts.module.css"

export function ContactsView() {
  const {
    contacts,
    clients,
    loading,
    saving,
    deleting,
    search,
    setSearch,
    formName,
    formPushName,
    assignClientId,
    setFormName,
    setAssignClientId,
    startEdit,
    cancelEdit,
    saveEdit,
    deleteContact,
    modalOpen,
  } = useContacts()

  const clientName = (id: number | null) =>
    id ? clients.find((c) => c.id === id)?.name ?? "—" : "—"

  return (
    <div className={styles.page}>
      <aside className={styles.sidebar}>
        <header className={styles.header}>
          <h2>Contatos</h2>
          <div className={styles.headerActions}>
            <button className={styles.addBtnDisabled} disabled title="Crie contatos acessando um chat e clicando em 'Salvar em contatos'">
              <Info size={18} />
              Novo
            </button>
          </div>
          <div className={styles.createHint}>
            Para criar um novo contato, acesse um chat e clique em &quot;Salvar em contatos&quot;.
          </div>
          <div className={styles.search}>
            <Search size={18} />
            <input placeholder="Pesquisar contato" value={search} onChange={(e) => setSearch(e.target.value)} />
          </div>
        </header>

        <section className={styles.list}>
          {loading ? (
            <div className={styles.skeletonList}>
              {Array.from({ length: 5 }).map((_, i) => (
                <div key={i} className={styles.skeletonItem}>
                  <div className="skeleton" style={{ width: 44, height: 44, borderRadius: "50%", flexShrink: 0 }} />
                  <div style={{ flex: 1, display: "flex", flexDirection: "column", gap: 6 }}>
                    <div className="skeleton" style={{ height: 14, width: "60%" }} />
                    <div className="skeleton" style={{ height: 11, width: "40%" }} />
                  </div>
                </div>
              ))}
            </div>
          ) : contacts.length === 0 ? (
            <p className={styles.loading}>Nenhum contato encontrado</p>
          ) : (
            contacts.map((contact) => (
              <div key={contact.id} className={`${styles.item} ${deleting === contact.id ? styles.itemDeleting : ""}`}>
                {contact.profilePicUrl ? (
                  <img src={contact.profilePicUrl} alt="" className={styles.avatarImg} />
                ) : (
                  <AvatarView name={contact.name ?? contact.pushName ?? contact.phoneNumber} />
                )}
                <div className={styles.info}>
                  <strong>{contact.name ?? contact.pushName ?? contact.phoneNumber}</strong>
                  <span>{clientName(contact.clientId)}</span>
                </div>
                <button className={styles.editBtn} onClick={() => startEdit(contact)} disabled={deleting === contact.id}>
                  <Pencil size={18} />
                </button>
                <button className={styles.deleteBtn} onClick={() => deleteContact(contact.id)} disabled={deleting === contact.id}>
                  {deleting === contact.id ? <span className="spinner spinnerDark" /> : <Trash2 size={18} />}
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

      {modalOpen && (
        <>
          <div className={styles.overlay} onClick={cancelEdit} />
          <div className={`${styles.modal} fadeIn`}>
            <div className={styles.modalHeader}>
              <h3>Editar contato</h3>
              <button className={styles.closeBtn} onClick={cancelEdit}>
                <X size={20} />
              </button>
            </div>

            <div className={styles.field}>
              <label>Nome</label>
              <input value={formName} onChange={(e) => setFormName(e.target.value)} disabled={saving} />
            </div>

            <div className={styles.field}>
              <label>Push Name (WhatsApp)</label>
              <input value={formPushName} readOnly />
            </div>

            <div className={styles.field}>
              <label>Empresa</label>
              <select
                className={styles.select}
                value={assignClientId ?? ""}
                onChange={(e) => setAssignClientId(e.target.value ? Number(e.target.value) : null)}
                disabled={saving}
              >
                <option value="">Sem empresa</option>
                {clients.map((c) => (
                  <option key={c.id} value={c.id}>{c.name}</option>
                ))}
              </select>
            </div>

            <div className={styles.modalActions}>
              <button className={styles.cancelBtn} onClick={cancelEdit} disabled={saving}>Cancelar</button>
              <button className={styles.saveBtn} onClick={saveEdit} disabled={saving}>
                {saving ? (
                  <span className={styles.btnLoading}>
                    <span className="spinner" />
                    Salvando...
                  </span>
                ) : "Salvar"}
              </button>
            </div>
          </div>
        </>
      )}
    </div>
  )
}
