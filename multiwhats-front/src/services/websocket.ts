import * as signalR from "@microsoft/signalr"
import type { MessageResponse } from "../types/chat"

const BASE_URL = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5261"

export type WsConnectionState = "disconnected" | "connecting" | "connected" | "reconnecting"

export interface WsEventMap {
  "message:received": MessageResponse
  "message:sent": MessageResponse
  "message:raw": unknown
}

export type WsEventName = keyof WsEventMap

type WsCallback<E extends WsEventName> = (data: WsEventMap[E]) => void

class WsClient {
  private connection: signalR.HubConnection | null = null
  private listeners = new Map<string, Set<(...args: unknown[]) => void>>()
  private _state: WsConnectionState = "disconnected"
  private stateCallbacks = new Set<(state: WsConnectionState) => void>()
  private startPromise: Promise<void> | null = null
  private handlersRegistered = false

  private getToken(): string | null {
    if (typeof window === "undefined") return null
    return localStorage.getItem("token")
  }

  get state(): WsConnectionState {
    return this._state
  }

  onStateChange(callback: (state: WsConnectionState) => void) {
    this.stateCallbacks.add(callback)
    return () => this.stateCallbacks.delete(callback)
  }

  private setState(state: WsConnectionState) {
    this._state = state
    this.stateCallbacks.forEach((cb) => cb(state))
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

    this.connection.onreconnecting(() => {
      console.log("[WS] reconectando...")
      this.setState("reconnecting")
    })

    this.connection.onreconnected(() => {
      console.log("[WS] reconectado")
      this.registerHandlers()
      this.setState("connected")
    })

    this.connection.onclose((err) => {
      console.error("[WS] conexão fechada:", err)
      this.connection = null
      this.handlersRegistered = false
      this.setState("disconnected")
    })

    return this.connection
  }

  private registerHandlers() {
    const conn = this.connection
    if (!conn) return
    if (this.handlersRegistered) return
    this.handlersRegistered = true

    conn.on("MessageReceived", (payload: MessageResponse) => {
      this.emit("message:received", payload)
    })

    conn.on("MessageSent", (payload: MessageResponse) => {
      this.emit("message:sent", payload)
    })
  }

  async start() {
    if (this._state === "connected") return
    if (this.startPromise) return this.startPromise

    this.startPromise = this.startInternal()
    return this.startPromise
  }

  private async startInternal() {
    try {
      this.setState("connecting")
      const conn = await this.ensureConnection()

      if (conn.state === signalR.HubConnectionState.Disconnected) {
        await conn.start()
        this.registerHandlers()
        this.setState("connected")
        console.log("[WS] conectado")
      }
    } catch (err) {
      console.error("[WS] falha na conexão:", err)
      this.connection = null
      this.handlersRegistered = false
      this.setState("disconnected")
    } finally {
      this.startPromise = null
    }
  }

  on<E extends WsEventName>(event: E, callback: WsCallback<E>): () => void {
    if (!this.listeners.has(event)) {
      this.listeners.set(event, new Set())
    }
    this.listeners.get(event)!.add(callback as (...args: unknown[]) => void)
    this.start()

    return () => {
      this.listeners.get(event)?.delete(callback as (...args: unknown[]) => void)
    }
  }

  private emit(event: string, data: unknown) {
    this.listeners.get(event)?.forEach((cb) => cb(data))
  }

  async stop() {
    this.startPromise = null
    this.handlersRegistered = false
    if (this.connection) {
      await this.connection.stop()
      this.connection = null
      this.setState("disconnected")
    }
  }

  async refreshToken() {
    await this.stop()
  }
}

export const ws = new WsClient()
