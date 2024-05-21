import { RoomChannel } from "./src/protocol/room_channel.js";
import { RoomCount } from "./src/protocol/room_count.js";
import { RoomDataUtils, decoder, encoder } from "./src/protocol/room_data_utils.js";
import { RoomVerb } from "./src/protocol/room_verb.js";
import { RoomStream } from "./src/streams/room_stream.js";


async function delay(ms) { await new Promise(resolve => setTimeout(resolve, ms)); }

console.log(RoomChannel.parse("-0123ABCdfe").toString());
console.log(RoomChannel.parse(encoder.encode("-0123ABCdfe")).toString());

var count = new RoomCount(123);
console.log(count.toString());

let input = [...encoder.encode("TEXT -1 1 A")];
let output = [];

class StreamImpl extends RoomStream {

    async readAsync(chunk) {
        chunk.set(input);
        return input.length;
    }

    async writeAsync(chunk) {
        output.push(...chunk);
        return chunk.length;
    }

    constructor() {
        super(...arguments);
    }

}

let stream = new StreamImpl();
var message = await stream.readMessageAsync();
console.log(message);
await stream.writeMessageAsync(message);
console.log(decoder.decode(new Uint8Array(output)));

////////////////////////////////////////////////////////////////////////////////////////////////////////////////