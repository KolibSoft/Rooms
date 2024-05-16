
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using KolibSoft.Rooms.Core.Protocol;
using KolibSoft.Rooms.Core.Services;
using KolibSoft.Rooms.Core.Streams;

var mode = await args.GetOptionAsync("mode", ["Server", "Client"]);
var impl = await args.GetOptionAsync("impl", ["TCP", "WEB"]);
var buffering = await args.GetIntegerAsync("buff", 1024);
var rating = await args.GetIntegerAsync("rate", 1024);
if (mode == "Server")
{
    if (impl == "TCP")
    {
        var endpoint = await args.GetIPEndpointAsync("endpoint", new IPEndPoint(IPAddress.Any, 55000));
        Console.WriteLine($"Using endpoint: {endpoint}");
        using var service = new RoomServer() { Logger = Console.Error };
        service.Start();
        using var listener = new TcpListener(endpoint!);
        CommandAsync(service);
        await ListenTcpAsync(service, listener);
    }
    else if (impl == "WEB")
    {
        var uri = await args.GetUriAsync("uri", new Uri("http://localhost:55000/"));
        Console.WriteLine($"Using uri: {uri}");
        using var service = new RoomServer() { Logger = Console.Error };
        service.Start();
        using var listener = new HttpListener();
        listener.Prefixes.Add(uri!.ToString());
        CommandAsync(service);
        await ListenWebAsync(service, listener);
    }
}
else if (mode == "Client")
{
    if (impl == "TCP")
    {
        var endpoint = await args.GetIPEndpointAsync("endpoint", new IPEndPoint(IPAddress.Loopback, 55000));
        Console.WriteLine($"Using endpoint: {endpoint}");
        using var service = new RoomClient() { Logger = Console.Error };
        service.Start();
        using var client = new TcpClient();
        await client.ConnectAsync(endpoint!);
        using var stream = new RoomNetworkStream(client);
        CommandAsync(service);
        await service.ListenAsync(stream);
    }
    else if (impl == "WEB")
    {
        var uri = await args.GetUriAsync("uri", new Uri("ws://localhost:55000/"));
        Console.WriteLine($"Using uri: {uri}");
        using var service = new RoomClient() { Logger = Console.Error };
        service.Start();
        using var client = new ClientWebSocket();
        await client.ConnectAsync(uri!, default);
        using var stream = new RoomWebStream(client);
        CommandAsync(service);
        await service.ListenAsync(stream);
    }
}

return;

static async Task ListenTcpAsync(IRoomService service, TcpListener listener)
{
    listener.Start();
    while (service.IsRunning)
        try
        {
            var client = await listener.AcceptTcpClientAsync();
            var stream = new RoomNetworkStream(client);
            _ = service.ListenAsync(stream);
        }
        catch (Exception error)
        {
            await Console.Error.WriteLineAsync($"Error listening connection: {error}");
        }
    listener.Stop();
}

static async Task ListenWebAsync(IRoomService service, HttpListener listener)
{
    listener.Start();
    while (service.IsRunning)
        try
        {
            var httpContext = await listener.GetContextAsync();
            var wsContext = await httpContext.AcceptWebSocketAsync(null);
            var socket = wsContext.WebSocket;
            var stream = new RoomWebStream(socket);
            _ = service.ListenAsync(stream);
        }
        catch (Exception error)
        {
            await Console.Error.WriteLineAsync($"Error listening connection: {error}");
        }
    listener.Stop();
}

async void CommandAsync(IRoomService service)
{
    while (service.IsRunning)
    {
        var command = await Task.Run(() => Console.ReadLine());
        try
        {
            var parts = command!.Split(" ");
            var message = new RoomMessage
            {
                Verb = parts[1],
                Channel = int.Parse(parts[2]),
                Content = new MemoryStream(Encoding.UTF8.GetBytes(string.Join(' ', parts.AsSpan().Slice(3).ToArray())))
            };
            service.Send(int.Parse(parts[0]), message);
        }
        catch (Exception error)
        {
            await Console.Error.WriteLineAsync($"Error parsing command: {error}");
        }
        await Task.Delay(100);
    }
}

