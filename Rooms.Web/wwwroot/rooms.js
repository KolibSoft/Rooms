const RoomSubProtocol = "Room";
const RoomLoopback = "00000000";
const RoomBroadcast = "ffffffff";
const encoder = new TextEncoder('utf-8');
const decoder = new TextDecoder('utf-8');

class RoomMessage {

    constructor(verb, channel, content) {
        this.verb = encoder.encode(verb);
        this.channel = encoder.encode(channel);
        this.content = encoder.encode(content);
    }

    toString() {
        return `${decoder.decode(this.verb)} ${decoder.decode(this.channel)} ${decoder.decode(this.content)}`;
    }

}

function scanVerb(utf8) {
    var index = 0;
    while (index < utf8.length && lookup(utf8[index]))
        index++;
    return index;
    function lookup(c) { return c === 95 || (c >= 65 && c <= 90) || (c >= 97 && c <= 122); }
}

function scanChannel(utf8) {
    var index = 0;
    while (index < utf8.length && lookup(utf8[index]))
        index++;
    return index == 8 ? 8 : 0;
    function lookup(c) { return (c >= 48 && c <= 57) || (c >= 65 && c <= 70) || (c >= 97 && c <= 102); }
}

async function parseRoomMessage(blob) {
    let message = new RoomMessage();
    let buffer = new Uint8Array(await blob.arrayBuffer());
    let offset = 0;
    let index = scanVerb(buffer.slice(offset));
    message.verb = buffer.slice(offset, offset + index);
    offset += index + 1;
    index = scanChannel(buffer.slice(offset));
    message.channel = buffer.slice(offset, offset + index);
    offset += index;
    if (offset < buffer.length) message.content = buffer.slice(offset + 1);
    return message;
}

function convertChannel(srcChannel, dstChannel) {
    let src = parseInt(srcChannel, 16);
    let dst = parseInt(dstChannel, 16);
    let convert = src ^ dst;
    let channel = convert.toString(16).padStart(8, "0");
    return channel;
}

export {
    RoomSubProtocol,
    RoomLoopback,
    RoomBroadcast,
    RoomMessage,
    parseRoomMessage,
    convertChannel
}