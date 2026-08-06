export function getJwtExp(token: string): number | null {
  try {
    const part = token.split(".")[1]
    if (!part) return null
    const json = atob(part.replace(/-/g, "+").replace(/_/g, "/"))
    const payload = JSON.parse(json)
    return typeof payload.exp === "number" ? payload.exp : null
  } catch {
    return null
  }
}

export function isJwtExpired(token: string, skewSeconds = 30): boolean {
  const exp = getJwtExp(token)
  if (exp === null) return false
  return exp * 1000 - skewSeconds * 1000 <= Date.now()
}
