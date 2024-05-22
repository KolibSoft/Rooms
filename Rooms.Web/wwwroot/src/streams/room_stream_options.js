class RoomStreamOptions {

    constructor(json = null) {
        this.readBuffering = json?.readBuffering ?? RoomStreamOptions.DEFAULT_READ_BUFFERING;
        this.writeBuffering = json?.readBuffering ?? RoomStreamOptions.DEFAULT_READ_BUFFERING;
        this.maxVerbLength = json?.maxVerbLength ?? RoomStreamOptions.DEFAULT_MAX_VERB_LENGTH;
        this.maxChannelLength = json?.maxChannelLength ?? RoomStreamOptions.DEFAULT_MAX_CHANNEL_LENGTH_LENGTH;
        this.maxCountLength = json?.maxCountLength ?? RoomStreamOptions.DEFAULT_MAX_COUNT_LENGTH;
        this.maxContentLength = json?.maxContentLength ?? RoomStreamOptions.DEFAULT_MAX_CONTENT_LENGTH_LENGTH;
    }

    static get DEFAULT_READ_BUFFERING() { return 1024; }
    static get DEFAULT_WRITE_BUFFERING() { return 1024; }
    static get DEFAULT_MAX_VERB_LENGTH() { return 128; }
    static get DEFAULT_MAX_CHANNEL_LENGTH() { return 32; }
    static get DEFAULT_MAX_COUNT_LENGTH() { return 32; }
    static get DEFAULT_MAX_CONTENT_LENGTH() { return 4 * 1024 * 1024; }

}

export {
    RoomStreamOptions
}