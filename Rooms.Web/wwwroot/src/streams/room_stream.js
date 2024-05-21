import { RoomStreamOptions } from "./room_stream_options";

class RoomStream {

    #options;
    #data;
    #readBuffer;
    #writeBuffer;
    #position;
    #length;
    #disposed;

    get options() { return this.#options; }
    get isAlive() { throw Error("Abstract member"); }
    get isDisposed() { return this.#disposed; }

    async readAsync(chunk) { throw Error("Abstract member"); }
    async #getChunkAsync() { }
    async #readVerbAsync() { }
    async #readChannelAsync() { }
    async #readCountAsync() { }
    async #readContentAsync() { }
    async readMessageAsync() { }

    async writeAsync(chunk) { throw Error("Abstract member"); }
    async #writeVerbAsync(verb) { }
    async #writeChannelAsync(channel) { }
    async #writeCountAsync(count) { }
    async #writeContentAsync(content) { }
    async writeMessageAsync(message) { }

    async onDisposeAsync(disposing) {
        if (!this.#disposed) {
            if (disposing)
                _data = null;
            this.#readBuffer = null;
            this.#writeBuffer = null;
            this.#disposed = true;
        }
    }

    dispose() { _ = this.onDisposeAsync(true); }
    async disposeAsync() { await this.onDisposeAsync(true); }

    constructor() {
        if (arguments.length = 3) {
            if (!(arguments[0] instanceof Uint8Array) || !(arguments[1] instanceof Uint8Array))
                throw new Error("Invalid arguments");
            this.#readBuffer = arguments[0];
            this.#writeBuffer = arguments[1];
            this.#options = new RoomStreamOptions(arguments[2]);
        } else {
            this.#options = new RoomStreamOptions(arguments[0]);
            this.#readBuffer = new Uint8Array(this.#options.readBuffering);
            this.#writeBuffer = new Uint8Array(this.#options.writeBuffering);
        }
    }


}

export {
    RoomStream
}