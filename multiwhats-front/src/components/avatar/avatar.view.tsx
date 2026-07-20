import styles from "./avatar.module.css"

const GRADIENTS = [
  ["#25d366", "#128c7e"],
  ["#00a884", "#075e54"],
  ["#e91e63", "#9c27b0"],
  ["#673ab7", "#3f51b5"],
  ["#2196f3", "#03a9f4"],
  ["#009688", "#4caf50"],
  ["#ff9800", "#ff5722"],
  ["#795548", "#607d8b"],
  ["#e91e63", "#f44336"],
  ["#9c27b0", "#673ab7"],
  ["#3f51b5", "#2196f3"],
  ["#00bcd4", "#009688"],
  ["#8bc34a", "#4caf50"],
  ["#ffc107", "#ff9800"],
  ["#ff5722", "#e64a19"],
  ["#607d8b", "#455a64"],
]

function hashGradient(name: string): string {
  let hash = 0
  for (let i = 0; i < name.length; i++) {
    hash = name.charCodeAt(i) + ((hash << 5) - hash)
  }
  const [a, b] = GRADIENTS[Math.abs(hash) % GRADIENTS.length]
  return `linear-gradient(135deg, ${a}, ${b})`
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
        background: hashGradient(name),
        fontSize: fontSize ?? size * 0.42,
      }}
    >
      {getInitial(name)}
    </div>
  )
}
