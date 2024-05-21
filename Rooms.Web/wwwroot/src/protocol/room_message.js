class RoomMessage {

    constructor(json) {
        this.verb = json?.verb ?? "";
        this.channel = json?.channel ?? 0;
        this.content = json?.content ?? {};
    }

}

export {
    RoomMessage
}