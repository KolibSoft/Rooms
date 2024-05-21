
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using KolibSoft.Rooms.Core.Protocol;
using KolibSoft.Rooms.Core.Services;
using KolibSoft.Rooms.Core.Streams;

var mode = await args.GetOptionAsync("mode", ["Server", "Client"]);
var impl = await args.GetOptionAsync("impl", ["TCP", "WEB"]);
var path = await args.GetArgumentAsync("settings", "settings.json");

Settings? settings = null;
if (File.Exists(path))
{
    var json = await File.ReadAllTextAsync(path);
    settings = JsonSerializer.Deserialize<Settings>(json);
}

if (mode == "Server")
{
    if (impl == "TCP")
    {
        var endpoint = await args.GetIPEndpointAsync("endpoint", new IPEndPoint(IPAddress.Any, 55000));
        Console.WriteLine($"Using endpoint: {endpoint}");
        using var server = new RoomServer(settings?.ServiceOptions) { Logger = Console.Error };
        server.Start();
        using var listener = new TcpListener(endpoint!);
        CommandServer(server);
        await ListenTcpAsync(server, listener, settings);
    }
    else if (impl == "WEB")
    {
        var uri = await args.GetUriAsync("uri", new Uri("http://localhost:55000/"));
        Console.WriteLine($"Using uri: {uri}");
        using var server = new RoomServer(settings?.ServiceOptions) { Logger = Console.Error };
        server.Start();
        using var listener = new HttpListener();
        listener.Prefixes.Add(uri!.ToString());
        CommandServer(server);
        await ListenWebAsync(server, listener, settings);
    }
}
else if (mode == "Client")
{
    if (impl == "TCP")
    {
        var endpoint = await args.GetIPEndpointAsync("endpoint", new IPEndPoint(IPAddress.Loopback, 55000));
        Console.WriteLine($"Using endpoint: {endpoint}");
        using var client = new RoomClient(settings?.ServiceOptions) { Logger = Console.Error };
        client.Start();
        using var _client = new TcpClient();
        await _client.ConnectAsync(endpoint!);
        using var stream = new RoomNetworkStream(_client, settings?.StreamOptions);
        await ClientHandshake(client, stream, settings?.ConnectionOptions);
        CommandClient(client);
        await client.ListenAsync(stream);
    }
    else if (impl == "WEB")
    {
        var uri = await args.GetUriAsync("uri", new Uri("ws://localhost:55000/"));
        Console.WriteLine($"Using uri: {uri}");
        using var client = new RoomClient(settings?.ServiceOptions) { Logger = Console.Error };
        client.Start();
        using var _client = new ClientWebSocket();
        await _client.ConnectAsync(uri!, default);
        using var stream = new RoomWebStream(_client, settings?.StreamOptions);
        await ClientHandshake(client, stream, settings?.ConnectionOptions);
        CommandClient(client);
        await client.ListenAsync(stream);
    }
}

return;

static async Task ListenTcpAsync(RoomServer server, TcpListener listener, Settings? settings = null)
{
    listener.Start();
    while (server.IsRunning)
        try
        {
            var client = await listener.AcceptTcpClientAsync();
            var stream = new RoomNetworkStream(client, settings?.StreamOptions);
            await ServerHandshake(server, stream, settings?.ConnectionOptions);
            _ = server.ListenAsync(stream);
        }
        catch (Exception error)
        {
            await Console.Error.WriteLineAsync($"Error listening connection: {error}");
        }
    listener.Stop();
}

static async Task ListenWebAsync(RoomServer server, HttpListener listener, Settings? settings = null)
{
    listener.Start();
    while (server.IsRunning)
        try
        {
            var httpContext = await listener.GetContextAsync();
            var wsContext = await httpContext.AcceptWebSocketAsync(null);
            var socket = wsContext.WebSocket;
            var stream = new RoomWebStream(socket, settings?.StreamOptions);
            await ServerHandshake(server, stream, settings?.ConnectionOptions);
            _ = server.ListenAsync(stream);
        }
        catch (Exception error)
        {
            await Console.Error.WriteLineAsync($"Error listening connection: {error}");
        }
    listener.Stop();
}

