import axios from 'axios'
import express from 'express'
import fs from 'fs'
import path from 'path'
import qrcode from 'qrcode-terminal'
import { fileURLToPath } from 'url'
import pkg from 'whatsapp-web.js'
const { Client, LocalAuth, MessageMedia } = pkg

const __dirname = path.dirname(fileURLToPath(import.meta.url))
const LOG_DIR = path.join(__dirname, 'logs')
if (!fs.existsSync(LOG_DIR)) {
  fs.mkdirSync(LOG_DIR, { recursive: true })
}
const LOG_FILE = path.join(LOG_DIR, 'bridge.log')

function log(...args) {
  const line = `[${new Date().toISOString()}] ${args.map((a) => (typeof a === 'string' ? a : JSON.stringify(a))).join(' ')}`
  console.log(line)
  try {
    fs.appendFileSync(LOG_FILE, line + '\n')
  } catch (_) {}
}

// Resolve o ID serializado da mensagem de forma robusta. Versões recentes do
// WhatsApp Web expõem o campo minificado `$1` no lugar de `_serialized`.
function serializedId(msg) {
  const id = msg?.id || {}
  if (id._serialized) return id._serialized
  if (id.$1) return id.$1
  if (id.remote && id.id) {
    return `${id.fromMe ? 'true' : 'false'}_${id.remote}_${id.id}`
  }
  return null
}

// Log compacto dos campos relevantes de uma mensagem do whatsapp-web.js
function describeMsg(msg, extra = {}) {
  const id = msg.id || {}
  return {
    event: extra.event,
    id: id.id ?? null,
    serialized: serializedId(msg),
    idObj: JSON.stringify(id),
    fromMe: msg.fromMe,
    deviceType: msg.deviceType,
    type: msg.type,
    from: msg.from,
    to: msg.to,
    author: msg.author,
    hasMedia: msg.hasMedia,
    isNewMsg: msg._data?.isNewMsg ?? null,
    subtype: msg._data?.subtype ?? null,
    t: msg.timestamp,
    ...extra,
  }
}

const app = express()
app.use(express.json({ limit: '100mb' }))

// Configurações de Ambiente
const PORT = process.env.PORT || 3333
const ASPNET_WEBHOOK_URL =
  process.env.ASPNET_WEBHOOK_URL || 'http://localhost:5261/api/webhook/whatsapp'
const ASPNET_DEVICE_URL =
  process.env.ASPNET_DEVICE_URL || 'http://localhost:5261/api/device'

// Rastreia os IDs das mensagens enviadas pelo sistema via API. Serve para
// diferenciar, no evento message_create, se a mensagem veio do sistema
// (source="system") ou do celular (source="phone").
const apiSentMessageIds = new Set()

// Fila de envios aguardando o ID real da mensagem. O objeto retornado pelo
// sendMessage nem sempre expõe o id serializado (resposta.id sem _serialized /
// remote / id), o que fazia o /api/enviar devolver um "fallback-<ts>" e o
// webhook salvar a MESMA mensagem de novo com o id real (duplicação no banco).
// Aqui correlacionamos o envio com o evento message_create (que sempre traz o
// id real) por JID + janela de tempo.
const sendsAguardandoId = new Map()

// Evita que duas sincronizações iniciais rodem ao mesmo tempo.
let syncEmAndamento = false

// Inicialização do Cliente WhatsApp
const client = new Client({
  authStrategy: new LocalAuth(),
  puppeteer: {
    headless: true,
    executablePath:
      'C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe',
    args: [
      '--no-sandbox',
      '--disable-setuid-sandbox',
      '--disable-dev-shm-usage',
      '--disable-gpu',
    ],
  },
})

// ==========================================
// EVENTOS DO CLIENTE WHATSAPP
// ==========================================

client.on('qr', (qr) => {
  console.log('Escaneie o QRCode abaixo:')
  qrcode.generate(qr, { small: true })
})

client.on('authenticated', () => {
  console.log('✅ Autenticado com sucesso.')
})

client.on('loading_screen', (percent, message) => {
  console.log(`Carregando: ${percent}% - ${message}`)
})

client.on('change_state', (state) => {
  console.log('Estado da conexão:', state)
})

