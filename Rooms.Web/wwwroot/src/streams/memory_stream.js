class MemoryStream {

    #buffer;
    #position;
    #length;

    get position() { return this.#position; }
    set position(value) { this.#position = Math.min(this.#position, this.#length); }

    get length() { return this.#length; }
    set length(value) { this.#length = value; this.#position = Math.min(this.#position, this.#length); }

    toArray() { return new Uint8Array(this.#buffer.slice(0, this.#length)); }

    async readAsync(buffer) {
        return await new Promise(resolve => {
            const bytesRead = Math.min(this.#length, this.#length - this.#position);
            new Uint8Array(buffer).set(new Uint8Array(this.#buffer, this.#position, bytesRead));
            this.#position += bytesRead;
            this.#length -= bytesRead;
            resolve(bytesRead);
        });
    }

    async writeAsync(bytes) {
        await new Promise(resolve => {
            const byteLength = bytes.byteLength;
            this.#ensureCapacity(byteLength);
            new Uint8Array(this.#buffer, this.#position, byteLength).set(bytes);
            this.#position += byteLength;
            this.#length += byteLength;
            resolve();
        });
    }

    #ensureCapacity(bytesNeeded) {
        const requiredCapacity = this.#position + bytesNeeded;
        if (requiredCapacity > this.#buffer.byteLength) {
            const newCapacity = Math.max(this.#buffer.byteLength * 2, requiredCapacity);
            const newBuffer = new ArrayBuffer(newCapacity);
            new Uint8Array(newBuffer).set(new Uint8Array(this.#buffer));
            this.#buffer = newBuffer;
        }
    }

    constructor(buffer = new ArrayBuffer(0)) {
        if (!(buffer instanceof ArrayBuffer)) throw new Error("Invalid argument");
        this.#buffer = buffer;
        this.#position = 0;
        this.#length = buffer.byteLength;
    }

}

export {
    MemoryStream
}