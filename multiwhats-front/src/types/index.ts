export type MessageDirection = 0 | 1

export type MessageType = "Text" | "Image" | "Audio" | "Video" | "Document" | "Sticker" | "Vcard" | "Location" | "Unknown" | 0 | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8

export const MESSAGE_TYPE_MAP: Record<number, string> = {
  0: "Text",
  1: "Image",
  2: "Audio",
  3: "Video",
  4: "Document",
  5: "Location",
  6: "Contact",
  7: "Sticker",
  8: "Unknown", 
}

export const MESSAGE_TYPE_REVERSE: Record<string, number> = Object.fromEntries(
  Object.entries(MESSAGE_TYPE_MAP).map(([k, v]) => [v, Number(k)])
)

export function toNumericType(type: MessageType): number {
  if (typeof type === "number") return type
  return MESSAGE_TYPE_REVERSE[type] ?? 0
}

export function isContactType(type: MessageType): boolean {
  return type === "Vcard" || type === 6
}

export type DeliveryStatus = "Pending" | "Sent" | "Delivered" | "Read" | "Failed" | 0 | 1 | 2 | 3

export type OccurrenceStatus = "Open" | "InProgress" | "Resolved" | "Closed"

export type OccurrenceStatusNumeric = 0 | 1 | 2 | 3

export type Priority = 0 | 1 | 2 | 3

export type ClientTaskStatus = "Open" | "InProgress" | "Completed" | "Cancelled"