static async Task ServerHandshake(RoomServer server, IRoomStream stream, ConnectionOptions? options = null)
{
    Console.WriteLine($"[{stream.GetHashCode()}] Configuring connection");
    var message = await stream.ReadMessageAsync();
    if (message.Verb != "OPTIONS")
    {
        await stream.WriteMessageAsync(new RoomMessage
        {
            Verb = "INFO",
            Content = await RoomContentUtils.CreateAsTextAsync("Stream Options Required")
        });
        throw new InvalidOperationException("Stream Options Required");
    }
    var _options = await message.Content.ReadAsJsonAsync<ConnectionOptions>();
    await stream.WriteMessageAsync(new RoomMessage
    {
        Verb = "OPTIONS",
        Content = await RoomContentUtils.CreateAsJsonAsync(_options)
    });
    Console.WriteLine($"[{stream.GetHashCode()}] Connection configured");
}

static async Task ClientHandshake(RoomClient client, IRoomStream stream, ConnectionOptions? options = null)
{
    Console.WriteLine($"Configuring connection");
    await stream.WriteMessageAsync(new RoomMessage
    {
        Verb = "OPTIONS",
        Content = await RoomContentUtils.CreateAsJsonAsync(options)
    });
    var message = await stream.ReadMessageAsync();
    if (message.Verb != "OPTIONS")
    {
        Console.WriteLine($"[{message.Channel}] {message.Verb}: {await message.Content.ReadAsTextAsync()}");
        throw new InvalidOperationException("Stream Options Required");
    }
    var _options = await message.Content.ReadAsJsonAsync<ConnectionOptions>();
    Console.WriteLine($"Connection configured");
}

async void CommandServer(RoomServer server)
{
    while (server.IsRunning)
    {
        var command = await Task.Run(() => Console.ReadLine());
        try
        {
            var parts = command!.Split(" ");
            var message = new RoomMessage
            {
                Verb = parts[0],
                Channel = int.Parse(parts[1]),
                Content = new MemoryStream(Encoding.UTF8.GetBytes(string.Join(' ', parts.AsSpan().Slice(2).ToArray())))
            };
            server.Send(message);
        }
        catch (Exception error)
        {
            await Console.Error.WriteLineAsync($"Error parsing command: {error}");
        }
        await Task.Delay(100);
    }
}

async void CommandClient(RoomClient client)
{
    while (client.IsRunning)
    {
        var command = await Task.Run(() => Console.ReadLine());
        try
        {
            var parts = command!.Split(" ");
            var message = new RoomMessage
            {
                Verb = parts[0],
                Channel = int.Parse(parts[1]),
                Content = new MemoryStream(Encoding.UTF8.GetBytes(string.Join(' ', parts.AsSpan().Slice(2).ToArray())))
            };
            client.Send(message);
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

    public void Send(RoomMessage message)
    {
        if (message.Channel == -1)
        {
            message.Channel = 0;
            foreach (var stream in Streams)
                Enqueue(stream, message);
        }
        else
        {
            var target = Streams.FirstOrDefault(x => x.GetHashCode() == message.Channel);
            if (target != null)
            {
                message.Channel = 0;
                Enqueue(target, message);
            }
        }
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
        Console.WriteLine($"[{message.Channel}] {message.Verb}: {await message.Content.ReadAsTextAsync(null, token)}");
    }

    public override async ValueTask ListenAsync(IRoomStream stream, CancellationToken token = default)
    {
        if (_stream != null) throw new InvalidOperationException("Stream already listening");
        _stream = stream;
        await base.ListenAsync(stream, token);
        _stream = null;
    }

    public override void Enqueue(IRoomStream stream, RoomMessage message)
    {
        if (_stream != stream) throw new InvalidOperationException("Stream already listening");
        base.Enqueue(stream, message);
    }

    public void Send(RoomMessage message)
    {
        if (_stream == null) throw new InvalidOperationException("Stream no listening");
        Enqueue(_stream, message);
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

    private IRoomStream? _stream;

}

public class ConnectionOptions
{

}

public class Settings
{
    public RoomStreamOptions? StreamOptions { get; set; }
    public RoomServiceOptions? ServiceOptions { get; set; }
    public ConnectionOptions? ConnectionOptions { get; set; }
}