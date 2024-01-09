using System.Collections.Concurrent;

namespace KolibSoft.Rooms.Core;

public class RoomHub
{

    public RoomSocket[] Sockets { get; private set; } = Array.Empty<RoomSocket>();
    public ConcurrentQueue<(RoomSocket, RoomMessage)> Messages { get; } = new();

    public async Task ListenAsync(RoomSocket socket, byte[]? pass = null)
    {
        Sockets = Sockets.Append(socket).ToArray();
        while (socket.IsAlive)
        {
            var message = await socket.ReceiveAsync();
            if (message != null)
                Messages.Enqueue((socket, message));
        }
        Sockets = Sockets.Where(x => x != socket).ToArray();
    }

    public async Task TransmitAsync()
    {
        while (Sockets.Any())
        {
            while (Messages.TryDequeue(out (RoomSocket source, RoomMessage message) message))
                foreach (var socket in Sockets)
                    try
                    {
                        var channel = message.source.GetHashCode() ^ socket.GetHashCode();
                        message.message.Channel = RoomChannel.Parse($"{channel:D8}");
                        await socket.SendAsync(message.message);
                    }
                    catch { }
            await Task.Delay(100);
        }
    }

}