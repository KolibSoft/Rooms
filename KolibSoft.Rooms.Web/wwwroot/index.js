var socket = new WebSocket("ws://localhost:5085/api/rooms/join?code=12345678");
socket.onmessage = ev => {
    console.log(ev.data);
}
socket.onopen = ev => {
    socket.send("MSG 00000000\nMessage Content");
}