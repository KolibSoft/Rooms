using System.Net.Sockets;
using System.Net.WebSockets;
using KolibSoft.Rooms.Core;

IRoomSocket socket = null!;
await UdpVersion();

_ = Task.Run(async () =>
{
    while (socket.IsAlive)
    {
        RoomMessage? message = null;
        try
        {
            message = await socket.ReceiveAsync();
        }
        catch { }
        if (message != null)
        {
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.WriteLine($"{message.Verb} [{message.Channel}] {message.Content}");
            Console.Write("> ");
        }
        await Task.Delay(100);
    }
});

while (socket.IsAlive)
{
    Console.Write("> ");
    var input = Console.ReadLine();
    if (input != null)
    {
        var @string = input.AsMemory();
        if (@string.Length < 13) { Console.WriteLine("Expected: <verb> <channel> <content>"); continue; }
        if (!RoomVerb.Verify(@string.Slice(0, 3).Span)) { Console.WriteLine("Expected a valid verb"); continue; }
        if (!RoomChannel.Verify(@string.Slice(4, 8).Span)) { Console.WriteLine("Expected a valid channel"); continue; }
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

async Task UdpVersion()
{

    var port = GetArg(0, "Enter Local Port (random): ");
    if (string.IsNullOrEmpty(port)) port = Random.Shared.Next().ToString().Substring(0, 8);

    var host = GetArg(0, "Enter Remote host: ")!;
    var remote = GetArg(0, "Enter Remote Port: ")!;

    var client = new UdpClient(int.Parse(port));
    client.Connect(host, int.Parse(remote));
    socket = new UdpRoomSocket(client);
}

async Task WebVersion()
{

    var code = GetArg(0, "Enter Room Code (random): ")?.Substring(0, 8);
    if (string.IsNullOrEmpty(code)) code = Random.Shared.Next().ToString().Substring(0, 8);

    var slots = GetArg(1, "Enter Room Slots (4): ");
    var pass = GetArg(2, "Enter Room Pass (optional): ");
    var tag = GetArg(2, "Enter Room Tag (optional): ");
    Console.WriteLine($"Room Code: {code}");

    var client = new ClientWebSocket();
    client.Options.AddSubProtocol(WebRoomSocket.Protocol);
    await client.ConnectAsync(new Uri($"wss://krooms.azurewebsites.net/api/rooms/join?code={code}&slots={slots ?? ""}&pass={pass ?? ""}&tag={tag ?? ""}"), CancellationToken.None);
    socket = new WebRoomSocket(client);
}

string? Prompt(string hint)
{
    Console.Write(hint);
    var input = Console.ReadLine();
    return input;
}

string? GetArg(int index, string? hint)
{
    string? arg = null;
    if (args.Length > index)
    {
        arg = args[index];
        return arg;
    }
    arg = Prompt(hint ?? "");
    return arg;
}