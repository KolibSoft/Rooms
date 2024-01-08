using System.Collections.Concurrent;

namespace KolibSoft.Rooms.Core;

public class Room
{

    public RoomSocket[] Sockets { get; private set; } = Array.Empty<RoomSocket>();

    public ConcurrentQueue<(int id, RoomMessage)> Messages { get; } = new();

    public async Task ListenAsync(RoomSocket socket)
    {
        Sockets = Sockets.Append(socket).ToArray();
        while (socket.IsAlive)
        {
            var message = await socket.ReceiveAsync();
            if (message != null)
                Messages.Enqueue((socket.GetHashCode(), message));
        }
        Sockets = Sockets.Where(x => x != socket).ToArray();
    }

    public async Task TransmitAsync()
    {
        while (Sockets.Any())
        {
            if (Messages.TryDequeue(out (int id, RoomMessage message) message))
                foreach (var socket in Sockets)
                    try
                    {
                        var channel = message.id ^ socket.GetHashCode();
                        message.message.Channel = RoomChannel.Parse($"{channel:D8}");
                        await socket.SendAsync(message.message);
                    }
                    catch { }
            else await Task.Delay(100);
        }
    }

    public async Task RunAsync()
    {
        while (true)
        {
            await TransmitAsync();
            await Task.Delay(100);
        }
    }

}