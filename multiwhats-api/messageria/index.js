import express from "express";
import pkg from "whatsapp-web.js";
import qrcode from "qrcode-terminal";
import axios from "axios";
const { Client, LocalAuth, MessageMedia } = pkg;

const app = express();
app.use(express.json({ limit: "100mb" }));

// Configurações de Ambiente
const PORT = process.env.PORT || 3333;
const ASPNET_WEBHOOK_URL = process.env.ASPNET_WEBHOOK_URL || "http://localhost:5261/api/webhook/whatsapp";
const ASPNET_DEVICE_URL = process.env.ASPNET_DEVICE_URL || "http://localhost:5261/api/device";

// Inicialização do Cliente WhatsApp
const client = new Client({
    authStrategy: new LocalAuth(),
    puppeteer: {
        headless: true,
        args: [
            "--no-sandbox",
            "--disable-setuid-sandbox",
            "--disable-dev-shm-usage",
            "--disable-gpu"
        ]
    }
});

// ==========================================
// EVENTOS DO CLIENTE WHATSAPP
// ==========================================

client.on("qr", qr => {
    console.log("Escaneie o QRCode abaixo:");
    qrcode.generate(qr, { small: true });
});

client.on("authenticated", () => {
    console.log("✅ Autenticado com sucesso.");
});

client.on("loading_screen", (percent, message) => {
    console.log(`Carregando: ${percent}% - ${message}`);
});

client.on("change_state", state => {
    console.log("Estado da conexão:", state);
});

client.on("ready", async () => {
    console.log("✅ WhatsApp pronto e conectado.");

    try {
        const version = await client.getWWebVersion();
        console.log(`Versão Web do WhatsApp: ${version}`);

        const info = client.info;
        await axios.post(ASPNET_DEVICE_URL, {
            jid: `${info.wid.user}@c.us`,
            phoneNumber: info.wid.user,
            pushName: info.pushname || info.me?.name || null,
            platform: info.platform || null
        });

        console.log("Informações do dispositivo enviadas ao ASP.NET");
    } catch (err) {
        console.error("Erro no evento ready/envio de dispositivo:", err.message || err);
    }
});

// ==========================================
// FUNÇÃO REUTILIZÁVEL DE PROCESSAMENTO DE MENSAGENS
// ==========================================

async function processarEMandarParaAspNet(msg, enviadaPorMim = false) {
    
    try {
        if (msg.from.includes("@newsletter") || msg.from.includes("@g.us") || msg.to?.includes("@g.us")) {
            return;
        }

        const contato = await msg.getContact();
        const targetJid = enviadaPorMim ? msg.to : msg.from;
        const rawNumber = contato.number || contato.id?.user || targetJid.split("@")[0];
        const numeroReal = rawNumber ? rawNumber.replace(/\D/g, "") : null;
        
        let messageType = "text";
        let hasMedia = false;
        let mediaUrl = null;
        let mediaMimeType = null;
        let mediaFilename = null;
        let mediaSize = null;
        let mediaCaption = null;
        
        if (msg.hasMedia  && msg.type != "ptt" && msg.type != "audio") {
            const typesMap = {
                image: "image",
                video: "video",
                audio: "audio",
                ptt: "audio",
                document: "document",
                sticker: "sticker"
            };
            
            messageType = msg.type
            
            try {
                const midia = await msg.downloadMedia();
                if (midia) {
                    hasMedia = true;
                    mediaMimeType = midia.mimetype || "image/jpeg";
                    mediaFilename = midia.filename || "arquivo";

                    mediaUrl = midia.data;
                    mediaCaption = msg.caption || msg.body || null;
                    mediaSize = midia.filesize ? Number(midia.filesize) : midia.data.length;
                    
                } else {
                    hasMedia = false;
                    messageType = "text";
                }
            } catch (err) {
                hasMedia = false;
                messageType = "text";
            }
        }

        const payload = {
            from: targetJid,
            phoneNumber: numeroReal,
            body: msg.body,
            timestamp: msg.timestamp,
            notifyName: msg._data?.notifyName || contato.pushname || null,
            messageType,
            hasMedia,
            mediaUrl,
            mediaMimeType,
            mediaFilename,
            mediaSize,
            mediaCaption,
            messageId: msg.id?._serialized || null,
            isForwarded: msg.hasQuotedMsg || false,
            fromMe: enviadaPorMim, 
            userId: 1
        };

        const response = await axios.post(ASPNET_WEBHOOK_URL, payload);
        console.log(`🚀 Webhook ASP.NET respondido com status: ${response.status}`);

    } catch (err) {
        console.error("Erro no processamento do webhook:", err.message || err);
    }
}

client.on("message", async (msg) => {
    await processarEMandarParaAspNet(msg, false);
});

client.on("message_create", async (msg) => {
    if (msg.fromMe) {
        await processarEMandarParaAspNet(msg, true);
    }
});

process.on("unhandledRejection", (reason) => {
    console.error("Unhandled Rejection:", reason);
});

process.on("uncaughtException", (err) => {
    console.error("Uncaught Exception:", err.message || err);
});

client.initialize();

// ==========================================
// ENDPOINTS HTTP (API)
// ==========================================

app.post("/api/enviar", async (req, res) => {
    const { jid, mensagem, type, mediaBase64, mediaMimeType, caption, filename } = req.body;

    if (!jid) {
        return res.status(400).json({ error: "JID é obrigatório." });
    }

    if (type && type !== "text" && !mediaBase64) {
        return res.status(400).json({ error: "mediaBase64 é obrigatório para envio de mídias." });
    }

    try {
        console.log(`\n📤 Disparando envio via API para: ${jid} (Tipo: ${type || 'text'})`);

        let resposta;
        const chat = await client.getChatById(jid).catch(() => null);

        // Envio de Mensagem de Texto
        if (!type || type === "text") {
            resposta = chat
                ? await chat.sendMessage(mensagem)
                : await client.sendMessage(jid, mensagem);
        }
        // Envio de Mídia
        else {
            const cleanBase64 = mediaBase64.replace(/^data:.*;base64,/, "");
            const media = new MessageMedia(mediaMimeType, cleanBase64, filename || "arquivo");
            const options = {};

            if (["image", "video", "document"].includes(type)) {
                options.caption = caption || "";
            }

            if (type === "document") {
                options.filename = filename || "arquivo";
            }

            if (type === "sticker") {
                options.sendMediaAsSticker = true;
            }

            if (type === "ptt" || type === "audio") {
                options.sendAudioAsVoice = true;
            }

            resposta = chat
                ? await chat.sendMessage(media, options)
                : await client.sendMessage(jid, media, options);
        }

        const messageId = resposta?.id?._serialized ?? `fallback-${Date.now()}`;
        console.log("✅ Mensagem enviada com sucesso.");

        return res.status(200).json({
            sucesso: true,
            messageId
        });

    } catch (err) {
        console.error("Erro ao enviar mensagem:", err.message);
        return res.status(500).json({
            sucesso: false,
            error: err.message
        });
    }
});

app.get("/", (_, res) => {
    res.send("WhatsApp Bridge Online");
});

// ==========================================
// INICIALIZAÇÃO DO SERVIDOR
// ==========================================

app.listen(PORT, () => {
    console.log("--------------------------------");
    console.log(`Servidor Gateway rodando na porta: ${PORT}`);
    console.log("--------------------------------");
});