client.on('ready', async () => {
  console.log('✅ WhatsApp pronto e conectado.')
  log('EVENTO_READY')
  try {
    const version = await client.getWWebVersion()
    console.log(`Versão Web do WhatsApp: ${version}`)
    log('VERSION_OK', { version })

    const info = client.info
    log('CLIENT_INFO', { info: JSON.stringify(info) })
    if (!info || !info.wid) {
      log('DEVICE_ERRO', {
        error: 'client.info sem wid',
        info: JSON.stringify(info),
      })
      return
    }
    await axios.post(ASPNET_DEVICE_URL, {
      jid: `${info.wid.user}@c.us`,
      phoneNumber: info.wid.user,
      pushName: info.pushname || info.me?.name || null,
      platform: info.platform || null,
    })

    console.log('Informações do dispositivo enviadas ao ASP.NET')
    log('DEVICE_OK', { jid: `${info.wid.user}@c.us` })
  } catch (err) {
    console.error(
      'Erro no evento ready/envio de dispositivo:',
      err.message || err,
    )
    log('DEVICE_ERRO', {
      error: err.message || String(err),
      stack: err.stack?.split('\n').slice(0, 4).join(' | '),
    })
  }
})

// ==========================================
// FUNÇÃO REUTILIZÁVEL DE PROCESSAMENTO DE MENSAGENS
// ==========================================

// Envia o payload ao ASP.NET com retentativas. O retry é seguro porque o
// backend deduplica por messageId.
async function postWebhook(payload) {
  const MAX_TENTATIVAS = 3
  let lastError
  for (let tentativa = 1; tentativa <= MAX_TENTATIVAS; tentativa++) {
    try {
      return await axios.post(ASPNET_WEBHOOK_URL, payload, { timeout: 15000 })
    } catch (err) {
      lastError = err
      if (tentativa < MAX_TENTATIVAS) {
        log('WEBHOOK_RETRY', {
          messageId: payload.messageId,
          tentativa,
          error: err.message || String(err),
        })
        await new Promise((resolve) => setTimeout(resolve, tentativa * 1000))
      }
    }
  }
  throw lastError
}

// options: { source: "system" | "phone" | "contact", isSync: boolean }
async function processarEMandarParaAspNet(msg, enviadaPorMim, options = {}) {
  const { source, isSync = false } = options
  try {
    if (
      msg.from.includes('@newsletter') ||
      msg.from.includes('@g.us') ||
      msg.to?.includes('@g.us') ||
      msg.from.includes('@broadcast') ||
      msg.from.includes('status@broadcast')
    ) {
      log('FILTRO_IGNORADO', describeMsg(msg, { event: source || '?' }))
      return
    }

    log('PROCESSANDO', describeMsg(msg, { event: source || '?', isSync }))

    const targetJid = enviadaPorMim ? msg.to : msg.from

    // Obtém o contato do destinatário ou remetente real. Consulta não
    // essencial: se falhar (ex.: author LID não resolvível), seguimos sem
    // o pushname em vez de descartar a mensagem inteira.
    // IMPORTANTE: para mensagens enviadas (fromMe), msg.getContact() retorna o
    // contato do PRÓPRIO dono da conta (Message.getContact usa author||from e,
    // em 1:1, from é o JID de quem envia). Por isso, quando a mensagem foi
    // enviada por nós, buscamos o contato do destinatário real (msg.to).
    let contatoPushname = null
    try {
      const contato = enviadaPorMim
        ? await client.getContactById(msg.to)
        : await msg.getContact()
      contatoPushname = contato?.pushname ?? null
    } catch (err) {
      log('GETCONTACT_ERRO', {
        messageId: serializedId(msg),
        source: source || '?',
        error: err.message || String(err),
      })
    }

    const rawNumber = targetJid.split('@')[0]
    const numeroReal = rawNumber ? rawNumber.replace(/\D/g, '') : null

    let messageType = msg.type
    let hasMedia = false
    let mediaUrl = null
    let mediaMimeType = null
    let mediaFilename = null
    let mediaSize = null
    let mediaCaption = null

    if (msg.hasMedia) {
      try {
        const midia = await msg.downloadMedia()
        if (midia) {
          hasMedia = true
          mediaMimeType = midia.mimetype || 'image/jpeg'
          mediaFilename = midia.filename || 'arquivo'
          mediaUrl = midia.data
          mediaCaption = msg.caption || msg.body || null
          mediaSize = midia.filesize
            ? Number(midia.filesize)
            : midia.data.length
        }
      } catch (err) {
        hasMedia = false
      }
    }

    const payload = {
      from: targetJid, // ✅ Agora sempre conterá o JID do Cliente (ex: 5511999999999@c.us)
      phoneNumber: numeroReal,
      body: msg.body,
      timestamp: msg.timestamp,
      notifyName: (enviadaPorMim ? null : msg._data?.notifyName) || contatoPushname || null,
      messageType,
      hasMedia,
      mediaUrl,
      mediaMimeType,
      mediaFilename,
      mediaSize,
      mediaCaption,
      messageId: serializedId(msg),
      isForwarded: msg.isForwarded || false,
      fromMe: enviadaPorMim,
      source: source || (enviadaPorMim ? 'phone' : 'contact'),
      isSync,
      userId: 1,
    }

    await postWebhook(payload)
  } catch (err) {
    log('WEBHOOK_ERRO', {
      messageId: serializedId(msg) ?? msg.id?.id ?? null,
      source: source || '?',
      isSync,
      error: err.message || String(err),
      stack: err.stack?.split('\n').slice(0, 4).join(' | '),
      responseStatus: err.response?.status ?? null,
      responseData: err.response?.data ?? null,
    })
  }
}

