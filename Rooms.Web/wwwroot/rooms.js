const decoder = new TextDecoder("utf-8");
const encoder = new TextEncoder("utf-8");

class RoomVerb {

    #data;

    constructor(data) {
        this.#data = data;
    }

    get data() {
        return this.#data;
    }

    get Length() {
        return this.#data.length;
    }

    toString() {
        return decoder.decode(this.data);
    }

    equals(obj) {
        if (obj instanceof RoomVerb) {
            return this.data.every((value, index) => value === obj.data[index]);
        }
        return false;
    }

    copyTo(target) {
        if (target.length < this.data.length) throw new Error("Target is too short");
        this.data.forEach((value, index) => target[index] = value);
    }

    static scan(utf8) {
        let index = 0;
        while (index < utf8.length && lookup(utf8[index]))
            index++;
        return index;
        function lookup(c) {
            return c == 95 || (c >= 65 && c <= 90) || (c >= 97 && c <= 122);
        }
    }

    static verify(utf8) {
        return this.scan(utf8) === utf8.length;
    }

    static tryParse(utf8) {
        if (this.verify(utf8)) {
            let data = new Uint8Array(utf8.length);
            utf8.forEach((value, index) => data[index] = value);
            return new RoomVerb(data);
        }
        return null;
    }

    static parse(utf8) {
        let verb = this.tryParse(utf8);
        if (verb !== null) return verb;
        throw new Error(`Invalid verb format: ${decoder.decode(utf8)}`);
    }

}

///////////////////////////////////////////////////////////////////////////////////

class RoomChannel {

    #data;

    constructor(data) {
        this.#data = data;
    }

    get data() {
        return this.#data;
    }

    get Length() {
        return this.#data.length;
    }

    toString() {
        return decoder.decode(this.data);
    }

    equals(obj) {
        if (obj instanceof RoomChannel) {
            return this.data.every((value, index) => value === obj.data[index]);
        }
        return false;
    }

    copyTo(target) {
        if (target.length < this.data.length) throw new Error("Target is too short");
        this.data.forEach((value, index) => target[index] = value);
    }

    toInt() {
        let text = this.toString();
        return parseInt(text, 16);
    }

    static fromInt(int) {
        let data = encoder.encode(int.toString(16));
        return new RoomChannel(data);
    }

    static scan(utf8) {
        let index = 0;
        while (index < utf8.length && lookup(utf8[index]))
            index++;
        return index == 8 ? 8 : 0;
        function lookup(c) {
            return (c >= 48 && c <= 57) || (c >= 65 && c <= 70) || (c >= 97 && c <= 102);
        }
    }

    static verify(utf8) {
        return this.scan(utf8) === utf8.length;
    }

    static tryParse(utf8) {
        if (this.verify(utf8)) {
            let data = new Uint8Array(utf8.length);
            utf8.forEach((value, index) => data[index] = value);
            return new RoomChannel(data);
        }
        return null;
    }

    static parse(utf8) {
        let channel = this.tryParse(utf8);
        if (channel !== null) return channel;
        throw new Error(`Invalid channel format: ${decoder.decode(utf8)}`);
    }

