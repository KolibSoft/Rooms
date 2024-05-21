import { RoomDataUtils, decoder, encoder } from "./room_data_utils.js";

class RoomVerb {

    #data;

    get data() { return this.#data; }
    get length() { return this.#data.length; }
    toString() { return decoder.decode(this.#data); }
    validate() { return RoomVerb.verify(this.#data); }

    constructor(data) {
        if (!data instanceof Uint8Array) throw new Error("Invalid data type");
        this.#data = data;
    }

    static verify(data) {
        if (data.length < 1) return false;
        let index = RoomDataUtils.scanWord(data);
        return index === data.length;
    }

    static parse(data) {
        if (this.verify(data)) {
            if (typeof (data) === "string") data = encoder.encode(data);
            let verb = new RoomVerb(data);
            return verb;
        }
        throw new Error("Room verb format is incorrect");
    }

}

export {
    RoomVerb
}