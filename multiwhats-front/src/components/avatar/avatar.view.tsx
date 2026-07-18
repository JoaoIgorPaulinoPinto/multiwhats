import styles from "./avatar.module.css"

const COLORS = [
  "#25d366", "#00a884", "#128c7e", "#075e54",
  "#e91e63", "#9c27b0", "#673ab7", "#3f51b5",
  "#2196f3", "#03a9f4", "#009688", "#4caf50",
  "#ff9800", "#ff5722", "#795548", "#607d8b",
]

function hashColor(name: string): string {
  let hash = 0
  for (let i = 0; i < name.length; i++) {
    hash = name.charCodeAt(i) + ((hash << 5) - hash)
  }
  return COLORS[Math.abs(hash) % COLORS.length]
}

function getInitial(name: string): string {
  return name?.trim()?.charAt(0)?.toUpperCase() ?? "?"
}

interface Props {
  name: string
  size?: number
  fontSize?: number
  square?: boolean
}

export function AvatarView({ name, size = 44, fontSize, square }: Props) {
  return (
    <div
      className={`${styles.avatar} ${square ? styles.square : styles.circle}`}
      style={{
        width: size,
        height: size,
        background: hashColor(name),
        fontSize: fontSize ?? size * 0.42,
      }}
    >
      {getInitial(name)}
    </div>
  )
}
