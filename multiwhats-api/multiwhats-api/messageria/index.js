import express from 'express';
import pkg from 'whatsapp-web.js';
import qrcode from 'qrcode-terminal';
import axios from 'axios';


const { Client, LocalAuth, MessageMedia } = pkg;
const app = express();
app.use(express.json({ limit: '100mb' }));

const PORT = process.env.PORT || 3333;
const ASPNET_WEBHOOK_URL = process.env.ASPNET_WEBHOOK_URL || "http://127.0.0.1:5261/api/webhook/whatsapp";
const ASPNET_DEVICE_URL = process.env.ASPNET_DEVICE_URL || "http://127.0.0.1:5261/api/device";
const CHROME_PATH = process.env.CHROME_PATH || "C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe";

// ==========================================
// 1. INICIALIZAÇÃO DO WHATSAPP-WEB.JS
// ==========================================
const client = new Client({
    authStrategy: new LocalAuth(),
    puppeteer: {
        executablePath: CHROME_PATH,
        args: [
            "--no-sandbox",
            "--disable-setuid-sandbox",
            "--disable-dev-shm-usage",
            "--disable-gpu"
        ]
    }
});

// Gera o QR Code no terminal para você escanear na primeira vez
client.on('qr', (qr) => {
    console.log('Escaneie o QR Code abaixo para conectar:');
    qrcode.generate(qr, { small: true });
});

client.on('ready', () => {
    console.log('Conexão com o WhatsApp estabelecida com sucesso!');
});

client.on("authenticated", () => {
    console.log("✅ Autenticado");
});

client.on("loading_screen", (percent, message) => {
    console.log(percent, message);
});

client.on("change_state", state => {
    console.log("Estado:", state);
});
client.on("message_create", async (msg) => {
    console.log("Mensagem!");
    console.log(msg.body);
});
client.on("ready", async () => {
    console.log("Conectado!");

    const info = client.info;
    console.log(info);

    try {
        await axios.post(ASPNET_DEVICE_URL, {
            jid: info.wid.user + "@c.us",
            phoneNumber: info.wid.user,
            pushName: info.pushname || info.me?.name || null,
            platform: info.platform || null
        });
        console.log("✅ Informações do dispositivo enviadas para o backend");
    } catch (error) {
        console.error("❌ Erro ao enviar informações do dispositivo:", error.message);
    }
});
// ENDPOINT 1: Escuta as mensagens e envia para o ASP.NET
client.on("message_create", async (msg) => {

    if (msg.from.includes("@newsletter")) return;
    if (msg.from.includes("@g.us")) return;
    if (msg.fromMe && msg.to !== msg.from) return;

    try {
        const contato = await msg.getContact();

        const rawNumber =
            contato.number ||                          // Tenta do perfil do contato
            contato.id?.user ||                        // Tenta do ID do contato
            msg.author?.split('@')[0] ||               // Se for grupo, pega quem enviou
            msg.from.split('@')[0];                    // Fallback para o JID da mensagem

        const numeroReal = rawNumber ? rawNumber.replace(/\D/g, '') : null;
        // console.log("==================================");
        // console.log("Nova mensagem recebida");
        // console.log("Nome (WhatsApp):", msg._data?.notifyName || contato.pushname);
        // console.log("Nome Salvo na Agenda:", contato.name || "Não salvo");
        // console.log("JID Original:", msg.from);
        // console.log("Número Real Extraído:", numeroReal);
        // console.log("Número Real Extraído 2 teste:", numeroReal);
        // console.log("==================================");

        // 3. Detecta tipo e mídia
        let messageType = "text";
        let hasMedia = false;
        let mediaUrl = null;
        let mediaMimeType = null;
        let mediaFilename = null;
        let mediaSize = null;

        if (msg.hasMedia) {
            hasMedia = true;
            const media = await msg.downloadMedia().catch(() => null);
            if (media) {
                if (media.mimetype === "image/webp") {
                    messageType = "sticker";
                } else if (media.mimetype?.startsWith("image")) {
                    messageType = "image";
                } else if (media.mimetype?.startsWith("audio")) {
                    messageType = "audio";
                } else if (media.mimetype?.startsWith("video")) {
                    messageType = "video";
                } else {
                    messageType = "document";
                }
                mediaUrl = media.data ? `data:${media.mimetype};base64,${media.data}` : null;
                mediaMimeType = media.mimetype;
                mediaFilename = media.filename || null;
                mediaSize = media.filesize ? parseInt(media.filesize) : null;
            }
        }

        // 4. Envia para a API ASP.NET
        const response = await axios.post(ASPNET_WEBHOOK_URL, {
            from: msg.from,
            phoneNumber: numeroReal,
            body: msg.body,
            timestamp: msg.timestamp,
            notifyName: msg._data?.notifyName || contato.pushname,
            messageType: messageType,
            hasMedia: hasMedia,
            mediaUrl: mediaUrl,
            mediaMimeType: mediaMimeType,
            mediaFilename: mediaFilename,
            mediaSize: mediaSize,
            messageId: msg.id?._serialized || null,
            isForwarded: msg.hasQuotedMsg || false,
            userId: 1
        });

        console.log("Webhook respondeu:", response.status);

    } catch (error) {
        console.error("Erro ao processar mensagem ou enviar webhook:", error);
    }
});
client.initialize();

// ==========================================
// 2. ENDPOINT PARA O ASP.NET ENVIAR MENSAGENS
// ==========================================
app.post('/api/enviar', async (req, res) => {
    const {
        jid,
        mensagem,
        type,
        mediaBase64,
        mediaMimeType,
        caption,
        filename
    } = req.body;

    if (!jid) {
        return res.status(400).json({
            error: "JID é obrigatório."
        });
    }

    if (type !== "text" && !mediaBase64) {
        return res.status(400).json({
            error: "mediaBase64 é obrigatório para mensagens de mídia."
        });
    }

    try {

        console.log(`[WhatsApp] Tentando enviar ${type || "text"} para: ${jid}`);

        const chat = await client.getChatById(jid).catch(() => null);

        let resposta;

        // ============================
        // ENVIO DE TEXTO
        // ============================
        if (!type || type === "text") {

            if (chat)
                resposta = await chat.sendMessage(mensagem);
            else
                resposta = await client.sendMessage(jid, mensagem);

        } else {

            // Remove prefixo data:image/png;base64,...
            const base64 = mediaBase64.replace(/^data:.*;base64,/, "");

            const media = new MessageMedia(
                mediaMimeType,
                base64,
                filename || "arquivo"
            );

            const sendOptions = {};

            if (caption)
                sendOptions.caption = caption;

            if (filename && type === "document")
                sendOptions.filename = filename;

            if (chat)
                resposta = await chat.sendMessage(media, sendOptions);
            else
                resposta = await client.sendMessage(jid, media, sendOptions);

        }

        const messageId =
            resposta?.id?._serialized ??
            `fallback-${Date.now()}`;

        console.log("✅ Mensagem enviada!", messageId);

        return res.status(200).json({
            sucesso: true,
            messageId
        });

    } catch (error) {

        console.error("❌ ERRO AO ENVIAR:", error);

        return res.status(500).json({
            error: error.message
        });

    }
});
app.listen(PORT, () => {
    console.log(`Servidor Node de comunicação WhatsApp rodando na porta ${PORT}`);
});