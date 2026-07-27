import express from "express";
import db from "./database/connection.js";

const app = express();

app.use(express.json());

app.get("/api/test", async (req, res) => {
   return res.status(201).json({
            message: "Muito bem!"
        });  
})
app.post("/api/messages", async (req, res) => {
    try {
        const {
            from_jid,
            to_jid,
            phone_number,
            body,
            direction,
            type,
            timestamp,
            chat_id,
            user_id,
            occurrence_id,
            message_id,
            notify_name,
            has_media,
            media_url,
            media_mime_type,
            media_filename,
            media_size,
            media_caption,
            is_forwarded,
            reply_to_id
        } = req.body;

        if (!from_jid || !chat_id) {
            return res.status(400).json({ error: "from_jid e chat_id são obrigatórios." });
        }

        const sent_at = new Date(timestamp * 1000);
        const delivery_status = direction === "Outgoing" ? "Pending" : "Delivered";

        const sql = `
            INSERT INTO Messages (
                message_id, from_jid, to_jid, phone_number, body,
                direction, type, timestamp, sent_at, notify_name,
                has_media, media_url, media_mime_type, media_filename,
                media_size, media_caption, delivery_status, is_forwarded,
                chat_id, user_id, occurrence_id, reply_to_id,
                IsDeleted, CreatedAt, LastUpdate
            ) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, 0, NOW(), NOW())
        `;

        const [result] = await db.execute(sql, [
            message_id || null,
            from_jid,
            to_jid || null,
            phone_number || null,
            body || null,
            direction || "Incoming",
            type || "Text",
            timestamp || Math.floor(Date.now() / 1000),
            sent_at,
            notify_name || null,
            has_media ? 1 : 0,
            media_url || null,
            media_mime_type || null,
            media_filename || null,
            media_size || null,
            media_caption || null,
            delivery_status,
            is_forwarded ? 1 : 0,
            chat_id,
            user_id || null,
            occurrence_id || null,
            reply_to_id || null
        ]);

        return res.status(201).json({
            success: true,
            id: result.insertId
        });
    } catch (err) {
        console.error("Erro ao inserir mensagem:", err.message);
        return res.status(500).json({ error: err.message });
    }
});

export default app;