    static #loopback = this.parse(encoder.encode("00000000"));
    static get Loopback() { return this.#loopback; }

    static #broadcast = this.parse(encoder.encode("ffffffff"));
    static get Broadcast() { return this.#broadcast; }

}

///////////////////////////////////////////////////////////////////////////////////

class RoomContent {

    #data;

    constructor(data) {
        this.#data = data;
    }

    get data() {
        return this.#data;
    }

    get Length() {
        return this.#data.length;
    }

    toString() {
        return decoder.decode(this.data);
    }

    equals(obj) {
        if (obj instanceof RoomContent) {
            return this.data.every((value, index) => value === obj.data[index]);
        }
        return false;
    }

    copyTo(target) {
        if (target.length < this.data.length) throw new Error("Target is too short");
        this.data.forEach((value, index) => target[index] = value);
    }

    static create(utf8) {
        let data = new Uint8Array(utf8.length);
        utf8.forEach((value, index) => data[index] = value);
        return new RoomContent(data);
    }

}

///////////////////////////////////////////////////////////////////////////////////////////////////

class RoomMessage {

    constructor() {
        this.verb = null;
        this.channel = null;
        this.content = null;
    }

    get length() {
        return this.verb.Length + this.channel.Length + 1 + (this.content && this.content.Length > 0 ? this.content.Length + 1 : 0);
    }

    toString() {
        return `${this.verb.toString()} ${this.channel.toString()} ${this.content?.toString() ?? ""}`;
    }

    copyTo(target) {
        if (target.length < this.length) throw new Error("Target is too short");
        let offset = 0;
        this.verb.copyTo(target.subarray(offset));
        offset += this.verb.Length;
        target[offset] = 32;
        offset += 1;
        this.channel.copyTo(target.subarray(offset));
        offset += this.channel.Length;
        if (this.content && this.content.Length > 0) {
            target[offset] = 32;
            this.content.copyTo(target.subarray(offset + 1));
        }
    }

    copyFrom(source) {
        let offset = 0;
        let length = RoomVerb.scan(source.subarray(offset));
        this.verb = new RoomVerb(source.subarray(offset, offset + length));
        offset += this.verb.Length + 1;
        length = RoomChannel.scan(source.subarray(offset));
        this.channel = new RoomChannel(source.subarray(offset, offset + length));
        offset += this.channel.Length;
        if (offset < source.length) this.content = new RoomContent(source.slice(offset + 1));
    }

    static verify(utf8) {
        let offset = 0;
        let index = RoomVerb.scan(utf8.subarray(offset));
        if (index === 0 || index === utf8.length) return false;
        offset += index;
        if (!/\s/.test(String.fromCharCode(utf8[offset]))) return false;
        offset++;
        index = RoomChannel.scan(utf8.subarray(offset));
        if (index === 0 || index === utf8.length) return false;
        offset += index;
        if (offset < utf8.length && !/\s/.test(String.fromCharCode(utf8[offset]))) return false;
        return true;
    }

    static tryParse(utf8) {
        if (this.verify(utf8)) {
            const data = new Uint8Array(utf8.length);
            utf8.forEach((value, index) => data[index] = value);
            const message = new RoomMessage();
            message.copyFrom(data);
            return message;
        }
        return null;
    }

    static parse(utf8) {
        let message = this.tryParse(utf8);
        if (message !== null) return message;
        throw new Error(`Invalid message format: ${decoder.decode(utf8)}`);
    }
}

////////////////////////////////////////////////////////////////////////////////////////////////

class WebRoomSocket {

    #disposed;
    #socket;
    #sendBuffer;
    #receiveBuffer;

    constructor(socket = new WebSocket(), sendBuffering = 1024, receiveBuffering = 1024) {
        this.#disposed = false;
        this.#socket = socket;
        this.#sendBuffer = new Uint8Array(sendBuffering);
        this.#receiveBuffer = new Uint8Array(receiveBuffering);
        socket.binaryType = 'arraybuffer';
    }

    get socket() { return this.#socket; }

    get sendBuffer() { return this.#sendBuffer; }
    get receiveBuffer() { return this.#receiveBuffer; }

    get isAlive() {
        return this.#socket.readyState === WebSocket.OPEN;
    }

    async sendAsync(message) {
        if (this.#disposed) throw new Error('Socket has been disposed');
        message.copyTo(this.#sendBuffer);
        this.#socket.send(this.#sendBuffer.slice(0, message.length), { binary: true });
    }

    async receiveAsync(message) {
        if (this.#disposed) throw new Error('Socket has been disposed');
        await new Promise((resolve, reject) => {
            this.#socket.onmessage = event => {
                this.#receiveBuffer = new Uint8Array(event.data);
                message.copyFrom(this.#receiveBuffer);
                resolve();
            };
            this.#socket.onclose = event => reject();
        });
    }

    dispose() {
        if (!this.#disposed) {
            this.#socket.terminate();
            this.#disposed = true;
        }
    }

    static get SubProtocol() {
        return 'Room';
    }

}

export {
    RoomVerb,
    RoomChannel,
    RoomContent,
    RoomMessage,
    WebRoomSocket
}