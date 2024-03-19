using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using KolibSoft.Rooms.Core;

namespace KolibSoft.Rooms.Console;

public class Service : RoomService
{

    protected override void OnConnect(IRoomSocket socket)
    {
        base.OnConnect(socket);
        System.Console.WriteLine($"Service Online");
    }

    protected override async void OnMessageReceived(RoomMessage message)
    {
        base.OnMessageReceived(message);
        System.Console.WriteLine($"{message.Verb} [{message.Channel}] {message.Content}");
        await SendAsync(message);
    }

    protected override void OnDisconnect(IRoomSocket socket)
    {
        base.OnDisconnect(socket);
        System.Console.WriteLine($"Service Offline");
    }

}

public static class Program
{

    public static string? Prompt(string hint)
    {
        System.Console.Write(hint);
        var input = System.Console.ReadLine();
        return input;
    }

    public static string GetArgument(this string[] args, string name, string? hint = null, string? def = null)
    {
        var arg = args.FirstOrDefault(x => x.StartsWith(name));
        if (arg != null)
        {
            if (arg.Length == name.Length) return arg;
            if (arg[name.Length] == '=') return arg.Substring(name.Length + 1);
        }
        string? input = null;
        while (input == null) input = Prompt(hint ?? $"{name}: ") ?? def;
        return input;
    }

    public static int EnsureInteger(Func<string> func, int min = int.MinValue, int max = int.MaxValue)
    {
        if (max < min)
            throw new ArgumentException("Min value and max value overlaps");
        while (true) if (int.TryParse(func(), out int integer) && integer >= min && integer <= max) return integer;
    }

    public static Uri EnsureUri(Func<string> func)
    {
        while (true) if (Uri.TryCreate(func(), UriKind.RelativeOrAbsolute, out Uri? uri)) return uri;
    }

    public static string GetOption(this string[] args, string name, string[] options, string? hint = null)
    {
        if (!options.Any())
            throw new ArgumentException("Options can not be empty");
        while (true)
        {
            var input = args.GetArgument(name, hint, null);
            if (options.Contains(input)) return input;
        }
    }

    public static async Task Main(params string[] args)
    {
        var mode = args.GetOption("--mode", ["Client", "Server", "Service"], "Choose run mode [Client, Server, Service]: ");
        var impl = args.GetOption("--impl", ["TCP", "WEB"], "Choose implementation to use [TCP, WEB]: ");
        if (mode == "Client")
        {
            if (impl == "TCP")
            {
                var host = args.GetArgument("--host", "Enter remote host: ");
                var port = EnsureInteger(() => args.GetArgument("--port", "Enter remote port: "));
                var client = new TcpClient();
                await client.ConnectAsync(host, port);
                var socket = new TcpRoomSocket(client);
                Task.WaitAll(ReceiveAsync(socket), SendAsync(socket));
            }
            else if (impl == "WEB")
            {
                var uri = EnsureUri(() => args.GetArgument("--uri", "Enter remote URI: "));
                var client = new ClientWebSocket();
                client.Options.AddSubProtocol(WebRoomSocket.SubProtocol);
                await client.ConnectAsync(uri, default);
                var socket = new WebRoomSocket(client);
                Task.WaitAll(ReceiveAsync(socket), SendAsync(socket));
            }
        }
        else if (mode == "Server")
        {
            if (impl == "TCP")
            {
                var port = EnsureInteger(() => args.GetArgument("--port", "Enter local port to listen: "));
                var listener = new TcpListener(IPAddress.Any, port);
                _ = ListenAsync(listener);
                var client = new TcpClient();
                await client.ConnectAsync("localhost", port);
                var socket = new TcpRoomSocket(client);
                Task.WaitAll(ReceiveAsync(socket), SendAsync(socket));
            }
            else if (impl == "WEB")
            {
                var prefix = args.GetArgument("--prefix", "Enter http prefix: ");
                var listener = new HttpListener();
                listener.Prefixes.Add(prefix);
                _ = ListenAsync(listener);
                var client = new ClientWebSocket();
                client.Options.AddSubProtocol(WebRoomSocket.SubProtocol);
                await client.ConnectAsync(new Uri("prefix".Replace("http", "ws")), default);
                var socket = new WebRoomSocket(client);
                Task.WaitAll(ReceiveAsync(socket), SendAsync(socket));
            }
        }
        else if (mode == "Service")
        {
            var server = args.GetArgument("--server", "Enter Room server: ");
            var service = new Service();
            await service.ConnectAsync(server, impl);
            while (service.Socket?.IsAlive == true) await Task.Delay(100);
            await Task.Delay(100);
        }
    }

    public static async Task ListenAsync(TcpListener listener)
    {
        var hub = new RoomHub();
        listener.Start();
        System.Console.WriteLine("TCP Room Server started");
        while (true)
        {
            var client = await listener.AcceptTcpClientAsync();
            var socket = new TcpRoomSocket(client);
            _ = hub.ListenAsync(socket);
            if (hub.Sockets.Count == 1) _ = hub.TransmitAsync();
        }
    }

    public static async Task ListenAsync(HttpListener listener)
    {
        var hub = new RoomHub();
        listener.Start();
        System.Console.WriteLine("WEB Room Server started");
        while (true)
        {
            var context = await listener.GetContextAsync();
            if (!context.Request.IsWebSocketRequest)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.Close();
                continue;
            }
            var client = await context.AcceptWebSocketAsync(WebRoomSocket.SubProtocol);
            var socket = new WebRoomSocket(client.WebSocket);
            _ = hub.ListenAsync(socket);
            if (hub.Sockets.Count == 1) _ = hub.TransmitAsync();
        }
    }

    public static async Task ReceiveAsync(IRoomSocket socket)
    {
        while (socket.IsAlive)
        {
            try
            {
                var message = await socket.ReceiveAsync();
                System.Console.SetCursorPosition(0, System.Console.CursorTop);
                System.Console.WriteLine($"{message.Verb} [{message.Channel}] {message.Content}");
                System.Console.Write("> ");
            }
            catch { }
            await Task.Delay(100);
        }
    }

    public static async Task SendAsync(IRoomSocket socket)
    {
        while (socket.IsAlive)
        {
            var input = Prompt("> ");
            if (input != null)
            {
                var @string = input.AsMemory();
                if (@string.Length < 13) { System.Console.WriteLine("Expected: <verb> <channel> <content>"); continue; }
                if (!RoomVerb.Verify(@string.Slice(0, 3).Span)) { System.Console.WriteLine("Expected a valid verb"); continue; }
                if (!RoomChannel.Verify(@string.Slice(4, 8).Span)) { System.Console.WriteLine("Expected a valid channel"); continue; }
                var message = new RoomMessage
                {
                    Verb = RoomVerb.Parse(@string.Slice(0, 3).Span),
                    Channel = RoomChannel.Parse(@string.Slice(4, 8).Span),
                    Content = RoomContent.Parse(@string.Slice(13).Span)
                };
                try
                {
                    await socket.SendAsync(message);
                }
                catch { }
            }
        }
    }

}