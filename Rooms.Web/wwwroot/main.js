import { RoomMessage, RoomService, WebRoomSocket } from "./rooms.js";

const isSecure = location.protocol.startsWith("https");
const service = new RoomService();
service.logger = message => console.log(message);

let rooms = [];
let hint = "";

let server = `${location.protocol}//${location.hostname}:${location.port}/api/room`;
let code = (100000000 * Math.random()).toFixed().toString().padStart(8, "0");
let slots = 4;
let pass = "";
let tag = "";
let buff = 1024;
let rate = 1024;

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
            span.innerHTML = `${room.code} ${room.count}/${room.slots} (${room.pass ? "RPIVATE" : "PUBLIC"}) [${room.tag ?? ""}]`;
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

const iCode = document.getElementById("iCode");
iCode.value = code;
iCode.oninput = function () { code = this.value; };

const iSlots = document.getElementById("iSlots");
iSlots.value = slots;
iSlots.oninput = function () { slots = this.value; };

const iPass = document.getElementById("iPass");
iPass.value = pass;
iPass.oninput = function () { pass = this.value; };

const iTag = document.getElementById("iTag");
iTag.value = tag;
iTag.oninput = function () { tag = this.value; };

const iBuff = document.getElementById("iBuff");
iBuff.value = buff;
iBuff.oninput = function () { buff = this.value; };

const iRate = document.getElementById("iRate");
iRate.value = rate;
iRate.oninput = function () { rate = this.value; };

const tLog = document.getElementById("tLog");
const iCommand = document.getElementById("iCommand");
iCommand.disabled = true;
iCommand.onkeyup = async function (event) {
    if (event.key === "Enter") {
        iCommand.disabled = true;
        let message = RoomMessage.tryParse(this.value);
        if (message) await service.sendAsync(message);
        else tLog.value += "< Invalid message format\n";
        commands.push(this.value);
        if (commands.length > 16) commands.shift();
        commandIndex = commands.length;
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
    await service.disconnectAsync();
    let url = new URL(server);
    url.protocol = isSecure ? "wss" : "ws";
    url.pathname += "/join";
    url.searchParams.set("code", code);
    url.searchParams.set("slots", slots);
    url.searchParams.set("pass", pass);
    url.searchParams.set("tag", tag);
    url.searchParams.set("buff", buff);
    url.searchParams.set("rate", rate);
    let connstring = url.toString();
    service.onOnline = function (socket) {
        if (socket == this.socket) {
            tLog.value = "< Service online\n";
            commands = [];
            tLog.scrollTop = tLog.scrollHeight;
            iCommand.disabled = false;
            iRefresh.click();
        }
    };
    service.onOffline = function (socket) {
        if (socket == this.socket) {
            tLog.value += "< Service offline\n";
            tLog.scrollTop = tLog.scrollHeight;
            iCommand.disabled = true;
            iRefresh.click();
        }
    };
    service.onMessageReceived = function (message) {
        tLog.value += `${message.verb} [${message.channel}] ${message.content}\n`;
        tLog.scrollTop = tLog.scrollHeight;
    };
    service.onMessageSent = function (message) {
        tLog.value += `${message.verb} [${message.channel}] ${message.content}\n`;
        tLog.scrollTop = tLog.scrollHeight;
    };
    await service.connectAsync(connstring, RoomService.WEB, parseInt(rate))
};

iRefresh.click();