import { RoomMessage } from "../protocol/room_message.js";

async function delay(ms) { await new Promise(resolve => setTimeout(resolve, ms)); }

class RoomService {

    #messages;
    #running;
    #disposed;

    get isRunning() { return this.#running; }
    get isDisposed() { return this.#disposed; }

    async onReceiveAsync(stream, message) { throw new Error("Abstract member"); }

    async listenAsync(stream) {
        if (this.#disposed) throw new Error("RoomService was disposed");
        if (!this.#running) throw new Error("Service not running");
        try {
            while (this.#running && stream.isAlive) {
                let message = await stream.readMessageAsync();
                await this.onReceiveAsync(stream, message);
            }
        } catch (error) {
            if (this.logger) this.logger(`Error receiving message: ${error}`)
        }
    }

    async onSendAsync(stream, message) {
        await stream.writeMessageAsync(message);
    }

    enqueue(stream, message) {
        if (!(message instanceof RoomMessage)) throw new Error("Invalid argument");
        if (this.#disposed) throw new Error("RoomService was disposed");
        if (!this.#running) throw new Error("Service not running");
        this.#messages.push({ stream, message });
    }

    async #transmit() {
        while (this.#running)
            if (this.#messages.length > 0) {
                let context = this.#messages.shift();
                try {
                    await this.onSendAsync(context.stream, context.message);
                } catch (error) {
                    if (this.logger) this.logger(`Error sending message: ${error}`);
                }
            }
            else await delay(100);
    }

    onStart() { this.#transmit(); }
    start() {
        if (this.#disposed) throw new Error("RoomService was disposed");
        if (!this.#running) {
            this.#running = true;
            this.onStart();
        }
    }

    onStop() { }
    stop() {
        if (this.#disposed) throw new Error("RoomService was disposed");
        if (this.#running) {
            this.#running = false;
            this.onStop();
        }
    }

    async onDisposeAsync(disposing) {
        if (!this.#disposed) {
            if (disposing) { }
            this.#running = false;
            this.#disposed = true;
        }
    }

    dispose() { _ = this.onDisposeAsync(true); }
    async disposeAsync() { await this.onDisposeAsync(true); }

    constructor() {
        this.logger = null;
        this.#messages = [];
        this.#running = false;
        this.#disposed = false;
    }

}

export {
    RoomService
}