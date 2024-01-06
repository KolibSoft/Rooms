var socket = new WebSocket("ws://localhost:5114/api/rooms");
socket.onmessage = ev => {
    console.log("Received: " + ev.data);
}
socket.onopen = ev => {
    let message = "ID:0\n\nMessage";
    console.log("Sended:\n" + message);
    socket.send(message);
}