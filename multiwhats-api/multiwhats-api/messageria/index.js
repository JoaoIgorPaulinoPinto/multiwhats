import express from 'express';
import pkg from 'whatsapp-web.js';
import qrcode from 'qrcode-terminal';
import axios from 'axios';
import https from 'https';
const { Client, LocalAuth } = pkg;
const app = express();
app.use(express.json());

const PORT = 3000;
// URL da sua API ASP.NET que vai processar e salvar as mensagens recebidas
const ASPNET_WEBHOOK_URL = "http://localhost:51563/api/webhook/whatsapp";

// ==========================================
// 1. INICIALIZAÇÃO DO WHATSAPP-WEB.JS
// ==========================================
const client = new Client({
    authStrategy: new LocalAuth(),
    puppeteer: {
        executablePath: "C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe",
        args: [
            "--no-sandbox",
            "--disable-setuid-sandbox"
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
});
// ENDPOINT 1: Escuta as mensagens e envia para o ASP.NET
client.on("message_create", async (msg) => {

    if (msg.from.includes("@newsletter")) return;
    if (msg.fromMe && msg.to !== msg.from) return;

    try {
        // 1. Busca o contato de forma assíncrona (resolve tanto @c.us quanto @lid)
        const contato = await msg.getContact();

        // 2. Aqui você pega o número de telefone real e limpo (ex: 5511999999999)
        const numeroReal = contato.number;

        console.log("==================================");
        console.log("Nova mensagem recebida");
        console.log("Nome (WhatsApp):", msg._data?.notifyName || contato.pushname);
        console.log("Nome Salvo na Agenda:", contato.name || "Não salvo");
        console.log("JID Original:", msg.from);
        console.log("Número Real Extraído:", numeroReal);
        console.log("==================================");

        // 3. Envia o número correto e tratado para a sua API ASP.NET
        const response = await axios.post(ASPNET_WEBHOOK_URL, {
            from: msg.from, // Mantém o JID se precisar responder depois
            phoneNumber: numeroReal, // <--- O número limpo para salvar no banco
            body: msg.body,
            timestamp: msg.timestamp,
            notifyName: msg._data?.notifyName || contato.pushname,
            usuarioId: 1 // (Seu ID de controle que configuramos antes)
        }, { httpsAgent });

        console.log("Webhook respondeu:", response.status);

    } catch (error) {
        console.error("Erro ao processar mensagem ou enviar webhook:", error.message);
    }
});
client.initialize();

// ==========================================
// 2. ENDPOINT PARA O ASP.NET ENVIAR MENSAGENS
// ==========================================
app.post('/api/enviar', async (req, res) => {
    const { numero, mensagem } = req.body;

    if (!numero || !mensagem) {
        return res.status(400).json({ error: 'Número e mensagem são obrigatórios.' });
    }

    try {
        let numeroLimpo = numero.replace(/\D/g, '');
        const numeroFormatado = `${numeroLimpo}@c.us`;

        console.log(`[WhatsApp] Tentando enviar mensagem para: ${numeroFormatado}`);

        const chat = await client.getChatById(numeroFormatado).catch(() => null);

        let resposta;
        if (chat) {
            resposta = await chat.sendMessage(mensagem);
        } else {
            resposta = await client.sendMessage(numeroFormatado, mensagem);
        }

        const messageId = (resposta && resposta.id && resposta.id._serialized)
            ? resposta.id._serialized
            : `fallback-id-${Date.now()}`;

        console.log("Mensagem enviada ao wppw-js!", messageId)
        return res.status(200).json({
            sucesso: true,
            messageId: messageId
        });

    } catch (error) {
        console.error('❌ ERRO REAL AO ENVIAR NO WHATSAPP-WEB.JS:', error);
        return res.status(500).json({ error: `Falha ao enviar a mensagem: ${error.message}` });
    }
});
app.listen(PORT, () => {
    console.log(`Servidor Node de comunicação WhatsApp rodando na porta ${PORT}`);
});