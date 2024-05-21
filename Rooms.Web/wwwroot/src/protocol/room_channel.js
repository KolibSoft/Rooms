import { RoomDataUtils, decoder, encoder } from "./room_data_utils.js";

class RoomChannel {

    #data;

    get data() { return this.#data; }
    get length() { return this.#data.length; }
    toString() { return decoder.decode(this.#data); }
    validate() { return RoomChannel.verify(this.#data); }

    constructor(data = new Uint8Array(0)) {
        if (typeof (data) === "number") {
            if (data < 0) data = encoder.encode('-' + (-data).toString(16));
            else data = encoder.encode('+' + data.toString(16));
        }
        if (!data instanceof Uint8Array) throw new Error("Invalid data type");
        this.#data = data;
    }

    static verify(data) {
        if (data.length < 2 || !RoomDataUtils.isSign(data[0])) return false;
        let index = RoomDataUtils.scanHexadecimal(RoomDataUtils.slice(data, 1)) + 1;
        return index === data.length;
    }

    static parse(data) {
        if (this.verify(data)) {
            if (typeof (data) === "string") data = encoder.encode(data);
            let channel = new RoomChannel(data);
            return channel;
        }
        throw new Error("Room channel format is incorrect");
    }

}

export {
    RoomChannel
}