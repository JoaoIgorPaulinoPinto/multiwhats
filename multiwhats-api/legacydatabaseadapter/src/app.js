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

app.post("/api/contacts", async (req, res) => {
    try {
        const { jid, phone_number, name, push_name, is_blocked, is_group } = req.body;
        if (!jid) {
            return res.status(400).json({ error: "jid é obrigatório." });
        }
        const sql = `
            INSERT INTO Contacts (jid, phone_number, name, push_name, is_blocked, is_group, IsDeleted, CreatedAt, LastUpdate)
            VALUES (?, ?, ?, ?, ?, ?, 0, NOW(), NOW())
        `;
        const [result] = await db.execute(sql, [
            jid,
            phone_number || null,
            name || null,
            push_name || null,
            is_blocked ? 1 : 0,
            is_group ? 1 : 0
        ]);
        return res.status(201).json({ success: true, id: result.insertId });
    } catch (err) {
        console.error("Erro ao inserir contato:", err.message);
        return res.status(500).json({ error: err.message });
    }
});

app.post("/api/clients", async (req, res) => {
    try {
        const { name, main_phone_number, status } = req.body;
        if (!name) {
            return res.status(400).json({ error: "name é obrigatório." });
        }
        const sql = `
            INSERT INTO Clients (name, main_phone_number, status, IsDeleted, CreatedAt, LastUpdate)
            VALUES (?, ?, ?, 0, NOW(), NOW())
        `;
        const [result] = await db.execute(sql, [
            name,
            main_phone_number || null,
            status || "Active"
        ]);
        return res.status(201).json({ success: true, id: result.insertId });
    } catch (err) {
        console.error("Erro ao inserir cliente:", err.message);
        return res.status(500).json({ error: err.message });
    }
});

app.post("/api/occurrences", async (req, res) => {
    try {
        const { title, description, status, priority, chat_id } = req.body;
        if (!title || !chat_id) {
            return res.status(400).json({ error: "title e chat_id são obrigatórios." });
        }
        const sql = `
            INSERT INTO Occurrences (title, description, status, priority, chat_id, IsDeleted, CreatedAt, LastUpdate)
            VALUES (?, ?, ?, ?, ?, 0, NOW(), NOW())
        `;
        const [result] = await db.execute(sql, [
            title,
            description || null,
            status || "Open",
            priority || "Medium",
            chat_id
        ]);
        return res.status(201).json({ success: true, id: result.insertId });
    } catch (err) {
        console.error("Erro ao inserir ocorrência:", err.message);
        return res.status(500).json({ error: err.message });
    }
});

app.post("/api/tasks", async (req, res) => {
    try {
        const { title, description, status, priority, due_date, client_id } = req.body;
        if (!title || !client_id) {
            return res.status(400).json({ error: "title e client_id são obrigatórios." });
        }
        const sql = `
            INSERT INTO ClientTasks (title, description, status, priority, due_date, client_id, IsDeleted, CreatedAt, LastUpdate)
            VALUES (?, ?, ?, ?, ?, ?, 0, NOW(), NOW())
        `;
        const [result] = await db.execute(sql, [
            title,
            description || null,
            status || "Open",
            priority || "Medium",
            due_date || null,
            client_id
        ]);
        return res.status(201).json({ success: true, id: result.insertId });
    } catch (err) {
        console.error("Erro ao inserir tarefa:", err.message);
        return res.status(500).json({ error: err.message });
    }
});

app.post("/api/chats", async (req, res) => {
    try {
        const { jid, phone_number, name } = req.body;
        if (!jid) {
            return res.status(400).json({ error: "jid é obrigatório." });
        }
        const sql = `
            INSERT INTO Chats (jid, phone_number, name, IsDeleted, CreatedAt, LastUpdate)
            VALUES (?, ?, ?, 0, NOW(), NOW())
        `;
        const [result] = await db.execute(sql, [
            jid,
            phone_number || null,
            name || null
        ]);
        return res.status(201).json({ success: true, id: result.insertId });
    } catch (err) {
        console.error("Erro ao inserir chat:", err.message);
        return res.status(500).json({ error: err.message });
    }
});

app.post("/api/devices", async (req, res) => {
    try {
        const { jid, phone_number, push_name, platform } = req.body;
        if (!jid) {
            return res.status(400).json({ error: "jid é obrigatório." });
        }
        const sql = `
            INSERT INTO Devices (jid, phone_number, push_name, platform, connected_at, updated_at)
            VALUES (?, ?, ?, ?, NOW(), NOW())
        `;
        const [result] = await db.execute(sql, [
            jid,
            phone_number || null,
            push_name || null,
            platform || null
        ]);
        return res.status(201).json({ success: true, id: result.insertId });
    } catch (err) {
        console.error("Erro ao inserir dispositivo:", err.message);
        return res.status(500).json({ error: err.message });
    }
});

export default app;
