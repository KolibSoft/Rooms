import { RoomChannel, RoomMessage, RoomService, RoomVerb, WebRoomSocket } from "./rooms.js";

const encoder = new TextEncoder("utf-8");

let service = new RoomService();
service.logger = log => console.log(log);
service.onConnect = socket => console.log("Service online");
service.onDisconnect = socket => console.log("Service offline");
service.onMessageReceived = message => console.log(`${message.verb} [${message.channel}] ${message.content}`);
await service.connectAsync("ws://localhost:5000/api/rooms/join", RoomService.WEB);
await service.sendAsync(RoomMessage.parse("ECHO 00000000"));
await new Promise(resolve => setTimeout(resolve, 100));
await service.sendAsync(RoomMessage.parse("PING ffffffff PONG"));