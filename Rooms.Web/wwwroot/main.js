import { RoomMessage, RoomService, RoomWebStream, decoder, encoder } from "./rooms.js";

class RoomClient extends RoomService {

    #stream;

    async onReceiveAsync(stream, message) {
        if (this.onreceive) this.onreceive({ stream, message });
    }

    async listenAsync(stream) {
        if (this.#stream != null) throw new Error("Stream already listening");
        this.#stream = stream;
        await super.listenAsync(stream);
        this.#stream = null;
    }

    enqueue(stream, message) {
        if (this.#stream != stream) throw new Error("Stream already listening");
        super.enqueue(stream, message);
    }

    send(message) {
        if (this.#stream == null) throw new Error("Stream no listening");
        this.enqueue(this.#stream, message);
    }

    onStart() {
        super.onStart();
        console.log("Client started");
    }

    onStop() {
        super.onStop();
        console.log("Client stopped");
    }

    constructor() {
        super();
        this.onreceive = null;
    }

}

const isSecure = location.protocol.startsWith("https");
const client = new RoomClient();
client.logger = message => console.error(message);
let socket = null;

let rooms = [];
let hint = "";

let server = `${location.protocol}//${location.hostname}:${location.port}/api/rooms`;
let name = (100000000 * Math.random()).toFixed().toString().padStart(8, "0");
let tag = "";
let password = "";
let slots = 4;

let commands = [];
let commandIndex = 0;

const dList = document.getElementById("dList");

const iRefresh = document.getElementById("iRefresh");
iRefresh.onclick = async function () {
    iRefresh.disabled = true;
    try {
        let url = new URL(server);
        url.searchParams.set("hint", hint);
        let enpoint = url.toString();
        let response = await fetch(enpoint);
        rooms = await response.json();
        dList.innerHTML = "";
        for (let room of rooms) {
            let span = document.createElement("span");
            span.innerHTML = `${room.name} ${room.count}/${room.slots} (${room.hasPassword ? "PRIVATE" : "PUBLIC"}) [${room.tag ?? ""}]`;
            dList.append(span);
        }
    } catch { }
    iRefresh.disabled = false;
};

const iHint = document.getElementById("iHint");
iHint.value = hint;
iHint.oninput = function () {
    hint = this.value;
    iRefresh.click();
}

const iServer = document.getElementById("iServer");
iServer.value = server;
iServer.oninput = function () { server = this.value; };

const iName = document.getElementById("iName");
iName.value = name;
iName.oninput = function () { name = this.value; };

const iTag = document.getElementById("iTag");
iTag.value = tag;
iTag.oninput = function () { tag = this.value; };

const iPassword = document.getElementById("iPassword");
iPassword.value = password;
iPassword.oninput = function () { password = this.value; };

const iSlots = document.getElementById("iSlots");
iSlots.value = slots;
iSlots.oninput = function () { slots = this.value; };

const tLog = document.getElementById("tLog");
const iCommand = document.getElementById("iCommand");
iCommand.disabled = true;
iCommand.onkeyup = async function (event) {
    if (event.key === "Enter") {
        iCommand.disabled = true;
        try {
            let parts = this.value.split(' ');
            var message = new RoomMessage({
                verb: parts[0],
                channel: parseInt(parts[1]),
                content: encoder.encode(parts.slice(2).join(' '))
            });
            client.send(message);
            tLog.value += `> ${this.value}\n`;
            commands.push(this.value);
            if (commands.length > 16) commands.shift();
            commandIndex = commands.length;
        } catch (error) {
            tLog.value += "< Invalid message format\n";
            console.error(error);
        }
        this.value = "";
        iCommand.disabled = false;
        iCommand.focus()
    }
    else if (event.key == "ArrowDown") {
        if (commandIndex < commands.length - 1) {
            commandIndex++;
            iCommand.value = commands[commandIndex];
        }
    }
    else if (event.key == "ArrowUp") {
        if (commandIndex > 0) {
            commandIndex--;
            iCommand.value = commands[commandIndex];
        }
    } else if (!["ArrowLeft", "ArrowRight"].includes(event.key)) {
        commandIndex = commands.length;
    }
};

const iJoin = document.getElementById("iJoin");
iJoin.onclick = async function () {
    iJoin.disabled = true;
    iCommand.disabled = true;
    socket?.close();
    while (client.isRunning) await new Promise(resolve => setTimeout(resolve, 100));
    let url = new URL(server);
    url.protocol = isSecure ? "wss" : "ws";
    socket = new WebSocket(url);
    socket.onerror = event => {
        console.error("Can not connect");
        iJoin.disabled = false;
    }
    socket.onopen = async event => {
        let stream = new RoomWebStream({ socket });
        await stream.writeMessageAsync(new RoomMessage({
            verb: "OPTIONS",
            content: encoder.encode(JSON.stringify({
                name: iName.value,
                tag: iTag.value,
                password: iPassword.value,
                slots: parseInt(iSlots.value)
            }))
        }));
        let message = await stream.readMessageAsync();
        console.log(message);
        iJoin.disabled = false;
        iCommand.disabled = false;
        iRefresh.click();
        if (!client.isRunning) client.start();
        await client.listenAsync(stream);
        socket.close();
        if (client.isRunning) client.stop();
    }
};

client.onreceive = async function (event) {
    tLog.value += `[${event.message.channel}] ${event.message.verb}: ${decoder.decode(event.message.content)}\n`;
};

iRefresh.click();