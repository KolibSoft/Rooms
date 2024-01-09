import { RoomProtocol, parseRoomMessage, RoomLoopback, RoomBroadcast, RoomMessage } from "https://kolibsoft.github.io/rooms/lib/room.js";

let socket = null;
let verbOptions = [];
let channelOptions = [RoomLoopback, RoomBroadcast];
let contentOptions = [];

let tRoomEndpoint = document.getElementById("tRoomEndpoint");

let tRoomHint = document.getElementById("tRoomHint");
let bRefresh = document.getElementById("bRefresh");
let dRoomList = document.getElementById("dRoomList");

let tRoomCode = document.getElementById("tRoomCode");
let tRoomSlots = document.getElementById("tRoomSlots");
let tRoomPass = document.getElementById("tRoomPass");
let tRoomTag = document.getElementById("tRoomTag");
let bJoin = document.getElementById("bJoin");

let dRoomLog = document.getElementById("dRoomLog");
let tRoomVerb = document.getElementById("tRoomVerb");
let sRoomVerb = document.getElementById("sRoomVerb");
let tRoomChannel = document.getElementById("tRoomChannel");
let sRoomChannel = document.getElementById("sRoomChannel");
let tRoomContent = document.getElementById("tRoomContent");
let sRoomContent = document.getElementById("sRoomContent");
let bSend = document.getElementById("bSend");

function appendOption(select, value, text) {
    let option = document.createElement("option");
    option.value = value;
    option.text = text ?? value;
    select.append(option);
}

async function fetchRoomList() {
    let response = await fetch(`${tRoomEndpoint.value}?hint=${tRoomHint.value}`);
    if (response.ok) {
        let rooms = await response.json();
        dRoomList.innerHTML = "";
        for (let room of rooms) {
            let element = document.createElement("p");
            element.innerText = `${room.code} ${room.count}/${room.slots} (${room.pass ? "PRIVATE" : "PUBLIC"}) [${room.tag ?? ""}]`;
            dRoomList.append(element);
        }
    }
}
bRefresh.addEventListener("click", fetchRoomList);

function joinRoom() {
    if (socket) socket.close();

    let endpoint = new URL(tRoomEndpoint.value);
    endpoint.protocol = endpoint.protocol == "https:" ? "wss:" : "ws:";
    endpoint.pathname += "/join";
    endpoint.searchParams.set("code", tRoomCode.value);
    endpoint.searchParams.set("slots", tRoomSlots.value);
    endpoint.searchParams.set("pass", tRoomPass.value);
    endpoint.searchParams.set("tag", tRoomTag.value);
    socket = new WebSocket(endpoint.href, RoomProtocol);

    socket.addEventListener("open", function () {
        channelOptions = [RoomLoopback, RoomBroadcast];
        sRoomChannel.innerHTML = "";
        appendOption(sRoomChannel, RoomLoopback, "Loopback");
        appendOption(sRoomChannel, RoomBroadcast, "Broadcast");
        appendRoomLog("Joined");
        bSend.disabled = false;
        fetchRoomList();
    });

    socket.addEventListener("close", function () {
        channelOptions = [];
        sRoomChannel.innerHTML = "";
        appendRoomLog("Left");
        bSend.disabled = true;
        fetchRoomList();
    });

    socket.addEventListener("message", function (event) {
        let message = parseRoomMessage(event.data);
        appendRoomLog(`${message.verb} [${message.channel}] ${message.content}`);
        appendMessageOptions(message);
    });

}
bJoin.addEventListener("click", joinRoom);

function appendRoomLog(log) {
    let element = document.createElement("p");
    element.innerText = log;
    dRoomLog.append(element);
}

function appendMessageOptions(message) {

    if (!verbOptions.includes(message.verb)) {
        verbOptions.push(message.verb);
        let option = document.createElement("option");
        option.value = message.verb;
        option.text = message.verb;
        sRoomVerb.append(option);
    }

    if (!channelOptions.includes(message.channel)) {
        channelOptions.push(message.channel);
        let option = document.createElement("option");
        option.value = message.channel;
        option.text = message.channel;
        sRoomChannel.append(option);
    }

    if (!contentOptions.includes(message.content)) {
        contentOptions.push(message.content);
        let option = document.createElement("option");
        option.value = message.content;
        option.text = message.content;
        sRoomContent.append(option);
    }

}

sRoomVerb.addEventListener("input", function () {
    tRoomVerb.value = sRoomVerb.value;
});

sRoomChannel.addEventListener("input", function () {
    tRoomChannel.value = sRoomChannel.value;
});

sRoomContent.addEventListener("input", function () {
    tRoomContent.value = sRoomContent.value;
});

function sendRoomMessage() {
    let message = new RoomMessage(tRoomVerb.value, tRoomChannel.value, tRoomContent.value);
    appendRoomLog(`${message.verb} [${message.channel}] ${message.content}`);
    appendMessageOptions(message);
    socket.send(message.toString());
}
bSend.addEventListener("click", sendRoomMessage);

let url = new URL(location.href);
url.pathname += "api/rooms";
tRoomEndpoint.value = url.href;

tRoomCode.value = (Math.random() * 99999999).toFixed().toString();

tRoomVerb.value = "TST";
tRoomChannel.value = RoomLoopback;
tRoomContent.value = "Ping";
bSend.disabled = true;

fetchRoomList();