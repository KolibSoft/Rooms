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
            while (Messages.TryDequeue(out (RoomSocket, RoomMessage) msg))
            {
                (RoomSocket author, RoomMessage message) = msg;
                if (message.Channel == RoomChannel.Loopback)
                    await author.SendAsync(message);
                else if (message.Channel == RoomChannel.Broadcast)
                {
                    var ochannel = message.Channel;
                    var hash = author.GetHashCode();
                    foreach (var socket in Sockets)
                        if (socket != author)
                        {
                            var channel = RoomChannel.Parse($"{hash ^ socket.GetHashCode():x8}");
                            message.Channel = channel;
                            try
                            {
                                await socket.SendAsync(message);
                            }
                            catch { }
                        }
                    message.Channel = ochannel;
                }
                else
                {
                    var hash = author.GetHashCode();
                    var target = Convert.ToInt32(message.Channel.ToString(), 16) ^ hash;
                    var socket = Sockets.FirstOrDefault(x => x.GetHashCode() == target);
                    if (socket != null)
                        await socket.SendAsync(message);
                }

            }
            await Task.Delay(100);
        }
    }

}