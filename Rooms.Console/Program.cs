using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using KolibSoft.Rooms.Core.Protocol;
using KolibSoft.Rooms.Core.Services;
using KolibSoft.Rooms.Core.Sockets;

namespace KolibSoft.Rooms.Console;

public class Serice : RoomService
{

    protected override void OnConnect(IRoomSocket socket)
    {
        base.OnConnect(socket);
        System.Console.WriteLine("Service is online");
    }

    protected override void OnDisconnect(IRoomSocket socket)
    {
        base.OnConnect(socket);
        System.Console.WriteLine("Service is offline");
    }

    protected override void OnMessageReceived(RoomMessage message)
    {
        base.OnMessageReceived(message);
        System.Console.WriteLine($"{message.Verb} [{message.Channel}] {message.Content}");
    }

}

public static class Program
{

    public static string? Prompt(string? hint = "> ")
    {
        System.Console.Write(hint);
        var input = System.Console.ReadLine();
        return input;
    }

    public static string? GetArgument(this string[] args, string name, string? hint = null, bool required = false)
    {
        var argName = $"--{name}";
        string? argument = args.FirstOrDefault(x => x.StartsWith(argName) && (x.Length == argName.Length || x[argName.Length] == '='));
        if (argument != null)
        {
            if (argument.Length == argName.Length) return name;
            if (argument[argName.Length] == '=') return argument[(argName.Length + 1)..];
        }
        while (required && string.IsNullOrWhiteSpace(argument)) argument = Prompt(hint ?? $"{name}: ");
        return argument;
    }

    public static string? GetOption(this string[] args, string name, string[] options, string? hint = null, bool required = false)
    {
        string? option = args.GetArgument(name, hint, required);
        while (required && !options.Contains(option)) option = Prompt(hint ?? $"{name}: ");
        return option;
    }

    public static int? GetInteger(this string[] args, string name, string? hint = null, bool required = false)
    {
        bool parsed;
        int integer;
        while (!(parsed = int.TryParse(args.GetArgument(name, hint, required), out integer)) && required) continue;
        return parsed ? integer : null;
    }

    public static IPEndPoint? GetIPEndpoint(this string[] args, string name, string? hint = null, bool required = false)
    {
        IPEndPoint? endpoint;
        while (!IPEndPoint.TryParse(args.GetArgument(name, hint, required)!, out endpoint) && required) continue;
        return endpoint;
    }

    public static Uri? GetUri(this string[] args, string name, string? hint = null, bool required = false)
    {
        Uri? uri;
        while (!TryParse(args.GetArgument(name, hint, required)!, out uri) && required) continue;
        return uri;
        static bool TryParse(string value, [NotNullWhen(true)] out Uri? uri)
        {
            try
            {
                uri = new Uri(value);
                return true;
            }
            catch
            {
                uri = null;
                return false;
            }
        }
    }

