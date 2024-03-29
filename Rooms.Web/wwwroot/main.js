import { RoomChannel, RoomMessage, RoomVerb, WebRoomSocket } from "./rooms.js";

const encoder = new TextEncoder("utf-8");
let client = new WebSocket("ws://localhost:5000/api/rooms/join", WebRoomSocket.SubProtocol);
client.onopen = async () => {
    let socket = new WebRoomSocket(client);
    listenAsync(socket);
    try {
        await socket.sendAsync(RoomMessage.parse(encoder.encode("ECHO 00000000")));
        await new Promise(resolve => setInterval(resolve, 100));
        await socket.sendAsync(RoomMessage.parse(encoder.encode("PING ffffffff PONG")));
    } catch { }
};

async function listenAsync(socket) {
    let message = new RoomMessage();
    console.log("Client online");
    while (socket.isAlive) {
        try {
            await socket.receiveAsync(message);
            console.log(`${message.verb} [${message.channel}] ${message.content ?? ""}`);
        } catch { }
    }
    console.log("Client offline");
}