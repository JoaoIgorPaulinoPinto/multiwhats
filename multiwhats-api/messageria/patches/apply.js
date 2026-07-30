import fs from "fs";
import path from "path";
import { fileURLToPath } from "url";

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const WWEBJS = path.join(__dirname, "..", "node_modules", "whatsapp-web.js");

const files = [
    {
        src: path.join(__dirname, "Utils.patched.js"),
        dest: path.join(WWEBJS, "src", "util", "Injected", "Utils.js"),
    },
    {
        src: path.join(__dirname, "Message.patched.js"),
        dest: path.join(WWEBJS, "src", "structures", "Message.js"),
    },
];

for (const { src, dest } of files) {
    if (fs.existsSync(src) && fs.existsSync(path.dirname(dest))) {
        fs.copyFileSync(src, dest);
        console.log(`[patch] ${path.basename(dest)} aplicado`);
    }
}
