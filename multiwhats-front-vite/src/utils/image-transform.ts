const MAX_DIMENSION = 800
const JPEG_QUALITY = 0.7
const MAX_CONCURRENT_TRANSFORMS = 3

let activeTransforms = 0
const transformQueue: (() => void)[] = []

function acquireTransformSlot(): Promise<void> {
  if (activeTransforms < MAX_CONCURRENT_TRANSFORMS) {
    activeTransforms++
    return Promise.resolve()
  }
  return new Promise<void>((resolve) => {
    transformQueue.push(() => {
      activeTransforms++
      resolve()
    })
  })
}

function releaseTransformSlot() {
  activeTransforms--
  transformQueue.shift()?.()
}

function loadImage(src: string): Promise<HTMLImageElement> {
  return new Promise((resolve, reject) => {
    const img = new Image()
    img.crossOrigin = "anonymous"
    img.onload = () => resolve(img)
    img.onerror = reject
    img.src = src
  })
}

export async function transformToJpeg(rawBase64: string, mime: string | null): Promise<string> {
  const dataUrl = rawBase64.startsWith("data:") ? rawBase64 : `data:${mime || "image/jpeg"};base64,${rawBase64}`

  const img = await loadImage(dataUrl)

  let { width, height } = img
  if (width > MAX_DIMENSION || height > MAX_DIMENSION) {
    const scale = MAX_DIMENSION / Math.max(width, height)
    width = Math.round(width * scale)
    height = Math.round(height * scale)
  }

  const canvas = document.createElement("canvas")
  canvas.width = width
  canvas.height = height
  const ctx = canvas.getContext("2d")!
  ctx.drawImage(img, 0, 0, width, height)

  return canvas.toDataURL("image/jpeg", JPEG_QUALITY)
}

export async function transformToJpegQueued(rawBase64: string, mime: string | null): Promise<string> {
  await acquireTransformSlot()
  try {
    return await transformToJpeg(rawBase64, mime)
  } finally {
    releaseTransformSlot()
  }
}
