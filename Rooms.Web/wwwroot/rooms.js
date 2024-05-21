import { RoomChannel } from "./src/protocol/room_channel.js";
import { RoomCount } from "./src/protocol/room_count.js";
import { RoomDataUtils, encoder } from "./src/protocol/room_data_utils.js";
import { RoomVerb } from "./src/protocol/room_verb.js";


async function delay(ms) { await new Promise(resolve => setTimeout(resolve, ms)); }

console.log(RoomChannel.parse("-0123ABCdfe").toString());
console.log(RoomChannel.parse(encoder.encode("-0123ABCdfe")).toString());

var count = new RoomCount(123);
console.log(count.toString());

////////////////////////////////////////////////////////////////////////////////////////////////////////////////