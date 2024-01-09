var socket = new WebSocket("ws://localhost:5085/api/rooms/join?code=12345678");
socket.onmessage = ev => {
    console.log(ev.data);
}

socket.onopen = ev => {
    console.log("Opened");
    send("TST", "00000000", "Ping");
}

socket.onclose = ev => {
    console.log("Closed");
}

function send(verb, channel, content) {
    socket.send(`${verb} ${channel}\n${content}`);
}

window.send = send;