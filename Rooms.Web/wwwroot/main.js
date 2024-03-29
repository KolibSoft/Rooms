import { RoomChannel, RoomMessage, RoomVerb, WebRoomSocket } from "./rooms.js";

const encoder = new TextEncoder("utf-8");
let client = new WebSocket("ws://localhost:5000/api/rooms/join", WebRoomSocket.SubProtocol);
client.onopen = async () => {
    let socket = new WebRoomSocket(client);
    listenAsync(socket);
    await socket.sendAsync(RoomMessage.parse(encoder.encode("ECHO 00000000")));
    await new Promise(resolve => setInterval(resolve, 100));
    await socket.sendAsync(RoomMessage.parse(encoder.encode("PING ffffffff PONG")));
};

async function listenAsync(socket) {
    let message = new RoomMessage();
    console.log("Client online");
    while (socket.isAlive) {
        await socket.receiveAsync(message);
        console.log(`${message.verb} [${message.channel}] ${message.content ?? ""}`);
    }
    console.log("Client offline");
}