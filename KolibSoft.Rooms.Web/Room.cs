using System.Collections.Concurrent;
using Microsoft.AspNetCore.StaticFiles.Infrastructure;

namespace KolibSoft.Rooms.Web;

public class Room
{

    public RoomSocket[] Sockets { get; private set; } = Array.Empty<RoomSocket>();
    public ConcurrentQueue<(int id, RoomMessage message)> Messages { get; } = new();

    public async Task Listen(RoomSocket socket)
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

    public async Task Transmit()
    {
        while (Sockets.Any())
        {
            while (Messages.TryDequeue(out (int id, RoomMessage message) message))
                foreach (var socket in Sockets)
                    try
                    {
                        message.message.Headers["ID"] = $"{message.id ^ socket.GetHashCode():D8}";
                        await socket.SendAsync(message.message);
                    }
                    catch { }
            await Task.Delay(TimeSpan.FromMilliseconds(1));
        }
    }

    public async Task RunAsync()
    {
        while (true)
        {
            await Transmit();
            await Task.Delay(TimeSpan.FromMilliseconds(1));
        }
    }

    public static Room Shared { get; } = new();

    static Room()
    {
        _ = Shared.RunAsync();
    }

}