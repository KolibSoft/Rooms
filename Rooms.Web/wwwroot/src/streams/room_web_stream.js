import { RoomStream } from "./room_stream.js";

class RoomWebStream extends RoomStream {

    #socket;

    get socket() { return this.#socket; }
    get isAlive() { return this.#socket.readyState == WebSocket.OPEN; }

    async readAsync(chunk) {
        if (this.#socket.readyState != WebSocket.OPEN) return 0;
        let count = await new Promise((resolve, reject) => {
            this.#socket.onerror = event => reject("Error reading payload");
            this.#socket.onclose = event => resolve(0);
            this.#socket.onmessage = event => {
                let data = new Uint8Array(event.data);
                if (chunk.length < data.length)
                    throw new Error("Payload too large");
                chunk.set(data);
                this.#socket.onmessage = null;
                this.#socket.onerror = null;
                resolve(data.length);
            };
        });
        return count;
    }

    async writeAsync(chunk) {
        if (this.#socket.readyState != WebSocket.OPEN) return 0;
        this.#socket.send(chunk);
        return chunk.length;
    }

    constructor(args = { socket: null, readBuffer: null, writeBuffer: null, options: null }) {
        if (!(args.socket instanceof WebSocket)) throw new Error("Invalid argument");
        super(args);
        this.#socket = args.socket;
        this.#socket.binaryType = "arraybuffer";
    }

}

export {
    RoomWebStream
}