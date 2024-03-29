const decoder = new TextDecoder("utf-8");
const encoder = new TextEncoder("utf-8");

async function delay(ms) { await new Promise(resolve => setTimeout(resolve, ms)); }

class RoomVerb {

    #data;

    constructor(data = new Uint8Array()) {
        this.#data = data;
    }

    get data() {
        return this.#data;
    }

    get length() {
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
        if (typeof utf8 === 'string') utf8 = encoder.encode(utf8);
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

    constructor(data = new Uint8Array()) {
        this.#data = data;
    }

    get data() {
        return this.#data;
    }

    get length() {
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
        if (typeof utf8 === 'string') utf8 = encoder.encode(utf8);
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

    constructor(data = new Uint8Array()) {
        this.#data = data;
    }

    get data() {
        return this.#data;
    }

    get length() {
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
        if (typeof utf8 === 'string') utf8 = encoder.encode(utf8);
        let data = new Uint8Array(utf8.length);
        utf8.forEach((value, index) => data[index] = value);
        return new RoomContent(data);
    }

}

///////////////////////////////////////////////////////////////////////////////////////////////////

class RoomMessage {

    constructor() {
        this.verb = new RoomVerb();
        this.channel = new RoomChannel();
        this.content = new RoomContent();
    }

    get length() {
        return this.verb.length + this.channel.length + 1 + (this.content && this.content.length > 0 ? this.content.length + 1 : 0);
    }

    toString() {
        return `${this.verb.toString()} ${this.channel.toString()} ${this.content?.toString() ?? ""}`;
    }

    copyTo(target) {
        if (target.length < this.length) throw new Error("Target is too short");
        let offset = 0;
        this.verb.copyTo(target.subarray(offset));
        offset += this.verb.length;
        target[offset] = 32;
        offset += 1;
        this.channel.copyTo(target.subarray(offset));
        offset += this.channel.length;
        if (this.content && this.content.length > 0) {
            target[offset] = 32;
            this.content.copyTo(target.subarray(offset + 1));
        }
    }

    copyFrom(source) {
        let offset = 0;
        let length = RoomVerb.scan(source.subarray(offset));
        this.verb = new RoomVerb(source.subarray(offset, offset + length));
        offset += this.verb.length + 1;
        length = RoomChannel.scan(source.subarray(offset));
        this.channel = new RoomChannel(source.subarray(offset, offset + length));
        offset += this.channel.length;
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
        if (typeof utf8 === 'string') utf8 = encoder.encode(utf8);
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
        await new Promise(async (resolve, reject) => {
            message.copyTo(this.#sendBuffer);
            this.#socket.onerror = event => reject();
            this.#socket.onclose = event => reject();
            this.#socket.send(this.#sendBuffer.slice(0, message.length), { binary: true });
            resolve();
        });
        await delay(100);
    }

    async receiveAsync(message) {
        if (this.#disposed) throw new Error('Socket has been disposed');
        await new Promise((resolve, reject) => {
            this.#socket.onmessage = event => {
                this.#receiveBuffer = new Uint8Array(event.data);
                message.copyFrom(this.#receiveBuffer);
                resolve();
            };
            this.#socket.onerror = event => reject();
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

//////////////////////////////////////////////////////////////////////////////////////////

class RoomService {

    #disposed;
    #socket;

    constructor() {
        this.#disposed = false;
        this.connectors = {
            [RoomService.WEB]: RoomService.WebConnector
        };
        this.#socket = null;
        this.logger = null;
    }

    get socket() { return this.#socket; }

    get isOnline() {
        return this.#socket && this.#socket.IsAlive;
    }

    onConnect(socket) { }
    async connectAsync(connstring, impl, rating = 1024) {
        if (this.#disposed) throw new Error('Service has been disposed');
        try {
            await this.disconnectAsync();
            let connector = this.connectors[impl];
            if (connector) {
                this.#socket = await connector(connstring);
                this.onConnect(this.#socket);
                this.listenAsync(this.#socket, rating);
            }
        } catch (error) {
            if (this.logger) this.logger(`Room Service error: ${error}`);
        }
    }

    onDisconnect(socket) { }
    async disconnectAsync() {
        if (this.#disposed) throw new Error('Service has been disposed');
        try {
            if (this.#socket) {
                this.#socket.dispose();
                this.onDisconnect(this.#socket);
            }
        } catch (error) {
            if (this.logger) this.logger(`Room Service error: ${error}`);
        }
    }

    onMessageReceived(message) { }
    async listenAsync(socket, rating = 1024) {
        let message = new RoomMessage();
        let ttl = 1000;
        let rate = 0;
        let stopwatch = new Date();
        while (socket.isAlive) {
            try {
                await socket.receiveAsync(message);
                rate += message.length;
                if (rate > rating) {
                    socket.dispose();
                    break;
                }
                if (new Date() - stopwatch >= ttl) {
                    rate = 0;
                    stopwatch = new Date();
                }
                this.onMessageReceived(message);
            } catch (error) {
                if (this.logger) this.logger(`Room Service error: ${error}`);
            }
        }
        this.onDisconnect(socket);
    }

    onMessageSent(message) { }
    async sendAsync(message) {
        if (this.#disposed) throw new Error('Service has been disposed');
        if (this.#socket) {
            try {
                await this.#socket.sendAsync(message);
                this.onMessageSent(message);
            } catch (error) {
                if (this.logger) this.logger(`Room Service error: ${error}`);
            }
        }
    }

    dispose() {
        if (!this.#disposed) {
            if (this.#socket) this.#socket.dispose();
            this.#disposed = true;
        }
    }

    static get WEB() { return "WEB" };

    static #webConnector = async (connstring) => {
        return await new Promise((resolve, reject) => {
            let client = new WebSocket(connstring, WebRoomSocket.SubProtocol);
            client.onopen = event => {
                let socket = new WebRoomSocket(client);
                resolve(socket);
            };
            client.onerror = event => reject();
        });
    };

    static get WebConnector() { return this.#webConnector; }

}

export {
    RoomVerb,
    RoomChannel,
    RoomContent,
    RoomMessage,
    WebRoomSocket,
    RoomService
}