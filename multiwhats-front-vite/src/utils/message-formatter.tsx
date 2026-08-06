
import React from "react"

function parseInline(text: string): React.ReactNode[] {
  const regex =
    /(```[\s\S]*?```|`[^`]+`|\*[^*\n]+\*|_[^_\n]+_|~[^~\n]+~)/g

  const result: React.ReactNode[] = []
  let last = 0
  let match: RegExpExecArray | null

  while ((match = regex.exec(text))) {
    if (match.index > last) {
      result.push(text.slice(last, match.index))
    }

    const raw = match[0]

    if (raw.startsWith("```")) {
      result.push(
        <pre
          key={match.index + "```"}
          style={{
            margin: "6px 0",
            padding: "10px",
            borderRadius: 8,
            background: "rgba(0,0,0,.08)",
            overflowX: "auto",
            fontFamily: "monospace",
            fontSize: 13,
            whiteSpace: "pre-wrap",
          }}
        >
          <code>{raw.slice(3, -3)}</code>
        </pre>
      )
    } else if (raw.startsWith("`")) {
      result.push(
        <code
          key={match.index + "`"}
          style={{
            background: "rgba(0,0,0,.08)",
            padding: "2px 5px",
            borderRadius: 4,
            fontFamily: "monospace",
            fontSize: "0.9em",
          }}
        >
          {raw.slice(1, -1)}
        </code>
      )
    } else if (raw.startsWith("*")) {
      result.push(
        <strong key={match.index + "*"}>
          {parseInline(raw.slice(1, -1))}
        </strong>
      )
    } else if (raw.startsWith("_")) {
      result.push(
        <em key={match.index + "_"}>
          {parseInline(raw.slice(1, -1))}
        </em>
      )
    } else if (raw.startsWith("~")) {
      result.push(
        <s key={match.index + "~"}>
          {parseInline(raw.slice(1, -1))}
        </s>
      )
    }

    last = regex.lastIndex
  }

  if (last < text.length) {
    result.push(text.slice(last))
  }

  return result
}

export function formatMessageText(text: string) {
  return text.split("\n").flatMap((line, index, arr) => {
    if (line.startsWith("> ")) {
      return [
        <blockquote
          key={`quote-${index}`}
          style={{
            margin: "4px 0",
            paddingLeft: 10,
            borderLeft: "4px solid #25D366",
            color: "var(--text-muted)",
          }}
        >
          {parseInline(line.substring(2))}
        </blockquote>,
        ...(index < arr.length - 1 ? [<br key={`br-${index}`} />] : []),
      ]
    }

    return [
      ...parseInline(line),
      ...(index < arr.length - 1 ? [<br key={`br-${index}`} />] : []),
    ]
  })
}
