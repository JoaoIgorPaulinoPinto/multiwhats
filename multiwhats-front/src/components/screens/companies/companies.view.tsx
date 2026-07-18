"use client"

import { Pencil, Plus, Search, Trash2, X } from "lucide-react"
import { useCompanies } from "./companies.logic"
import { AvatarView } from "../../../components/avatar/avatar.view"
import styles from "./companies.module.css"

export function CompaniesView() {
  const {
    companies,
    loading,
    search,
    setSearch,
    editing,
    formName,
    formPhone,
    formStatus,
    setFormName,
    setFormPhone,
    setFormStatus,
    startEdit,
    cancelEdit,
    saveEdit,
    createCompany,
    deleteCompany,
    companyContacts,
    unassignContact,
  } = useCompanies()

  return (
    <div className={styles.page}>
      <header className={styles.header}>
        <div className={styles.headerRow}>
          <h2>Empresas</h2>
          <button className={styles.newBtn} onClick={() => startEdit({ id: 0, name: "", mainPhoneNumber: null, status: "Active" } as never)}>
            <Plus size={18} />
            Nova empresa
          </button>
        </div>
        <div className={styles.search}>
          <Search size={18} />
          <input placeholder="Pesquisar empresa" value={search} onChange={(e) => setSearch(e.target.value)} />
        </div>
      </header>

      <section className={styles.list}>
        {loading ? (
          <p className={styles.loading}>Carregando...</p>
        ) : companies.length === 0 ? (
          <p className={styles.loading}>Nenhuma empresa encontrada</p>
        ) : (
          companies.map((company) => {
            const contacts = companyContacts(company.id)
            return (
              <div key={company.id} className={styles.card}>
                <div className={styles.cardLeft}>
              <AvatarView name={company.name} size={44} fontSize={18} square />
                  <div className={styles.info}>
                    <strong>{company.name}</strong>
                    <span>{company.mainPhoneNumber ?? "Sem telefone"}</span>
                  </div>
                </div>
                <div className={styles.cardRight}>
                  <span className={styles.contactsCount}>{contacts.length} contatos</span>
                  <span className={`${styles.status} ${company.status === "Active" ? styles.activeStatus : styles.inactiveStatus}`}>
                    {company.status === "Active" ? "Ativo" : "Inativo"}
                  </span>
                  <button className={styles.editBtn} onClick={() => startEdit(company)}>
                    <Pencil size={18} />
                  </button>
                  <button className={styles.deleteBtn} onClick={() => deleteCompany(company.id)}>
                    <Trash2 size={18} />
                  </button>
                </div>
              </div>
            )
          })
        )}
      </section>

      {editing && (
        <>
          <div className={styles.overlay} onClick={cancelEdit} />
          <div className={styles.modal}>
            <div className={styles.modalHeader}>
              <h3>{editing.id ? "Editar empresa" : "Nova empresa"}</h3>
              <button className={styles.closeBtn} onClick={cancelEdit}>
                <X size={20} />
              </button>
            </div>

            <div className={styles.field}>
              <label>Nome</label>
              <input value={formName} onChange={(e) => setFormName(e.target.value)} />
            </div>

            <div className={styles.field}>
              <label>Telefone</label>
              <input value={formPhone} onChange={(e) => setFormPhone(e.target.value)} />
            </div>

            <div className={styles.field}>
              <label>Status</label>
              <select
                className={styles.select}
                value={formStatus}
                onChange={(e) => setFormStatus(e.target.value)}
              >
                <option value="Active">Ativo</option>
                <option value="Inactive">Inativo</option>
              </select>
            </div>

            {editing.id ? (
              <div className={styles.field}>
                <label>Contatos vinculados</label>
                <div className={styles.contactList}>
                  {companyContacts(editing.id).length === 0 ? (
                    <span className={styles.noContacts}>Nenhum contato vinculado</span>
                  ) : (
                    companyContacts(editing.id).map((contact) => (
                      <div key={contact.id} className={styles.contactRow}>
                        <div className={styles.contactInfo}>
                          <strong>{contact.name ?? contact.pushName ?? contact.phoneNumber}</strong>
                          <span>{contact.phoneNumber}</span>
                        </div>
                        <button className={styles.removeBtn} onClick={() => unassignContact(contact.id)}>
                          <Trash2 size={16} />
                        </button>
                      </div>
                    ))
                  )}
                </div>
              </div>
            ) : null}

            <div className={styles.modalActions}>
              <button className={styles.cancelBtn} onClick={cancelEdit}>Cancelar</button>
              <button className={styles.saveBtn} onClick={editing.id ? saveEdit : createCompany}>
                {editing.id ? "Salvar" : "Criar"}
              </button>
            </div>
          </div>
        </>
      )}
    </div>
  )
}