client.on('message', async (msg) => {
  log('EVENTO_MESSAGE', describeMsg(msg, { event: 'message' }))
  if (msg.fromMe) return
  await processarEMandarParaAspNet(msg, false, { source: 'contact' })
})

client.on('message_create', async (msg) => {
  log('EVENTO_MESSAGE_CREATE', describeMsg(msg, { event: 'message_create' }))
  if (msg.fromMe) {
    const msgId = serializedId(msg)
    // Resolve o primeiro envio pendente para este destino com o ID real.
    // O guard de tempo evita consumir um pendente com um evento que não
    // corresponde a esse envio (ex.: mensagem mandada pelo próprio celular).
    // O evento dispara durante o sendMessage, então isso também identifica a
    // mensagem como enviada pelo sistema (source="system").
    let viaApi = false
    if (msgId) {
      const fila = sendsAguardandoId.get(msg.to || msg.from)
      const pendente = fila?.[0]
      const tsMsg = (msg.timestamp || 0) * 1000
      if (pendente && Math.abs(tsMsg - pendente.criadoEm) < 15000) {
        fila.shift()
        if (fila.length === 0) sendsAguardandoId.delete(msg.to || msg.from)
        viaApi = true
        pendente.resolve(msgId)
      }
    }
    const isApiSent = viaApi || (!!msgId && apiSentMessageIds.has(msgId))
    log(
      'MESSAGE_CREATE_FROMME',
      describeMsg(msg, {
        event: 'message_create',
        isApiSent,
        inApiSentSet: !!msgId && apiSentMessageIds.has(msgId),
      }),
    )
    if (isApiSent) {
      apiSentMessageIds.delete(msgId)
    }
    await processarEMandarParaAspNet(msg, true, {
      source: isApiSent ? 'system' : 'phone',
    })
  }
})

process.on('unhandledRejection', (reason) => {
  console.error('Unhandled Rejection:', reason)
  log('UNHANDLED_REJECTION', { reason: String(reason), stack: reason?.stack })
})

process.on('uncaughtException', (err) => {
  console.error('Uncaught Exception:', err.message || err)
  log('UNCAUGHT_EXCEPTION', {
    error: err.message || String(err),
    stack: err.stack,
  })
})

// ==========================================
// ENDPOINTS HTTP (API)
// ==========================================

app.post('/api/enviar', async (req, res) => {
  const { jid, mensagem, type, mediaBase64, mediaMimeType, caption, filename } =
    req.body

  if (!jid) {
    return res.status(400).json({ error: 'JID é obrigatório.' })
  }

  if (type && type !== 'text' && !mediaBase64) {
    return res
      .status(400)
      .json({ error: 'mediaBase64 é obrigatório para envio de mídias.' })
  }

  try {
    console.log(
      `\n📤 Disparando envio via API para: ${jid} (Tipo: ${type || 'text'})`,
    )

    // Pré-registra a espera do ID real ANTES do envio. O evento message_create
    // dispara quando a mensagem é criada no web (dentro do sendMessage), então
    // a promise já estará resolvida quando o sendMessage retornar. Isso evita
    // o fallback-<ts> e a consequente duplicação no backend (que deduplica por
    // messageId).
    const fila = sendsAguardandoId.get(jid) || []
    let resolveId = null
    const idRealPromise = new Promise((resolve) => {
      resolveId = resolve
    })
    const pendente = { criadoEm: Date.now() }
    pendente.resolve = (id) => {
      clearTimeout(pendente._timeout)
      resolveId(id)
    }
    pendente._timeout = setTimeout(() => {
      const idx = fila.indexOf(pendente)
      if (idx >= 0) fila.splice(idx, 1)
      if (fila.length === 0) sendsAguardandoId.delete(jid)
      pendente.resolve(null)
    }, 8000)
    fila.push(pendente)
    sendsAguardandoId.set(jid, fila)

    let resposta
    const chat = await client.getChatById(jid).catch(() => null)

    // Envio de Mensagem de Texto
    if (!type || type === 'text') {
      resposta = chat
        ? await chat.sendMessage(mensagem)
        : await client.sendMessage(jid, mensagem)
    }
    // Envio de Mídia
    else {
      const cleanBase64 = mediaBase64.replace(/^data:.*;base64,/, '')
      const media = new MessageMedia(
        mediaMimeType,
        cleanBase64,
        filename || 'arquivo',
      )
      const options = {}

      if (['image', 'video', 'document'].includes(type)) {
        options.caption = caption || ''
      }

      if (type === 'document') {
        options.filename = filename || 'arquivo'
      }

      if (type === 'sticker') {
        options.sendMediaAsSticker = true
      }

      if (type === 'ptt' || type === 'audio') {
        options.sendAudioAsVoice = true
      }

      resposta = chat
        ? await chat.sendMessage(media, options)
        : await client.sendMessage(jid, media, options)
    }

    const realId = serializedId(resposta) || (await idRealPromise) || null

    // Nota: não adicionamos aqui a apiSentMessageIds. A classificação
    // source="system" acontece no handler do message_create quando ele consome
    // a pendência registrada acima (o evento dispara durante o sendMessage).
    const messageId = realId ?? `fallback-${Date.now()}`
    log('ENVIAR_API_OK', {
      jid,
      type: type || 'text',
      messageId,
      respostaId: resposta?.id?.id ?? null,
    })
    console.log('✅ Mensagem enviada com sucesso.')

    return res.status(200).json({
      sucesso: true,
      messageId,
    })
  } catch (err) {
    log('ENVIAR_API_ERRO', {
      jid,
      error: err.message || String(err),
      stack: err.stack?.split('\n').slice(0, 4).join(' | '),
    })
    console.error('Erro ao enviar mensagem:', err.message)
    return res.status(500).json({
      sucesso: false,
      error: err.message,
    })
  }
})

