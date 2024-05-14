
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using KolibSoft.Rooms.Core.Protocol;
using KolibSoft.Rooms.Core.Services;
using KolibSoft.Rooms.Core.Streams;

var mode = Console.ReadLine();
switch (mode)
{
    case "TcpServer": await RunTcpServer(); break;
    case "WebServer": await RunWebServer(); break;
    case "TcpClient": await RunTcpClient(); break;
    case "WebClient": await RunWebClient(); break;
}

async Task RunTcpServer()
{
    using var hub = new RoomHub();
    hub.Start();
    using var listener = new TcpListener(IPAddress.Any, 55000);
    await ListenAsync(listener);
    //////////////////////////////////////////////////////////////
    async Task ListenAsync(TcpListener listener)
    {
        listener.Start();
        while (hub.IsRunning)
            try
            {
                var client = await listener.AcceptTcpClientAsync();
                var stream = new RoomNetworkStream(client);
                _ = hub.ListenAsync(stream);
            }
            catch (Exception error)
            {
                await Console.Error.WriteLineAsync($"Error listening connection: {error}");
            }
        listener.Stop();
    }
}

async Task RunWebServer()
{
    using var hub = new RoomHub();
    hub.Start();
    using var listener = new HttpListener();
    listener.Prefixes.Add("http://localhost:55000/");
    await ListenAsync(listener);
    ///////////////////////////////////////////////////////////
    async Task ListenAsync(HttpListener listener)
    {
        listener.Start();
        while (hub.IsRunning)
            try
            {
                var httpContext = await listener.GetContextAsync();
                var wsContext = await httpContext.AcceptWebSocketAsync(null);
                var socket = wsContext.WebSocket;
                var stream = new RoomWebStream(socket);
                _ = hub.ListenAsync(stream);
            }
            catch (Exception error)
            {
                await Console.Error.WriteLineAsync($"Error listening connection: {error}");
            }
        listener.Stop();
    }
}

async Task RunTcpClient()
{
    using var client = new TcpClient();
    await client.ConnectAsync(IPAddress.Loopback, 55000);
    using var stream = new RoomNetworkStream(client);
    var message = "MESSAGE CONTENT";
    //
    var data = new MemoryStream(Encoding.UTF8.GetBytes(message));
    var protocol = new RoomProtocol
    {
        Verb = RoomVerb.Parse("ECHO "),
        Channel = (RoomChannel)0,
        Count = (RoomCount)data.Length
    };
    await stream.WriteProtocolAsync(protocol);
    await stream.WriteContentAsync((long)protocol.Count, data);
    data.Seek(0, SeekOrigin.Begin);
    await stream.ReadProtocolAsync(protocol);
    await stream.ReadContentAsync((long)protocol.Count, data);
    Console.WriteLine($"> {protocol.Verb}{protocol.Channel}{protocol.Count}[{data.Length} bytes]");
    message = Encoding.UTF8.GetString(data.ToArray());
    await Task.Delay(1000);
}

async Task RunWebClient()
{
    using var client = new ClientWebSocket();
    await client.ConnectAsync(new Uri("ws://localhost:55000"), default);
    using var stream = new RoomWebStream(client);
    var message = "MESSAGE CONTENT";
    //
    var data = new MemoryStream(Encoding.UTF8.GetBytes(message));
    var protocol = new RoomProtocol
    {
        Verb = RoomVerb.Parse("ECHO "),
        Channel = (RoomChannel)0,
        Count = (RoomCount)data.Length
    };
    await stream.WriteProtocolAsync(protocol);
    await stream.WriteContentAsync((long)protocol.Count, data);
    data.Seek(0, SeekOrigin.Begin);
    await stream.ReadProtocolAsync(protocol);
    await stream.ReadContentAsync((long)protocol.Count, data);
    Console.WriteLine($"> {protocol.Verb}{protocol.Channel}{protocol.Count}[{data.Length} bytes]");
    message = Encoding.UTF8.GetString(data.ToArray());
    await Task.Delay(1000);
}