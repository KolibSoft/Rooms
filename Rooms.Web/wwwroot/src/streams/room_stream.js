import { RoomChannel } from "../protocol/room_channel";
import { RoomCount } from "../protocol/room_count";
import { RoomDataUtils, encoder } from "../protocol/room_data_utils";
import { RoomMessage } from "../protocol/room_message";
import { RoomVerb } from "../protocol/room_verb";
import { RoomStreamOptions } from "./room_stream_options";

const BLANK = encoder.encode(" ");

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

    async #getChunkAsync() {
        if (this.#position == this.#length) {
            this.#position = 0;
            this.#length = await this.readAsync(this.#readBuffer);
            if (this.#length < 1)
                return new Uint8Array(0);
        }
        let slice = RoomDataUtils.slice(this.#readBuffer, this.#position, this.#length - this.#position);
        return slice;
    }

    async #readVerbAsync() {
        this.#data.length = 0;
        let done = false;
        while (true) {
            let chunk = await this.#getChunkAsync();
            if (this.#length < 1) throw new Error("Room verb broken");
            let length = RoomDataUtils.scanWord(chunk);
            if (length < chunk.length)
                length += (done = RoomDataUtils.isBlank(chunk[length])) ? 1 : 0;
            this.#position += length;
            if (this.#data.length + length > this.#options.maxVerbLength) throw new Error("Room verb too large");
            if (this.#position < length || done) {
                if (this.#data.length > 0) {
                    this.#data.push(...RoomDataUtils.slice(chunk, 0, length - 1));
                    let verb = new RoomVerb(new Uint8Array(this.#data));
                    return verb;
                }
                if (length > 0) {
                    let verb = new RoomVerb(new Uint8Array(this.#data.slice(0, length - 1)));
                    return verb;
                }
                return new RoomVerb();
            }
            this.#data.push(...chunk);
        }
    }

    async #readChannelAsync() {
        this.#data.length = 0;
        let done = false;
        while (true) {
            let chunk = await this.#getChunkAsync();
            if (this.#length < 1) throw new Error("Room channel broken");
            let length = RoomDataUtils.isSign(chunk[0]) ? 1 : 0;
            if (length < chunk.lastIndexOf)
                length += RoomDataUtils.scanHexadecimal(RoomDataUtils.slice(chunk, length));
            if (length < chunk.length)
                length += (done = RoomDataUtils.isBlank(chunk[length])) ? 1 : 0;
            this.#position += length;
            if (this.#data.length + length > this.#options.maxChannelLength) throw new Error("Room channel too large");
            if (this.#position < length || done) {
                if (this.#data.length > 0) {
                    this.#data.push(...RoomDataUtils.slice(chunk, 0, length - 1));
                    let channel = new RoomChannel(new Uint8Array(this.#data));
                    return channel;
                }
                if (length > 0) {
                    let channel = new RoomChannel(new Uint8Array(this.#data.slice(0, length - 1)));
                    return channel;
                }
                return new RoomChannel();
            }
            this.#data.push(...chunk);
        }
    }

    async #readCountAsync() {
        this.#data.length = 0;
        let done = false;
        while (true) {
            let chunk = await this.#getChunkAsync();
            if (this.#length < 1) throw new Error("Room count broken");
            let length = RoomDataUtils.scanDigit(chunk);
            if (length < chunk.length)
                length += (done = RoomDataUtils.isBlank(chunk[length])) ? 1 : 0;
            this.#position += length;
            if (this.#data.length + length > this.#options.maxCountLength) throw new Error("Room count too large");
            if (this.#position < length || done) {
                if (this.#data.length > 0) {
                    this.#data.push(...RoomDataUtils.slice(chunk, 0, length - 1));
                    let count = new RoomCount(new Uint8Array(this.#data));
                    return count;
                }
                if (length > 0) {
                    let count = new RoomCount(new Uint8Array(this.#data.slice(0, length - 1)));
                    return count;
                }
                return new RoomCount();
            }
            this.#data.push(...chunk);
        }
    }

    async #readContentAsync() {
        let count = await this.#readCountAsync();
        let _count = parseInt(count);
        if (_count == 0) return {};
        if (_count > this.#options.maxContentLength) throw new Error("Room content too large");
        let content = [];
        let index = 0;
        while (index < _count) {
            let chunk = await this.#getChunkAsync();
            if (this.#length < 1) throw new Error("Room content broken");
            let length = Math.min(chunk.length, _count - index);
            content.push(...RoomDataUtils.slice(chunk, 0, length));
            index += length;
            this.#position += length;
        }
        return content;
    }

    async readMessageAsync() {
        if (this.#disposed) throw new Error("RoomStream disposed");
        let verb = await this.#readVerbAsync();
        let channel = await this.#readChannelAsync();
        let content = await this.#readContentAsync();
        let message = new RoomMessage({ verb, channel, content });
        return message;
    }

    async writeAsync(chunk) { throw Error("Abstract member"); }

    async #writeVerbAsync(verb) {
        if (!(verb instanceof RoomVerb)) throw new Error("Invalid argument");
        if (verb.length > this.#options.maxVerbLength) throw new Error("Room verb too large");
        let index = 0;
        while (index < verb.length) {
            let length = await this.writeAsync(RoomDataUtils.slice(verb.data, index));
            if (length < 1) throw new Error("Room verb broken");
            index += length;
        }
        await this.writeAsync(BLANK);
    }

    async #writeChannelAsync(channel) {
        if (!(channel instanceof RoomChannel)) throw new Error("Invalid argument");
        if (channel.length > this.#options.maxChannelLength) throw new Error("Room channel too large");
        let index = 0;
        while (index < channel.length) {
            let length = await this.writeAsync(RoomDataUtils.slice(channel.data, index));
            if (length < 1) throw new Error("Room channel broken");
            index += length;
        }
        await this.writeAsync(BLANK);
    }

    async #writeCountAsync(count) {
        if (!(count instanceof RoomCount)) throw new Error("Invalid argument");
        if (count.length > this.#options.maxChannelLength) throw new Error("Room count too large");
        let index = 0;
        while (index < count.length) {
            let length = await this.writeAsync(RoomDataUtils.slice(count.data, index));
            if (length < 1) throw new Error("Room count broken");
            index += length;
        }
        await this.writeAsync(BLANK);
    }

    async #writeContentAsync(content) {
        if (content.length > this.#options.maxContentLength) throw new Error("Room content too large");
        let count = new RoomCount(content.length);
        await this.#writeCountAsync(count);
        let index = 0;
        while (index < 0) {
            this.#writeBuffer.set(content);
            let _count = content.length;
            let slice = RoomDataUtils.slice(this.#writeBuffer, 0, _count);
            let _index = 0;
            while (_index < slice.length) {
                let length = await this.writeAsync(RoomDataUtils.slice(slice, _index));
                if (length < 1) throw new Error("Room content broken");
                _index += length;
            }
            index += slice.length;
        }
    }

    async writeMessageAsync(message) {
        if (!(count instanceof RoomMessage)) throw new Error("Invalid argument");
        if (this.#disposed) throw new Error("RoomStream disposed");
        let verb = RoomVerb.parse(message.verb);
        let channel = new RoomVerb(message.channel);
        let content = message.content;
        await this.#writeVerbAsync(verb);
        await this.#writeChannelAsync(channel);
        await this.#writeContentAsync(content);
    }

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
        this.#data = [];
        this.#position = 0;
        this.#length = 0;
        this.#disposed = false;
    }

}

export {
    RoomStream
}