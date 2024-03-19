using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
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
        public ImmutableArray<IRoomSocket> Sockets { get; private set; } = ImmutableArray<IRoomSocket>.Empty;

        /// <summary>
        /// The messages to send with its author.
        /// </summary>
        public ConcurrentQueue<RoomContext> Messages { get; } = new();

        /// <summary>
        /// Log writer
        /// </summary>
        public TextWriter? LogWriter { get; set; }

        /// <summary>
        /// Starts to receive the incoming socket messages while it is alive. Use a delay of 100ms between messages.
        /// </summary>
        /// <param name="socket">A connected socket.</param>
        /// <param name="rateLimit">Max amount of bytes to read per second.</param>
        /// <returns></returns>
        public async Task ListenAsync(IRoomSocket socket, int rateLimit = 1024)
        {
            Sockets = Sockets.Add(socket);
            int bytes = 0;
            var ttl = TimeSpan.FromSeconds(1);
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            while (socket.IsAlive)
            {
                try
                {
                    var message = await socket.ReceiveAsync();
                    bytes += message.Length;
                    if (bytes > rateLimit)
                    {
                        socket.Dispose();
                        break;
                    }
                    if (stopwatch.Elapsed > ttl)
                    {
                        bytes = 0;
                        stopwatch.Restart();
                    }
                    Messages.Enqueue(new RoomContext(socket, message));
                }
                catch (Exception e)
                {
                    var writer = LogWriter;
                    if (writer != null) await writer.WriteLineAsync($"Room Hub exception: {e.Message}\n{e.StackTrace}");
                }
            }
            stopwatch.Stop();
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
                while (Messages.TryDequeue(out RoomContext? context))
                {
                    var author = context.Socket;
                    var message = context.Message;
                    if (author.IsAlive && message.Channel == RoomChannel.Loopback)
                        try
                        {
                            await author.SendAsync(message);
                        }
                        catch (Exception e)
                        {
                            var writer = LogWriter;
                            if (writer != null) await writer.WriteLineAsync($"Room Hub exception: {e.Message}\n{e.StackTrace}");
                        }
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
                                catch (Exception e)
                                {
                                    var writer = LogWriter;
                                    if (writer != null) await writer.WriteLineAsync($"Room Hub exception: {e.Message}\n{e.StackTrace}");
                                }
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
                            catch (Exception e)
                            {
                                var writer = LogWriter;
                                if (writer != null) await writer.WriteLineAsync($"Room Hub exception: {e.Message}\n{e.StackTrace}");
                            }
                    }

                }
            }
        }

    }

}