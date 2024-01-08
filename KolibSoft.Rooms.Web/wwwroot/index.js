var socket = new WebSocket("ws://localhost:5085/api/rooms");
socket.onmessage = ev => {
    console.log(ev.data);
}
socket.onopen = ev => {
    socket.send("MSG 00000000\nMessage Content");
}