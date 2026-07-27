import app from "./src/app.js";

const port = Number(process.env.PORT) || 3001;
app.get("/ping", (_, res) => {
    res.send("pong");
});

app.listen(port, () => {
    console.log(`API rodando na porta ${port}`);
});