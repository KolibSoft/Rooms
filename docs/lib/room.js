const RoomProtocol = "Room";
const RoomNoVerb = "NNN";
const RoomLoopback = "00000000";
const RoomBroadcast = "ffffffff";
const RoomNoContent = "";

class RoomMessage {

    constructor(verb = RoomNoVerb, channel = RoomLoopback, content = RoomNoContent) {
        this.verb = verb;
        this.channel = channel;
        this.content = content;
    }

    toString() {
        return `${this.verb} ${this.channel}\n${this.content}`;
    }

}

function parseRoomMessage(string = "") {
    if (string.Length < 13 || string[3] != ' ' || string[12] != '\n')
        throw new Error(`Invalid message format: ${string}`);
    let verb = string.substring(0, 3);
    if (!verb.match(/[A-Za-z]{3}/))
        throw new Error(`Invalid verb format: ${verb}`);
    let channel = string.substring(4, 12);
    if (!channel.match(/[0-9A-Fa-f]{8}/))
        throw new Error(`Invalid channel format: ${channel}`);
    let content = string.substring(13);
    let message = new RoomMessage();
    message.verb = verb;
    message.channel = channel;
    message.content = content;
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
    RoomProtocol,
    RoomNoVerb,
    RoomLoopback,
    RoomBroadcast,
    RoomNoContent,
    RoomMessage,
    parseRoomMessage,
    convertChannel
}