    public static async Task Main(params string[] args)
    {
        var mode = args.GetOption("mode", ["Server", "Client", "Service"], null, true);
        var impl = args.GetOption("impl", ["TCP", "WEB"], null, true);
        var buffering = args.GetInteger("buff") ?? 1024;
        var rating = args.GetInteger("rate") ?? 1024;
        if (mode == "Server")
        {
            if (impl == "TCP")
            {
                var endpoint = args.GetIPEndpoint("endpoint") ?? new IPEndPoint(IPAddress.Any, 55000);
                System.Console.WriteLine($"Using endpoint: {endpoint}");
                var listener = new TcpListener(endpoint);
                _ = ListenAsync(listener, buffering, rating);
                var client = new TcpClient();
                await client.ConnectAsync(new IPEndPoint(IPAddress.Loopback, endpoint.Port));
                var socket = new TcpRoomSocket(client, buffering, buffering);
                _ = CommandAsync(socket);
                await ListenAsync(socket);
            }
            else if (impl == "WEB")
            {
                var uri = args.GetUri("uri") ?? new Uri("http://localhost:55000/");
                System.Console.WriteLine($"Using uri: {uri}");
                var uriString = uri.ToString();
                var listener = new HttpListener();
                listener.Prefixes.Add(uriString);
                _ = ListenAsync(listener, buffering, rating);
                var client = new ClientWebSocket();
                client.Options.AddSubProtocol(WebRoomSocket.SubProtocol);
                await client.ConnectAsync(new Uri(uriString.Replace("http", "ws")), default);
                var socket = new WebRoomSocket(client, buffering);
                _ = CommandAsync(socket);
                await ListenAsync(socket);
            }
        }
        else if (mode == "Client")
        {
            if (impl == "TCP")
            {
                var endpoint = args.GetIPEndpoint("endpoint") ?? new IPEndPoint(IPAddress.Loopback, 55000);
                System.Console.WriteLine($"Using endpoint: {endpoint}");
                var client = new TcpClient();
                await client.ConnectAsync(endpoint);
                var socket = new TcpRoomSocket(client, buffering);
                _ = CommandAsync(socket);
                await ListenAsync(socket);
            }
            else if (impl == "WEB")
            {
                var uri = args.GetUri("uri") ?? new Uri("ws://localhost:55000/");
                System.Console.WriteLine($"Using uri: {uri}");
                var client = new ClientWebSocket();
                client.Options.AddSubProtocol(WebRoomSocket.SubProtocol);
                await client.ConnectAsync(uri, default);
                var socket = new WebRoomSocket(client, buffering);
                _ = CommandAsync(socket);
                await ListenAsync(socket);
            }
        }
        else if (mode == "Service")
        {
            if (impl == "TCP")
            {
                var server = args.GetArgument("server") ?? "127.0.0.1:55000";
                System.Console.WriteLine($"Using server: {server}");
                var service = new Serice { Logger = System.Console.Error };
                await service.ConnectAsync(server, RoomService.TCP, rating);
                while (service.IsOnline) await Task.Delay(100);
            }
            else if (impl == "WEB")
            {
                var server = args.GetArgument("server") ?? "ws://localhost:55000/";
                System.Console.WriteLine($"Using server: {server}");
                var service = new Serice { Logger = System.Console.Error };
                await service.ConnectAsync(server, RoomService.WEB, rating);
                while (service.IsOnline) await Task.Delay(100);
            }
        }
    }

    public static async Task ListenAsync(TcpListener listener, int buffering, int rating)
    {
        var hub = new RoomHub { Logger = System.Console.Error };
        listener.Start();
        System.Console.WriteLine("TCP server started");
        var listening = true;
        while (listening)
        {
            try
            {
                var client = await listener.AcceptTcpClientAsync();
                var socket = new TcpRoomSocket(client, buffering);
                _ = hub.ListenAsync(socket, rating);
                if (hub.Sockets.Length == 1) _ = hub.TransmitAsync();
            }
            catch (Exception error)
            {
                await System.Console.Error.WriteLineAsync($"TCP server error: {error}");
            }
        }
        System.Console.WriteLine("TCP server stopped");
    }

    public static async Task ListenAsync(HttpListener listener, int buffering, int rating)
    {
        var hub = new RoomHub { Logger = System.Console.Error };
        listener.Start();
        System.Console.WriteLine("WEB server started");
        while (listener.IsListening)
        {
            try
            {
                var context = await listener.GetContextAsync();
                if (context.Request.IsWebSocketRequest)
                {
                    var wcontext = await context.AcceptWebSocketAsync(WebRoomSocket.SubProtocol);
                    var socket = new WebRoomSocket(wcontext.WebSocket, buffering);
                    _ = hub.ListenAsync(socket, rating);
                    if (hub.Sockets.Length == 1) _ = hub.TransmitAsync();
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.Close();
                }
            }
            catch (Exception error)
            {
                await System.Console.Error.WriteLineAsync($"WEB server error: {error}");
            }
        }
        System.Console.WriteLine("WEB server stopped");
    }

    public static async Task ListenAsync(IRoomSocket socket)
    {
        var message = new RoomMessage();
        System.Console.WriteLine("Client connected");
        while (socket.IsAlive)
        {
            try
            {
                await socket.ReceiveAsync(message);
                System.Console.SetCursorPosition(0, System.Console.CursorTop);
                System.Console.WriteLine($"{message.Verb} [{message.Channel}] {message.Content}");
                System.Console.Write("> ");
            }
            catch (Exception error)
            {
                await System.Console.Error.WriteLineAsync($"Room client error: {error}");
            }
        }
        System.Console.WriteLine("Client disconnected");
    }

    public static async Task CommandAsync(IRoomSocket socket)
    {
        while (socket.IsAlive)
        {
            var input = await Task.Run(() => Prompt("> "));
            if (RoomMessage.TryParse(input, out RoomMessage? message))
                try
                {
                    await socket.SendAsync(message);
                }
                catch (Exception error)
                {
                    await System.Console.Error.WriteLineAsync($"Room client error: {error}");
                }
            else System.Console.WriteLine("Expected a valid message format: <VERB> <CHANNEL> [CONTENT]");
        }
    }

}