import fs from "fs";
import path from "path";
import { fileURLToPath } from "url";

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const CLIENT_JS = path.join(
    __dirname, "..", "node_modules", "whatsapp-web.js", "src", "Client.js"
);

if (!fs.existsSync(CLIENT_JS)) {
    console.log("[patch] Client.js não encontrado, pulando");
    process.exit(0);
}

let content = fs.readFileSync(CLIENT_JS, "utf-8");
let modified = false;

// Fix 1: Add timeout to socket state wait
const oldSocketWait = `                await new Promise((r) => {
                    window
                        .require('WAWebSocketModel')
                        .Socket.on(
                            'change:state',
                            function waitTillInit(_AppState, state) {
                                if (
                                    state !== 'OPENING' &&
                                    state !== 'UNLAUNCHED' &&
                                    state !== 'PAIRING'
                                ) {
                                    window
                                        .require('WAWebSocketModel')
                                        .Socket.off(
                                            'change:state',
                                            waitTillInit,
                                        );
                                    r();
                                }
                            },
                        );
                });`;

const newSocketWait = `                await new Promise((r) => {
                    let timeout = setTimeout(() => {
                        window.require('WAWebSocketModel').Socket.off('change:state', waitTillInit);
                        r();
                    }, 30000);
                    window
                        .require('WAWebSocketModel')
                        .Socket.on(
                            'change:state',
                            function waitTillInit(_AppState, state) {
                                if (
                                    state !== 'OPENING' &&
                                    state !== 'UNLAUNCHED' &&
                                    state !== 'PAIRING'
                                ) {
                                    clearTimeout(timeout);
                                    window
                                        .require('WAWebSocketModel')
                                        .Socket.off(
                                            'change:state',
                                            waitTillInit,
                                        );
                                    r();
                                }
                            },
                        );
                });`;

if (content.includes(oldSocketWait)) {
    content = content.replaceAll(oldSocketWait, newSocketWait);
    modified = true;
    console.log("[patch] Timeout adicionado ao socket state wait");
} else if (content.includes(newSocketWait)) {
    console.log("[patch] Timeout já aplicado ao socket state wait");
} else {
    console.log("[patch] Padrão socket wait não encontrado (já modificado?)");
}

// Fix 2: Add UNLAUNCHED to ACCEPTED_STATES
const acceptedStatesRegex = /(const ACCEPTED_STATES\s*=\s*\[[\s\S]*?WAState\.TIMEOUT,)(\n\s*)(\]\s*;)/;
if (acceptedStatesRegex.test(content)) {
    content = content.replace(
        acceptedStatesRegex,
        (match, p1, indent, p3) => {
            return p1 + "\n" + indent + "WAState.UNLAUNCHED," + p3;
        }
    );
    modified = true;
    console.log("[patch] UNLAUNCHED adicionado ao ACCEPTED_STATES");
} else {
    console.log("[patch] Padrão ACCEPTED_STATES não encontrado");
}

if (modified) {
    fs.writeFileSync(CLIENT_JS, content, "utf-8");
    console.log("[patch] Client.js atualizado com sucesso");
} else {
    console.log("[patch] Nenhuma modificação necessária em Client.js");
}
