import { RoomMessage, parseRoomMessage } from "../../docs/lib/room.js";

var socket = new WebSocket("ws://localhost:5085/api/rooms/join?code=12345678");
socket.onmessage = ev => {
    let message = parseRoomMessage(ev.data);
    console.log(message);
}

socket.onopen = ev => {
    console.log("Opened");
    send("TST", "00000000", "Ping");
}

socket.onclose = ev => {
    console.log("Closed");
}

function send(verb, channel, content) {
    var message = new RoomMessage(verb, channel, content);
    socket.send(message.toString());
}

window.send = send;