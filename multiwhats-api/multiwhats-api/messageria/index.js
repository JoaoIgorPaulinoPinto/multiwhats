import express from 'express';
import pkg from 'whatsapp-web.js';
import qrcode from 'qrcode-terminal';
import axios from 'axios';

const { Client, LocalAuth } = pkg;
const app = express();
app.use(express.json());

const PORT = 3000;
// URL da sua API ASP.NET que vai processar e salvar as mensagens recebidas
const ASPNET_WEBHOOK_URL = 'http://localhost:5000/api/webhook/whatsapp'; 

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

// ENDPOINT 1: Escuta as mensagens e envia para o ASP.NET
client.on('message', async (msg) => {
    console.log("RECEBIDO");
    // Ignora mensagens de grupos (opcional, remova se quiser escutar grupos também)
    if (msg.from.includes('@g.us') || msg.from.includes('@newsletter') || msg.from.includes('@lid')) {
        return;
    }
    // // Filtro para não duplicar o que você envia para terceiros
    if (msg.fromMe && msg.to !== msg.from) return;

    console.log(`Mensagem recebida de ${msg.from}: ${msg.body}`);

    try {
        // Envia o payload da mensagem diretamente para o seu ASP.NET
        await axios.post(ASPNET_WEBHOOK_URL, {
            from: msg.from,
            body: msg.body,
            timestamp: msg.timestamp,
            notifyName: msg._data?.notifyName // Nome do contato no WhatsApp
        });
    } catch (error) {
        console.error('Erro ao enviar mensagem para o ASP.NET:', error.message);
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