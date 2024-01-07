function test() {
    var socket = new WebSocket("ws://localhost:5114/api/rooms");
    socket.onmessage = ev => {
        if (!ev.data.includes("ID:00000000"))
            console.log("Received:\n" + ev.data);
    }
    socket.onopen = ev => {
        let message = "ID:0\n\nMessage";
        socket.send(message);
    }
}

test();
test();
test();