"use client"

import { LogIn, UserPlus } from "lucide-react"
import { useLogin } from "./login.logic"
import styles from "./login.module.css"

export function LoginView() {
  const { mode, name, password, error, loading, setName, setPassword, handleSubmit, toggleMode } = useLogin()

  return (
    <div className={styles.page}>
      <form className={styles.card} onSubmit={(e) => { e.preventDefault(); handleSubmit() }}>
        <div className={styles.icon}>
          {mode === "login" ? <LogIn size={32} /> : <UserPlus size={32} />}
        </div>
        <h2>{mode === "login" ? "Entrar" : "Criar conta"}</h2>

        {error && <span className={styles.error}>{error}</span>}

        <div className={styles.field}>
          <label>Nome</label>
          <input value={name} onChange={(e) => setName(e.target.value)} placeholder="Seu nome" disabled={loading} />
        </div>

        <div className={styles.field}>
          <label>Senha</label>
          <input type="password" value={password} onChange={(e) => setPassword(e.target.value)} placeholder="Mínimo 6 caracteres" disabled={loading} />
        </div>

        <button className={styles.submit} type="submit" disabled={loading || !name || !password}>
          {loading ? "Aguarde..." : mode === "login" ? "Entrar" : "Cadastrar"}
        </button>

        <button className={styles.toggle} type="button" onClick={toggleMode} disabled={loading}>
          {mode === "login" ? "Não tem conta? Cadastre-se" : "Já tem conta? Entre"}
        </button>
      </form>
    </div>
  )
}
