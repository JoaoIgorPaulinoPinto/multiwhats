import db from "./database/connection.js"
import app from "./app.js";
const port = Number(process.env.PORT) || 3001;
const conn = await db.getConnection();

console.log("Banco conectado!");

conn.release();
app.listen(port, () => {
    console.log(`API rodando na porta ${port}`);
});
app.get("/ping", (_:any, res:any) => {
    res.send("pong");
});