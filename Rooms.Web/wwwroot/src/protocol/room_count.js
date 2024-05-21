import { RoomDataUtils, decoder, encoder } from "./room_data_utils.js";

class RoomCount {

    #data;

    get data() { return this.#data; }
    get length() { return this.#data.length; }
    toString() { return decoder.decode(this.#data); }
    validate() { return RoomCount.verify(this.#data); }

    constructor(data = new Uint8Array(0)) {
        if (typeof (data) === "number") {
            if (data < 0) throw Error("Negative values are not allowed");
            data = encoder.encode(data.toString());
        }
        if (!data instanceof Uint8Array) throw new Error("Invalid data type");
        this.#data = data;
    }

    static verify(data) {
        if (data.length < 1) return false;
        let index = RoomDataUtils.scanDigit(data);
        return index === data.length;
    }

    static parse(data) {
        if (this.verify(data)) {
            if (typeof (data) === "string") data = encoder.encode(data);
            let count = new RoomCount(data);
            return count;
        }
        throw new Error("Room count format is incorrect");
    }

}

export {
    RoomCount
}