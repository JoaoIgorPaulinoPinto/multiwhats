export interface ParsedVCard {
  name: string
  phone: string
  firstName: string
}

export function parseVCard(raw: string): ParsedVCard | null {
  if (!raw || !raw.includes("BEGIN:VCARD")) return null

  const lines = raw.split(/\r?\n/)
  let name = ""
  let phone = ""

  for (const line of lines) {
    if (line.startsWith("FN:")) {
      name = line.slice(3).trim()
    } else if (line.startsWith("N:")) {
      const parts = line.slice(2).split(";")
      const lastName = (parts[0] || "").trim()
      const firstName = (parts[1] || "").trim()
      if (!name && (firstName || lastName)) {
        name = firstName ? `${firstName} ${lastName}`.trim() : lastName
      }
    } else if (line.includes("TEL") && line.includes(":")) {
      const phoneMatch = line.match(/:(\+?[\d\s\-()]+)/)
      if (phoneMatch) {
        phone = phoneMatch[1].replace(/[\s\-()]/g, "")
      }
    }
  }

  if (!name && !phone) return null

  const firstName = name.split(" ")[0] || name || "Contato"

  return { name: name || "Contato", phone, firstName }
}

export function phoneToJid(phone: string): string {
  const digits = phone.replace(/\D/g, "")
  return `${digits}@s.whatsapp.net`
}