app.get('/', (_, res) => {
  res.send('WhatsApp Bridge Online')
})

// ==========================================
// SINCRONIZAÇÃO INICIAL DE MENSAGENS
// ==========================================

// POST /api/sync - Busca mensagens recentes de conversas privadas que já
// existiam antes da conexão e envia ao backend (dedup por messageId no backend).
app.post('/api/sync', async (req, res) => {
  if (syncEmAndamento) {
    return res
      .status(409)
      .json({ sucesso: false, error: 'Sincronização já em andamento.' })
  }

  const { limit = 50, chatsLimit = 100 } = req.body || {}

  syncEmAndamento = true
  let total = 0
  let chatsProcessados = 0

  try {
    const chats = await client.getChats()

    const chatsPrivados = chats.filter((c) => {
      const jid = c.id?._serialized || ''
      return (
        !jid.includes('@g.us') &&
        !jid.includes('@newsletter') &&
        !jid.includes('@broadcast') &&
        !jid.includes('status@broadcast')
      )
    })

    const alvo = chatsPrivados.slice(0, chatsLimit)

    for (const chat of alvo) {
      let mensagens = []
      try {
        mensagens = await chat.fetchMessages({ limit })
      } catch (err) {
        console.error(
          `Erro ao buscar mensagens do chat ${chat.id?._serialized}:`,
          err.message,
        )
        continue
      }

      for (const msg of mensagens) {
        const enviadaPorMim = msg.fromMe === true
        await processarEMandarParaAspNet(msg, enviadaPorMim, {
          source: enviadaPorMim ? 'phone' : 'contact',
          isSync: true,
        })
        total += 1
      }

      chatsProcessados += 1
    }

    console.log(
      `📦 Sincronização concluída: ${chatsProcessados} chats, ${total} mensagens.`,
    )
    return res.status(200).json({
      sucesso: true,
      chatsProcessados,
      total,
    })
  } catch (err) {
    console.error('Erro na sincronização:', err.message || err)
    return res
      .status(500)
      .json({ sucesso: false, error: err.message || String(err) })
  } finally {
    syncEmAndamento = false
  }
})

// ==========================================
// INICIALIZAÇÃO DO SERVIDOR
// ==========================================

// O servidor HTTP sobe primeiro. Se a porta já estiver em uso (outra instância
// rodando), o processo encerra antes de inicializar o WhatsApp, evitando a
// disputa pela mesma sessão do navegador (.wwebjs_auth).
const server = app.listen(PORT, () => {
  console.log('--------------------------------')
  console.log(`Servidor Gateway rodando na porta: ${PORT}`)
  console.log('--------------------------------')
  client.initialize()
})

server.on('error', (err) => {
  console.error(
    `Erro ao iniciar o servidor na porta ${PORT}:`,
    err.message || err,
  )
  log('SERVER_ERRO', { error: err.message || String(err) })
  process.exit(1)
})