public static class ConsoleUtils
{

    public static Task<string?> PromptAsync(string? hint = "> ") => Task.Run(() =>
    {
        Console.Write(hint);
        var input = Console.ReadLine();
        return input;
    });

    [return: NotNullIfNotNull(nameof(def))]
    public static async Task<string?> GetArgumentAsync(this string[] args, string name, string? def = null, string? hint = null)
    {
        var argName = $"--{name}";
        string? argument = args.FirstOrDefault(x => x.StartsWith(argName) && (x.Length == argName.Length || x[argName.Length] == '='));
        if (argument != null)
        {
            if (argument.Length == argName.Length) return name;
            if (argument[argName.Length] == '=') return argument[(argName.Length + 1)..];
        }
        if (!string.IsNullOrWhiteSpace(def)) return def;
        while (string.IsNullOrWhiteSpace(argument)) argument = await PromptAsync(hint ?? $"{name}: ");
        return argument;
    }

    [return: NotNullIfNotNull(nameof(def))]
    public static async Task<string?> GetOptionAsync(this string[] args, string name, string[] options, string? def = null, string? hint = null)
    {
        string? option = await args.GetArgumentAsync(name, def, hint);
        while (!options.Contains(option)) option = await PromptAsync(hint ?? $"{name}: ");
        return option;
    }

    [return: NotNullIfNotNull(nameof(def))]
    public static async Task<int?> GetIntegerAsync(this string[] args, string name, int? def = null, string? hint = null)
    {
        int integer;
        while (!int.TryParse(await args.GetArgumentAsync(name, def?.ToString(), hint), out integer)) continue;
        return integer;
    }

    [return: NotNullIfNotNull(nameof(def))]
    public static async Task<IPEndPoint?> GetIPEndpointAsync(this string[] args, string name, IPEndPoint? def = null, string? hint = null)
    {
        IPEndPoint? endpoint;
        while (!IPEndPoint.TryParse((await args.GetArgumentAsync(name, def?.ToString(), hint))!, out endpoint)) continue;
        return endpoint;
    }

    [return: NotNullIfNotNull(nameof(def))]
    public static async Task<Uri?> GetUriAsync(this string[] args, string name, Uri? def = null, string? hint = null)
    {
        Uri? uri;
        while (!TryParse((await args.GetArgumentAsync(name, def?.ToString(), hint))!, out uri)) continue;
        return uri ?? def;
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

}

class RoomServer : RoomHub
{

    protected override async ValueTask OnReceiveAsync(IRoomStream stream, RoomMessage message, CancellationToken token)
    {
        var clone = new MemoryStream((int)message.Content.Length);
        await message.Content.CopyToAsync(clone);
        Console.WriteLine($"[{stream.GetHashCode()}] {message.Verb} {message.Channel} {Encoding.UTF8.GetString(clone.ToArray())}");
        message.Content.Seek(0, SeekOrigin.Begin);
        await base.OnReceiveAsync(stream, message, token);
    }

    protected override void OnStart()
    {
        base.OnStart();
        Console.WriteLine($"Server Started");
    }

    protected override void OnStop()
    {
        base.OnStop();
        Console.WriteLine($"Server Stopped");
    }

    public RoomServer(RoomServiceOptions? options = null) : base(options) { }

}

class RoomClient : RoomService
{

    protected override async ValueTask OnReceiveAsync(IRoomStream stream, RoomMessage message, CancellationToken token)
    {
        var clone = new MemoryStream((int)message.Content.Length);
        await message.Content.CopyToAsync(clone);
        Console.WriteLine($"[{stream.GetHashCode()}] {message.Verb} {message.Channel} {Encoding.UTF8.GetString(clone.ToArray())}");
    }

    protected override void OnStart()
    {
        base.OnStart();
        Console.WriteLine($"Client Started");
    }

    protected override void OnStop()
    {
        base.OnStop();
        Console.WriteLine($"Client Stopped");
    }

    public RoomClient(RoomServiceOptions? options = null) : base(options) { }

}