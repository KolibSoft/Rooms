using System.Net.WebSockets;
using KolibSoft.Rooms.Core;

var code = GetArg(0, "Enter Room Code (random): ")?.Substring(0, 8);
if (string.IsNullOrEmpty(code)) code = Random.Shared.Next().ToString().Substring(0, 8);

var slots = GetArg(1, "Enter Room Slots (4): ");
var pass = GetArg(2, "Enter Room Pass (optional): ");
var tag = GetArg(2, "Enter Room Tag (optional): ");
Console.WriteLine($"Room Code: {code}");

var client = new ClientWebSocket();
client.Options.AddSubProtocol(RoomSocket.Protocol);
await client.ConnectAsync(new Uri($"wss://krooms.azurewebsites.net/api/rooms/join?code={code}&slots={slots ?? ""}&pass={pass ?? ""}&tag={tag ?? ""}"), CancellationToken.None);
var socket = new RoomSocket(client);

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