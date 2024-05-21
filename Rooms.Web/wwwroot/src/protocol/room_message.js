class RoomMessage {

    constructor(json = null) {
        this.verb = json?.verb ?? "";
        this.channel = json?.channel ?? 0;
        this.content = json?.content ?? new ArrayBuffer();
    }

}

export {
    RoomMessage
}