using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace KolibSoft.Rooms.Core
{

    /// <summary>
    /// Represents a connection point to sharing messages.
    /// </summary>
    public class RoomHub
    {

        /// <summary>
        /// The current hub participants.
        /// </summary>
        public ImmutableList<IRoomSocket> Sockets { get; private set; } = ImmutableList<IRoomSocket>.Empty;

        /// <summary>
        /// The messages to send with its author.
        /// </summary>
        public ConcurrentQueue<(IRoomSocket author, RoomMessage message)> Messages { get; } = new();

        /// <summary>
        /// Starts to receive the incoming socket messages while it is alive. Use a delay of 100ms between messages.
        /// </summary>
        /// <param name="socket">A conncted socket</param>
        /// <returns></returns>
        public async Task ListenAsync(IRoomSocket socket)
        {
            Sockets = Sockets.Add(socket);
            while (socket.IsAlive)
            {
                try
                {
                    var message = await socket.ReceiveAsync();
                    Messages.Enqueue((socket, message));
                }
                catch { }
                await Task.Delay(100);
            }
            Sockets = Sockets.Remove(socket);
        }

        /// <summary>
        /// Starts to route and send the received messages while there are participants.
        /// </summary>
        /// <returns></returns>
        public async Task TransmitAsync()
        {
            while (Sockets.Any())
            {
                while (Messages.TryDequeue(out (IRoomSocket, RoomMessage) msg))
                {
                    (IRoomSocket author, RoomMessage message) = msg;
                    if (author.IsAlive && message.Channel == RoomChannel.Loopback)
                        try
                        {
                            await author.SendAsync(message);
                        }
                        catch { }
                    else if (message.Channel == RoomChannel.Broadcast)
                    {
                        var ochannel = message.Channel;
                        var hash = author.GetHashCode();
                        foreach (var socket in Sockets)
                            if (socket != author && socket.IsAlive)
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
                        var target = message.Channel ^ hash;
                        var socket = Sockets.FirstOrDefault(x => x.GetHashCode() == target);
                        if (socket != null && socket.IsAlive)
                            try
                            {
                                await socket.SendAsync(message);
                            }
                            catch { }
                    }

                }
                await Task.Delay(100);
            }
        }

    }

}