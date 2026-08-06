
import { Pencil, Phone, Plus, Search, Trash2, Users, X } from 'lucide-react'
import { AvatarView } from '../../../components/avatar/avatar.view'
import { SearchableSelect } from '../../../components/searchable-select/searchable-select.view'
import type { ChatListResponse } from '../../../services/chats.service'
import { useContacts } from './contacts.logic'
import styles from './contacts.module.css'

function chatLabel(chat: ChatListResponse): string {
  const name =
    chat.contactName ?? chat.name ?? chat.phoneNumber ?? `Chat #${chat.id}`
  return chat.clientName ? `${name} — ${chat.clientName}` : name
}

export function ContactsView() {
  const {
    contacts,
    clients,
    chats,
    loading,
    saving,
    deleting,
    search,
    setSearch,
    creating,
    formChatId,
    formName,
    formPushName,
    formPhone,
    assignClientId,
    selectChat,
    setFormName,
    setFormPhone,
    setAssignClientId,
    startCreate,
    startEdit,
    cancelEdit,
    saveEdit,
    createContact,
    deleteContact,
    modalOpen,
  } = useContacts()

  const clientName = (id: number | null) =>
    id ? (clients.find((c) => c.id === id)?.name ?? '—') : '—'

  return (
    <div className={styles.page}>
      <aside className={styles.sidebar}>
        <header className={styles.header}>
          <div className={styles.headerRow}>
            <div className={styles.headerTitle}>
              <div className={styles.headerIcon}>
                <Users size={20} />
              </div>
              <div>
                <h2>Contatos</h2>
                <p className={styles.subtitle}>
                  Contatos salvos e seus vínculos com empresas.
                </p>
              </div>
            </div>
            <div className={styles.headerActions}>
              <button className={styles.addBtn} onClick={startCreate}>
                <Plus size={18} />
                Novo
              </button>
            </div>
          </div>
          <div className={styles.search}>
            <Search size={18} />
            <input
              placeholder="Pesquisar contato"
              value={search}
              onChange={(e) => setSearch(e.target.value)}
            />
          </div>
        </header>

        <section className={styles.list}>
          {loading ? (
            <div className={styles.skeletonList}>
              {Array.from({ length: 5 }).map((_, i) => (
                <div key={i} className={styles.skeletonItem}>
                  <div
                    className="skeleton"
                    style={{
                      width: 44,
                      height: 44,
                      borderRadius: '50%',
                      flexShrink: 0,
                    }}
                  />
                  <div
                    style={{
                      flex: 1,
                      display: 'flex',
                      flexDirection: 'column',
                      gap: 6,
                    }}
                  >
                    <div
                      className="skeleton"
                      style={{ height: 14, width: '60%' }}
                    />
                    <div
                      className="skeleton"
                      style={{ height: 11, width: '40%' }}
                    />
                  </div>
                </div>
              ))}
            </div>
          ) : contacts.length === 0 ? (
            <p className={styles.loading}>Nenhum contato encontrado</p>
          ) : (
            contacts.map((contact) => (
              <div
                key={contact.id}
                className={`${styles.item} ${deleting === contact.id ? styles.itemDeleting : ''}`}
              >
                {contact.profilePicUrl ? (
                  <img
                    src={contact.profilePicUrl}
                    alt=""
                    className={styles.avatarImg}
                  />
                ) : (
                  <AvatarView
                    name={
                      contact.name ?? contact.pushName ?? contact.phoneNumber
                    }
                  />
                )}
                <div className={styles.info}>
                  <strong>
                    {contact.name ?? contact.pushName ?? contact.phoneNumber}
                  </strong>
                  <span>{clientName(contact.clientId)}</span>
                </div>
                <button
                  className={styles.editBtn}
                  onClick={() => startEdit(contact)}
                  disabled={deleting === contact.id}
                >
                  <Pencil size={18} />
                </button>
                <button
                  className={styles.deleteBtn}
                  onClick={() => deleteContact(contact.id)}
                  disabled={deleting === contact.id}
                >
                  {deleting === contact.id ? (
                    <span className="spinner spinnerDark" />
                  ) : (
                    <Trash2 size={18} />
                  )}
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
              <h3>{creating ? 'Novo contato' : 'Editar contato'}</h3>
              <button className={styles.closeBtn} onClick={cancelEdit}>
                <X size={20} />
              </button>
            </div>

            {creating && (
              <div className={styles.field}>
                <label>Chat *</label>
                <SearchableSelect
                  options={chats.map((chat) => ({
                    id: chat.id,
                    name: chatLabel(chat),
                  }))}
                  value={formChatId === '' ? null : formChatId}
                  onChange={(v) => selectChat(v ?? '')}
                  emptyLabel="Selecione um chat"
                  placeholder="Selecione um chat"
                  disabled={saving}
                />
              </div>
            )}

            {creating && (
              <div className={styles.field}>
                <label>Telefone</label>
                <input
                  value={formPhone}
                  onChange={(e) => setFormPhone(e.target.value)}
                  disabled={saving}
                />
              </div>
            )}

            <div className={styles.field}>
              <label>Nome</label>
              <input
                value={formName}
                onChange={(e) => setFormName(e.target.value)}
                disabled={saving}
              />
            </div>

            <div className={styles.field}>
              <label>Push Name (WhatsApp)</label>
              <input value={formPushName} readOnly />
            </div>

            <div className={styles.field}>
              <label>Empresa</label>
              <SearchableSelect
                options={clients}
                value={assignClientId}
                onChange={setAssignClientId}
                emptyLabel="Sem empresa"
                placeholder="Selecione uma empresa"
                disabled={saving}
              />
            </div>

            <div className={styles.modalActions}>
              <button
                className={styles.cancelBtn}
                onClick={cancelEdit}
                disabled={saving}
              >
                Cancelar
              </button>
              <button
                className={styles.saveBtn}
                onClick={creating ? createContact : saveEdit}
                disabled={
                  saving || (creating && (formChatId === '' || !formPhone))
                }
              >
                {saving ? (
                  <span className={styles.btnLoading}>
                    <span className="spinner" />
                    Salvando...
                  </span>
                ) : creating ? (
                  'Criar contato'
                ) : (
                  'Salvar'
                )}
              </button>
            </div>
          </div>
        </>
      )}
    </div>
  )
}
