export function toDataUrl(raw: string, mime: string | null): string {
  if (raw.startsWith("data:")) return raw
  const m = mime || guessMime(raw)
  return `data:${m};base64,${raw}`
}

export function guessMime(raw: string): string {
  if (raw.startsWith("/9j/")) return "image/jpeg"
  if (raw.startsWith("iVBOR")) return "image/png"
  if (raw.startsWith("UklGR")) return "image/webp"
  if (raw.startsWith("R0lGO")) return "image/gif"
  if (raw.startsWith("JVBER")) return "application/pdf"
  if (raw.startsWith("UEsD")) return "application/zip"
  return "application/octet-stream"
}

export function detectMediaType(file: File): "Image" | "Video" | "Audio" | "Sticker" | "Document" {
  if (file.type === "image/webp") return "Sticker"
  if (file.type.startsWith("image/")) return "Image"
  if (file.type.startsWith("video/")) return "Video"
  if (file.type.startsWith("audio/")) return "Audio"
  return "Document"
}

export function fileToBase64(file: File): Promise<string> {
  return new Promise((resolve, reject) => {
    const reader = new FileReader()
    reader.onload = () => resolve(reader.result as string)
    reader.onerror = reject
    reader.readAsDataURL(file)
  })
}
