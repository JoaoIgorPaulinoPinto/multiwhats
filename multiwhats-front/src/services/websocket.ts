import * as signalR from "@microsoft/signalr"

const BASE_URL = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5261"

class WsClient {
  private connection: signalR.HubConnection | null = null
  private listeners = new Map<string, Set<(data: unknown) => void>>()
  private started = false

  private getToken(): string | null {
    if (typeof window === "undefined") return null
    return localStorage.getItem("token")
  }

  private async ensureConnection() {
    if (this.connection) return this.connection

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(`${BASE_URL}/whatsappHub`, {
        accessTokenFactory: () => this.getToken() ?? "",
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .configureLogging(signalR.LogLevel.Warning)
      .build()

    this.connection.onreconnected(() => {
      console.log("[WS] reconectado")
    })

    this.connection.onclose((err) => {
      console.error("[WS] conexão fechada:", err)
      this.connection = null
      this.started = false
    })

    return this.connection
  }

  async start() {
    if (this.started) return
    const conn = await this.ensureConnection()
    if (conn.state === signalR.HubConnectionState.Disconnected) {
      await conn.start()
      this.started = true
      console.log("[WS] conectado")

      conn.on("ReceberNovaMensagem", (payload: unknown) => {
        this.emit("message:raw", payload)
      })

      conn.on("MessageReceived", (payload: unknown) => {
        this.emit("message:received", payload)
      })

      conn.on("MessageSent", (payload: unknown) => {
        this.emit("message:sent", payload)
      })
    }
  }

  on(event: string, callback: (data: unknown) => void) {
    if (!this.listeners.has(event)) {
      this.listeners.set(event, new Set())
    }
    this.listeners.get(event)!.add(callback)
    this.start()

    return () => {
      this.listeners.get(event)?.delete(callback)
    }
  }

  private emit(event: string, data: unknown) {
    this.listeners.get(event)?.forEach((cb) => cb(data))
  }

  async stop() {
    if (this.connection) {
      await this.connection.stop()
      this.connection = null
      this.started = false
    }
  }
}

export const ws = new WsClient()
