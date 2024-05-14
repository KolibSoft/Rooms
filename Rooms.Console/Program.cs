
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
    using var hub = new RoomHub() { Logger = Console.Error };
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
    using var hub = new RoomHub() { Logger = Console.Error };
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
    using var service = new RoomClient() { Logger = Console.Error };
    service.Start();
    using var client = new TcpClient();
    await client.ConnectAsync(IPAddress.Loopback, 55000);
    using var stream = new RoomNetworkStream(client);
    CommandAsync();
    await service.ListenAsync(stream);
    ///////////////////////////////////////////////////////
    async void CommandAsync()
    {
        while (service.IsRunning)
        {
            var command = Console.ReadLine();
            try
            {
                var parts = command!.Split(" ");
                var protocol = new RoomProtocol
                {
                    Verb = RoomVerb.Parse($"{parts[0]} "),
                    Channel = RoomChannel.Parse($"{parts[1]} "),
                    Count = (RoomCount)parts[2].Length
                };
                var content = new MemoryStream(Encoding.UTF8.GetBytes(parts[2]));
                await service.SendAsync(protocol, content);
            }
            catch (Exception error)
            {
                await Console.Error.WriteLineAsync($"Error parsing command: {error}");
            }
            await Task.Delay(100);
        }
    }
}

async Task RunWebClient()
{
    using var service = new RoomClient() { Logger = Console.Error };
    service.Start();
    using var client = new ClientWebSocket();
    await client.ConnectAsync(new Uri("ws://localhost:55000"), default);
    using var stream = new RoomWebStream(client);
    CommandAsync();
    await service.ListenAsync(stream);
    //////////////////////////////////////////////////////////////////////
    async void CommandAsync()
    {
        while (service.IsRunning)
        {
            var command = Console.ReadLine();
            try
            {
                var parts = command!.Split(" ");
                var protocol = new RoomProtocol
                {
                    Verb = RoomVerb.Parse($"{parts[0]} "),
                    Channel = RoomChannel.Parse($"{parts[1]} "),
                    Count = (RoomCount)parts[2].Length
                };
                var content = new MemoryStream(Encoding.UTF8.GetBytes(parts[2]));
                await service.SendAsync(protocol, content);
            }
            catch (Exception error)
            {
                await Console.Error.WriteLineAsync($"Error parsing command: {error}");
            }
            await Task.Delay(100);
        }
    }
}

class RoomClient : RoomService
{

    protected override async void OnMessageReceived(IRoomStream stream, RoomProtocol protocol, Stream content)
    {
        var clone = new MemoryStream((int)content.Length);
        await content.CopyToAsync(clone);
        Console.WriteLine($"{protocol.Verb}{protocol.Channel}{protocol.Count}{Encoding.UTF8.GetString(clone.ToArray())}");
    }

    public RoomClient(RoomServiceOptions? options = null) : base(options) { }

}