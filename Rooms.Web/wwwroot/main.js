import { RoomService, WebRoomSocket } from "./rooms.js";

const isSecure = location.protocol.startsWith("https");
const service = new RoomService();

let server = `${location.protocol}//${location.hostname}:${location.port}/api/room`;
let code = (100000000 * Math.random()).toFixed().toString().padStart(8, "0");
let slots = 4;
let pass = "";
let tag = "";
let buff = 1024;
let rate = 1024;

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
iCommand.onkeyup = function (event) {
    if (event.key === "Enter") {
        tLog.value += `${this.value}\n`;
        this.value = "";
        tLog.scrollTop = tLog.scrollHeight;
    }
};

const iJoin = document.getElementById("iJoin");
iJoin.onclick = async function () {
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
    service.onConnect = function (socket) { tLog.value = "Service online"; };
    service.onDisconnect = function (socket) { tLog.value = "Service offline"; };
    service.onMessageReceived = function (message) { tLog.value += `${message.verb} [${message.channel}] ${message.content}\n`; };
    service.onMessageSent = function (message) { };
    await service.connectAsync(connstring, RoomService.WEB, parseInt(rate))
};